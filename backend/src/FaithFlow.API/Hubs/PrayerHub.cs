using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FaithFlow.API.Hubs;

[Authorize]
public class PrayerHub : Hub
{
    private readonly ILogger<PrayerHub> _logger;

    public PrayerHub(ILogger<PrayerHub> logger) => _logger = logger;

    public async Task JoinPrayerWall()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "PrayerWall");
        _logger.LogInformation("User {UserId} joined prayer wall", Context.UserIdentifier);
    }

    public async Task LeavePrayerWall()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "PrayerWall");
        _logger.LogInformation("User {UserId} left prayer wall", Context.UserIdentifier);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
