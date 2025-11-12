using MediatR;
using AquaControl.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.EventHandlers;

public sealed class TankCreatedEventHandler : INotificationHandler<TankCreatedEvent>
{
    private readonly ILogger<TankCreatedEventHandler> _logger;

    public TankCreatedEventHandler(ILogger<TankCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TankCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tank created: {TankId} - {TankName} at {Location}",
            notification.TankId,
            notification.Name,
            notification.Location.GetFullAddress());

        // Additional business logic:
        // - Send notifications
        // - Update read models
        // - Trigger integrations
        // - Audit logging

        await Task.CompletedTask;
    }
}

