namespace AccountingERP.Domain.Entities;

/// <summary>
/// Base entity class cho tất cả entities trong hệ thống
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; } = string.Empty;
    public string? UpdatedBy { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
