using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Cfg;

/// <summary>
/// Represents an outbox message for reliable domain event delivery.
/// Implements the Outbox pattern for ensuring events are published even if the application crashes.
/// </summary>
public class OutboxMessage : BaseEntity
{
    /// <summary>
    /// Type of message/event (fully qualified type name).
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized message payload.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Optional key to ensure idempotent processing.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Timestamp when the message was processed (null = not processed yet).
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of times processing has been attempted.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Last error message if processing failed.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Timestamp when the message was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}