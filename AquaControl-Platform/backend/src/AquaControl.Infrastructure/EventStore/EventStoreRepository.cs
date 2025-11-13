using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Reflection;
using AquaControl.Domain.Common;
using AquaControl.Domain.Events;
using AquaControl.Infrastructure.EventStore.Models;

namespace AquaControl.Infrastructure.EventStore;

public interface IEventStore
{
    Task SaveEventsAsync<T>(Guid aggregateId, IEnumerable<IDomainEvent> events, long expectedVersion, CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>;
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, long fromVersion = 0, CancellationToken cancellationToken = default);
    Task SaveSnapshotAsync<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>;
    Task<T?> GetSnapshotAsync<T>(Guid aggregateId, CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>;
}

public sealed class EventStoreRepository : IEventStore
{
    private readonly EventStoreDbContext _context;
    private readonly ILogger<EventStoreRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly Dictionary<string, Type> EventTypeMap = new();

    static EventStoreRepository()
    {
        // Build event type map from all domain events
        var domainEventAssembly = Assembly.GetAssembly(typeof(DomainEvent));
        if (domainEventAssembly == null) return;

        var eventTypes = domainEventAssembly
            .GetTypes()
            .Where(t => 
                (t.IsClass || t.IsValueType) && 
                !t.IsAbstract && 
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                t != typeof(IDomainEvent) &&
                t != typeof(DomainEvent))
            .ToList();

        foreach (var eventType in eventTypes)
        {
            // Use the type name as the key (events use nameof(EventType) which matches the class name)
            EventTypeMap[eventType.Name] = eventType;
        }
    }

    public EventStoreRepository(EventStoreDbContext context, ILogger<EventStoreRepository> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveEventsAsync<T>(
        Guid aggregateId,
        IEnumerable<IDomainEvent> events,
        long expectedVersion,
        CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>
    {
        var eventsList = events.ToList();
        if (!eventsList.Any()) return;

        _logger.LogDebug("Saving {EventCount} events for aggregate {AggregateId} with expected version {ExpectedVersion}", 
            eventsList.Count, aggregateId, expectedVersion);

        // Check optimistic concurrency: verify that the current version matches expected version
        var currentVersion = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .Select(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentVersion != expectedVersion)
        {
            _logger.LogWarning(
                "Concurrency conflict for aggregate {AggregateId}: expected version {ExpectedVersion}, but current version is {CurrentVersion}",
                aggregateId, expectedVersion, currentVersion);
            throw new InvalidOperationException(
                $"Concurrency conflict: Expected version {expectedVersion}, but current version is {currentVersion}. " +
                "The aggregate has been modified by another process. Please reload and try again.");
        }

        var storedEvents = eventsList.Select((domainEvent, index) => new StoredEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            EventType = domainEvent.EventType,
            EventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions),
            Version = expectedVersion + index + 1,
            Timestamp = domainEvent.OccurredOn
        });

        _context.Events.AddRange(storedEvents);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId} at version {NewVersion}",
            eventsList.Count, aggregateId, expectedVersion + eventsList.Count);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, long fromVersion = 0, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading events for aggregate {AggregateId} from version {FromVersion}",
            aggregateId, fromVersion);

        var storedEvents = await _context.Events
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        var domainEvents = new List<IDomainEvent>();

        foreach (var storedEvent in storedEvents)
        {
            try
            {
                // Try to find the event type in our map
                if (!EventTypeMap.TryGetValue(storedEvent.EventType, out var eventType))
                {
                    _logger.LogWarning("Unknown event type {EventType} for event {EventId}, skipping",
                        storedEvent.EventType, storedEvent.Id);
                    continue;
                }

                // Deserialize the event to its proper type
                var domainEvent = JsonSerializer.Deserialize(storedEvent.EventData, eventType, _jsonOptions);
                
                if (domainEvent is IDomainEvent typedEvent)
                {
                    domainEvents.Add(typedEvent);
                }
                else
                {
                    _logger.LogWarning("Deserialized event {EventId} is not an IDomainEvent",
                        storedEvent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize event {EventId} of type {EventType}",
                    storedEvent.Id, storedEvent.EventType);
            }
        }

        _logger.LogDebug("Loaded {EventCount} events for aggregate {AggregateId}",
            domainEvents.Count, aggregateId);

        return domainEvents;
    }

    public async Task SaveSnapshotAsync<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>
    {
        _logger.LogDebug("Saving snapshot for aggregate {AggregateId} at version {Version}",
            aggregate.Id, aggregate.Version);

        var data = JsonSerializer.Serialize(aggregate, typeof(T), _jsonOptions);

        var existingSnapshot = await _context.Snapshots
            .FirstOrDefaultAsync(s => s.AggregateId == aggregate.Id, cancellationToken);

        if (existingSnapshot != null)
        {
            existingSnapshot.Data = data;
            existingSnapshot.Version = aggregate.Version;
            existingSnapshot.Timestamp = DateTime.UtcNow;
        }
        else
        {
            var snapshot = new EventSnapshot
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregate.Id,
                AggregateType = typeof(T).Name,
                Data = data,
                Version = aggregate.Version,
                Timestamp = DateTime.UtcNow
            };

            _context.Snapshots.Add(snapshot);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved snapshot for aggregate {AggregateId} at version {Version}",
            aggregate.Id, aggregate.Version);
    }

    public async Task<T?> GetSnapshotAsync<T>(Guid aggregateId, CancellationToken cancellationToken = default)
        where T : AggregateRoot<Guid>
    {
        var snapshot = await _context.Snapshots
            .FirstOrDefaultAsync(s => s.AggregateId == aggregateId, cancellationToken);

        if (snapshot == null) return null;

        try
        {
            var aggregate = JsonSerializer.Deserialize<T>(snapshot.Data, _jsonOptions);
            
            _logger.LogDebug("Loaded snapshot for aggregate {AggregateId} at version {Version}",
                aggregateId, snapshot.Version);
            
            return aggregate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize snapshot for aggregate {AggregateId}",
                aggregateId);
            return null;
        }
    }
}