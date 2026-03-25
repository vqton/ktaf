namespace AMS.Domain.Entities.Audit;

/// <summary>
/// Represents an immutable audit log entry for tracking all database changes.
/// Append-only table - update and delete operations are denied at database level.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Auto-incremented primary key (BIGINT for 1B+ records).
    /// </summary>
    public long AuditLogId { get; set; }

    /// <summary>
    /// Timestamp of the audit event (UTC).
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username who performed the action.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the table that was modified.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Primary key value of the modified record.
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// Type of operation (INSERT, UPDATE, DELETE, APPROVE, REJECT, POST, REVERSE).
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// JSON representation of the old values (before change).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON representation of the new values (after change).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// IP address of the client.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent/browser information.
    /// </summary>
    public string? UserAgent { get; set; }
}