using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using AquaControl.Application.Common.Interfaces;

namespace AquaControl.API.Hubs;

[Authorize]
public class TankDataHub : Hub
{
    private readonly ILogger<TankDataHub> _logger;
    private readonly ITankRepository _tankRepository;

    public TankDataHub(ILogger<TankDataHub> logger, ITankRepository tankRepository)
    {
        _logger = logger;
        _tankRepository = tankRepository;
    }

    public async Task JoinTankGroup(string tankId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tank_{tankId}");
        _logger.LogInformation("User {UserId} joined tank group {TankId}", Context.UserIdentifier, tankId);
    }

    public async Task LeaveTankGroup(string tankId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tank_{tankId}");
        _logger.LogInformation("User {UserId} left tank group {TankId}", Context.UserIdentifier, tankId);
    }

    public async Task JoinAllTanksGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AllTanks");
        _logger.LogInformation("User {UserId} joined all tanks group", Context.UserIdentifier);
    }

    public async Task LeaveAllTanksGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllTanks");
        _logger.LogInformation("User {UserId} left all tanks group", Context.UserIdentifier);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to TankDataHub", Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from TankDataHub", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}

// SignalR service for sending updates
public interface ISignalRService
{
    Task SendTankUpdateAsync(string tankId, object data);
    Task SendSensorReadingAsync(string tankId, object reading);
    Task SendAlertAsync(string tankId, object alert);
    Task SendSystemNotificationAsync(object notification);
}

public class SignalRService : ISignalRService
{
    private readonly IHubContext<TankDataHub> _hubContext;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(IHubContext<TankDataHub> hubContext, ILogger<SignalRService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendTankUpdateAsync(string tankId, object data)
    {
        try
        {
            await _hubContext.Clients.Group($"Tank_{tankId}").SendAsync("TankUpdated", data);
            _logger.LogDebug("Sent tank update for tank {TankId}", tankId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending tank update for tank {TankId}", tankId);
        }
    }

    public async Task SendSensorReadingAsync(string tankId, object reading)
    {
        try
        {
            await _hubContext.Clients.Group($"Tank_{tankId}").SendAsync("SensorReading", reading);
            _logger.LogDebug("Sent sensor reading for tank {TankId}", tankId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending sensor reading for tank {TankId}", tankId);
        }
    }

    public async Task SendAlertAsync(string tankId, object alert)
    {
        try
        {
            await _hubContext.Clients.Group($"Tank_{tankId}").SendAsync("Alert", alert);
            await _hubContext.Clients.Group("AllTanks").SendAsync("Alert", alert);
            _logger.LogInformation("Sent alert for tank {TankId}", tankId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert for tank {TankId}", tankId);
        }
    }

    public async Task SendSystemNotificationAsync(object notification)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("SystemNotification", notification);
            _logger.LogInformation("Sent system notification");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system notification");
        }
    }
}

