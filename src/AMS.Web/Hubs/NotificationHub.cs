using Microsoft.AspNetCore.SignalR;

namespace AMS.Web.Hubs;

/// <summary>
/// SignalR hub for real-time notifications.
/// </summary>
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationHub class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}, User: {User}",
            Context.ConnectionId, Context.User?.Identity?.Name ?? "Anonymous");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    /// <param name="exception">The exception if any.</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}, User: {User}, Exception: {Exception}",
            Context.ConnectionId, Context.User?.Identity?.Name ?? "Anonymous", exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Joins a user to a specific group for targeted notifications.
    /// </summary>
    /// <param name="groupName">The name of the group to join.</param>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {User} joined group {Group}",
            Context.User?.Identity?.Name ?? "Anonymous", groupName);
    }

    /// <summary>
    /// Leaves a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group to leave.</param>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {User} left group {Group}",
            Context.User?.Identity?.Name ?? "Anonymous", groupName);
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public async Task SendMessageToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", Context.User?.Identity?.Name ?? "System", message);
    }

    /// <summary>
    /// Sends a message to a specific user.
    /// </summary>
    /// <param name="userId">The user ID to send the message to.</param>
    /// <param name="message">The message to send.</param>
    public async Task SendMessageToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", Context.User?.Identity?.Name ?? "System", message);
    }

    /// <summary>
    /// Sends a message to a specific group.
    /// </summary>
    /// <param name="groupName">The group to send the message to.</param>
    /// <param name="message">The message to send.</param>
    public async Task SendMessageToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", Context.User?.Identity?.Name ?? "System", message);
    }
}
