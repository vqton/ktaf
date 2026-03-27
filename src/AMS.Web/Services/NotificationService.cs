using Microsoft.AspNetCore.SignalR;
using AMS.Application.Interfaces;

namespace AMS.Web.Hubs;

/// <summary>
/// Service for sending notifications through SignalR.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="type">The notification type (info, warning, error, success).</param>
    Task SendToAllAsync(string message, string type = "info");

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="type">The notification type.</param>
    Task SendToUserAsync(string userId, string message, string type = "info");

    /// <summary>
    /// Sends a notification to a specific group.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="type">The notification type.</param>
    Task SendToGroupAsync(string groupName, string message, string type = "info");

    /// <summary>
    /// Sends a voucher approval notification.
    /// </summary>
    /// <param name="voucherId">The voucher ID.</param>
    /// <param name="voucherNumber">The voucher number.</param>
    /// <param name="status">The new status.</param>
    /// <param name="actionBy">The user who performed the action.</param>
    Task SendVoucherNotificationAsync(Guid voucherId, string voucherNumber, string status, string actionBy);

    /// <summary>
    /// Sends a system notification about scheduled job completion.
    /// </summary>
    /// <param name="jobName">The job name.</param>
    /// <param name="success">Whether the job succeeded.</param>
    /// <param name="details">Additional details.</param>
    Task SendJobCompletionNotificationAsync(string jobName, bool success, string? details = null);
}

/// <summary>
/// Implementation of the notification service using SignalR.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the SignalRNotificationService class.
    /// </summary>
    /// <param name="hubContext">The hub context.</param>
    /// <param name="logger">The logger instance.</param>
    public SignalRNotificationService(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendToAllAsync(string message, string type = "info")
    {
        await _hubContext.Clients.All.SendAsync("notification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation("Notification sent to all: {Message}", message);
    }

    /// <inheritdoc />
    public async Task SendToUserAsync(string userId, string message, string type = "info")
    {
        await _hubContext.Clients.User(userId).SendAsync("notification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation("Notification sent to user {UserId}: {Message}", userId, message);
    }

    /// <inheritdoc />
    public async Task SendToGroupAsync(string groupName, string message, string type = "info")
    {
        await _hubContext.Clients.Group(groupName).SendAsync("notification", new
        {
            message,
            type,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation("Notification sent to group {Group}: {Message}", groupName, message);
    }

    /// <inheritdoc />
    public async Task SendVoucherNotificationAsync(Guid voucherId, string voucherNumber, string status, string actionBy)
    {
        var message = $"Voucher {voucherNumber} has been {status} by {actionBy}";
        await SendToGroupAsync("VoucherApprovals", message, "info");
    }

    /// <inheritdoc />
    public async Task SendJobCompletionNotificationAsync(string jobName, bool success, string? details = null)
    {
        var type = success ? "success" : "error";
        var message = success
            ? $"Job '{jobName}' completed successfully."
            : $"Job '{jobName}' failed. {details}";

        await SendToGroupAsync("SystemJobs", message, type);
    }
}
