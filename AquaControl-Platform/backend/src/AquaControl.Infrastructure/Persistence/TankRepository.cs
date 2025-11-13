using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Common;
using AquaControl.Domain.Enums;
using AquaControl.Domain.ValueObjects;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.ReadModels;

namespace AquaControl.Infrastructure.Persistence;

public sealed class TankRepository : ITankRepository
{
    private readonly IEventStore _eventStore;
    private readonly ReadModelDbContext _readContext;
    private readonly ILogger<TankRepository> _logger;

    public TankRepository(
        IEventStore eventStore,
        ReadModelDbContext readContext,
        ILogger<TankRepository> logger)
    {
        _eventStore = eventStore;
        _readContext = readContext;
        _logger = logger;
    }

    public async Task<Tank?> GetByIdAsync(TankId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading tank aggregate {TankId}", id.Value);

        // Try to get snapshot first
        var tank = await _eventStore.GetSnapshotAsync<Tank>(id.Value, cancellationToken);
        var fromVersion = tank?.Version ?? 0;

        // Load events since snapshot
        var events = await _eventStore.GetEventsAsync(id.Value, fromVersion, cancellationToken);
        
        if (tank == null && !events.Any())
        {
            return null;
        }

        // If no snapshot, create tank from first event (TankCreatedEvent)
        if (tank == null && events.Any())
        {
            var firstEvent = events.First();
            if (firstEvent is Domain.Events.TankCreatedEvent createdEvent)
            {
                // Create tank instance for event replay
                tank = Tank.FromEventReplay(id.Value);
                // Apply the creation event to set initial state
                tank.When(createdEvent);
                // Skip the first event in the replay loop since we've already applied it
                events = events.Skip(1);
            }
            else
            {
                _logger.LogWarning("First event for tank {TankId} is not TankCreatedEvent, cannot reconstruct", id.Value);
                return null;
            }
        }

        if (tank == null)
        {
            return null;
        }

        // Replay all remaining events to rebuild current state
        foreach (var domainEvent in events)
        {
            ApplyEventToAggregate(tank, domainEvent);
        }

        // Save snapshot if many events have been applied (optimization)
        var totalEvents = fromVersion + events.Count();
        if (totalEvents > 10 && totalEvents % 10 == 0)
        {
            await _eventStore.SaveSnapshotAsync(tank, cancellationToken);
        }

        return tank;
    }

    public async Task<Tank?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // For queries like this, we use read models for performance
        var tankReadModel = await _readContext.Tanks
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

        if (tankReadModel == null) return null;

        return await GetByIdAsync(TankId.Create(tankReadModel.Id), cancellationToken);
    }

    public async Task<(IEnumerable<Tank> Tanks, int TotalCount)> GetPagedAsync(
        ISpecification<Tank> specification,
        int page,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Use read models for efficient querying
        var query = _readContext.Tanks.AsQueryable();

        // Apply basic filtering (simplified - in production, you'd convert specification to read model query)
        var totalCount = await query.CountAsync(cancellationToken);
        
        var skip = (page - 1) * pageSize;
        var tankReadModels = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Convert read models to aggregates
        var tasks = tankReadModels.Select(rm => GetByIdAsync(TankId.Create(rm.Id), cancellationToken));
        var tanks = await Task.WhenAll(tasks);

        return (tanks.Where(t => t != null).Cast<Tank>(), totalCount);
    }

    public async Task AddAsync(Tank tank, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding tank aggregate {TankId}", tank.Id);

        var events = tank.DomainEvents;
        await _eventStore.SaveEventsAsync<Tank>(tank.Id, events, 0, cancellationToken);
        
        tank.ClearDomainEvents();
    }

    public async Task UpdateAsync(Tank tank, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating tank aggregate {TankId}", tank.Id);

        var events = tank.DomainEvents;
        if (!events.Any()) return;

        await _eventStore.SaveEventsAsync<Tank>(tank.Id, events, tank.Version - events.Count, cancellationToken);
        
        tank.ClearDomainEvents();
    }

    public async Task DeleteAsync(Tank tank, CancellationToken cancellationToken = default)
    {
        // In event sourcing, we don't delete - we add a "deleted" event
        // This is handled by the aggregate's business logic
        await UpdateAsync(tank, cancellationToken);
    }

    /// <summary>
    /// Applies a domain event to the tank aggregate to rebuild its state.
    /// This method is called during event sourcing replay to reconstruct aggregate state from events.
    /// </summary>
    /// <param name="tank">The tank aggregate to apply the event to.</param>
    /// <param name="domainEvent">The domain event to apply.</param>
    private void ApplyEventToAggregate(Tank tank, IDomainEvent domainEvent)
    {
        try
        {
            _logger.LogDebug("Applying event {EventType} to tank {TankId} for event replay", 
                domainEvent.EventType, tank.Id);
            
            // Call the When() method on the aggregate to apply the event
            // This rebuilds the aggregate state without raising new events
            tank.When(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply event {EventType} to tank {TankId} during replay",
                domainEvent.EventType, tank.Id);
            throw;
        }
    }
}

