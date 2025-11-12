using MediatR;
using Microsoft.EntityFrameworkCore;
using AquaControl.Domain.Events;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.ReadModels.Models;

namespace AquaControl.Infrastructure.ReadModels.Projections;

public sealed class TankProjectionHandler :
    INotificationHandler<TankCreatedEvent>,
    INotificationHandler<TankNameChangedEvent>,
    INotificationHandler<TankCapacityChangedEvent>,
    INotificationHandler<TankRelocatedEvent>,
    INotificationHandler<TankActivatedEvent>,
    INotificationHandler<TankDeactivatedEvent>
{
    private readonly ReadModelDbContext _context;
    private readonly ILogger<TankProjectionHandler> _logger;

    public TankProjectionHandler(ReadModelDbContext context, ILogger<TankProjectionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(TankCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Projecting TankCreatedEvent for tank {TankId}", notification.TankId);

        var tankReadModel = new TankReadModel
        {
            Id = notification.TankId.Value,
            Name = notification.Name,
            Capacity = notification.Capacity.Value,
            CapacityUnit = notification.Capacity.Unit,
            Building = notification.Location.Building,
            Room = notification.Location.Room,
            Zone = notification.Location.Zone,
            Latitude = notification.Location.Latitude,
            Longitude = notification.Location.Longitude,
            TankType = notification.TankType.ToString(),
            Status = "Inactive", // Default status
            CreatedAt = notification.OccurredOn,
            Version = 1
        };

        _context.Tanks.Add(tankReadModel);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tank read model created for {TankId}", notification.TankId);
    }

    public async Task Handle(TankNameChangedEvent notification, CancellationToken cancellationToken)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == notification.TankId.Value, cancellationToken);

        if (tank != null)
        {
            tank.Name = notification.NewName;
            tank.UpdatedAt = notification.OccurredOn;
            tank.Version++;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Tank name updated in read model: {TankId}", notification.TankId);
        }
    }

    public async Task Handle(TankCapacityChangedEvent notification, CancellationToken cancellationToken)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == notification.TankId.Value, cancellationToken);

        if (tank != null)
        {
            tank.Capacity = notification.NewCapacity.Value;
            tank.CapacityUnit = notification.NewCapacity.Unit;
            tank.UpdatedAt = notification.OccurredOn;
            tank.Version++;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Tank capacity updated in read model: {TankId}", notification.TankId);
        }
    }

    public async Task Handle(TankRelocatedEvent notification, CancellationToken cancellationToken)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == notification.TankId.Value, cancellationToken);

        if (tank != null)
        {
            tank.Building = notification.NewLocation.Building;
            tank.Room = notification.NewLocation.Room;
            tank.Zone = notification.NewLocation.Zone;
            tank.Latitude = notification.NewLocation.Latitude;
            tank.Longitude = notification.NewLocation.Longitude;
            tank.UpdatedAt = notification.OccurredOn;
            tank.Version++;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Tank location updated in read model: {TankId}", notification.TankId);
        }
    }

    public async Task Handle(TankActivatedEvent notification, CancellationToken cancellationToken)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == notification.TankId.Value, cancellationToken);

        if (tank != null)
        {
            tank.Status = "Active";
            tank.UpdatedAt = notification.OccurredOn;
            tank.Version++;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Tank activated in read model: {TankId}", notification.TankId);
        }
    }

    public async Task Handle(TankDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == notification.TankId.Value, cancellationToken);

        if (tank != null)
        {
            tank.Status = "Inactive";
            tank.UpdatedAt = notification.OccurredOn;
            tank.Version++;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Tank deactivated in read model: {TankId}", notification.TankId);
        }
    }
}

