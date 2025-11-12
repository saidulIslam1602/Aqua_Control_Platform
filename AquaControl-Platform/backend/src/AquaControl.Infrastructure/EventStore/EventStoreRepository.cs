using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AquaControl.Domain.Common;
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

        _logger.LogDebug("Saving {EventCount} events for aggregate {AggregateId}", 
            eventsList.Count, aggregateId);

        // Check for concurrency conflicts
        var currentVersion = await GetCurrentVersionAsync(aggregateId, cancellationToken);
        if (currentVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Concurrency conflict for aggregate {aggregateId}. Expected version {expectedVersion}, but current version is {currentVersion}");
        }

        var storedEvents = new List<StoredEvent>();
        var version = expectedVersion;

        foreach (var domainEvent in eventsList)
        {
            version++;
            
            var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions);
            var metadata = JsonSerializer.Serialize(new
            {
                EventId = domainEvent.EventId,
                OccurredOn = domainEvent.OccurredOn,
                EventVersion = "1.0",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            }, _jsonOptions);

            var storedEvent = new StoredEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.EventType,
                EventData = eventData,
                Metadata = metadata,
                Version = version,
                Timestamp = domainEvent.OccurredOn
            };

            storedEvents.Add(storedEvent);
        }

        _context.Events.AddRange(storedEvents);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId} up to version {Version}",
            eventsList.Count, aggregateId, version);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(
        Guid aggregateId,
        long fromVersion = 0,
        CancellationToken cancellationToken = default)
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
            var eventType = Type.GetType(storedEvent.EventType);
            if (eventType == null)
            {
                _logger.LogWarning("Could not find type {EventType} for event {EventId}",
                    storedEvent.EventType, storedEvent.Id);
                continue;
            }

            var domainEvent = JsonSerializer.Deserialize(storedEvent.EventData, eventType, _jsonOptions) as IDomainEvent;
            if (domainEvent != null)
            {
                domainEvents.Add(domainEvent);
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
                AggregateType = typeof(T).AssemblyQualifiedName!,
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

        var aggregate = JsonSerializer.Deserialize<T>(snapshot.Data, _jsonOptions);
        
        _logger.LogDebug("Loaded snapshot for aggregate {AggregateId} at version {Version}",
            aggregateId, snapshot.Version);

        return aggregate;
    }

    private async Task<long> GetCurrentVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        var lastEvent = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return lastEvent?.Version ?? 0;
    }
}

