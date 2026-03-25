namespace AMS.Domain.Entities;

/// <summary>
/// Base class for all domain entities providing common audit trail properties.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the entity was last modified (UTC).
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Username of the user who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Base class for audit trail entities with extended modification tracking.
/// Inherits from BaseEntity and overrides timestamp fields for audit purposes.
/// </summary>
public abstract class BaseAuditEntity : BaseEntity
{
    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username of the user who created the entity.
    /// </summary>
    public new string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the entity was last modified (UTC).
    /// </summary>
    public new DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Username of the user who last modified the entity.
    /// </summary>
    public new string? ModifiedBy { get; set; }
}

/// <summary>
/// Base class for lookup/master data entities with code, name, and active status.
/// </summary>
public abstract class BaseLookupEntity : BaseEntity
{
    /// <summary>
    /// Unique code for the lookup entity.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the lookup entity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the entity is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional description for the lookup entity.
    /// </summary>
    public string? Description { get; set; }
}
