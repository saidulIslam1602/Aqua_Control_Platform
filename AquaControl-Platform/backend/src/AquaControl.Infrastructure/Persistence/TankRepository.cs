using Microsoft.EntityFrameworkCore;
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

        // If no snapshot, we need to reconstruct from events
        // For now, we'll use a simplified approach - in production, you'd have proper event replay
        if (tank == null && events.Any())
        {
            // Get the first event to reconstruct initial state
            var firstEvent = events.First();
            if (firstEvent is Domain.Events.TankCreatedEvent createdEvent)
            {
                tank = Tank.Create(
                    createdEvent.Name,
                    createdEvent.Capacity,
                    createdEvent.Location,
                    createdEvent.TankType);
            }
        }

        if (tank == null)
        {
            return null;
        }

        // Apply remaining events to rebuild state
        // Note: This is simplified - in production, you'd have proper event application logic
        foreach (var domainEvent in events.Skip(1))
        {
            ApplyEventToAggregate(tank, domainEvent);
        }

        // Save snapshot if many events have been applied
        if (events.Count() > 10)
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
        _logger.LogDebug("Adding tank aggregate {TankId}", tank.Id.Value);

        var events = tank.DomainEvents;
        await _eventStore.SaveEventsAsync<Tank>(tank.Id.Value, events, 0, cancellationToken);
        
        tank.ClearDomainEvents();
    }

    public async Task UpdateAsync(Tank tank, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating tank aggregate {TankId}", tank.Id.Value);

        var events = tank.DomainEvents;
        if (!events.Any()) return;

        await _eventStore.SaveEventsAsync<Tank>(tank.Id.Value, events, tank.Version - events.Count, cancellationToken);
        
        tank.ClearDomainEvents();
    }

    public async Task DeleteAsync(Tank tank, CancellationToken cancellationToken = default)
    {
        // In event sourcing, we don't delete - we add a "deleted" event
        // This is handled by the aggregate's business logic
        await UpdateAsync(tank, cancellationToken);
    }

    private void ApplyEventToAggregate(Tank tank, IDomainEvent domainEvent)
    {
        // This is a simplified placeholder
        // In production, you'd have a proper event application mechanism
        // For now, we'll just log that we're applying the event
        _logger.LogDebug("Applying event {EventType} to tank {TankId}", 
            domainEvent.EventType, tank.Id.Value);
        
        // In a real implementation, you'd have event handlers that apply events to rebuild state
        // This would typically use reflection or a more sophisticated event sourcing framework
    }
}

