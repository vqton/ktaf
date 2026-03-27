using AMS.Domain.Entities.DM;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for Warehouse entity.
/// </summary>
public class WarehouseDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the warehouse code.
    /// </summary>
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warehouse name.
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warehouse address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the warehouse manager.
    /// </summary>
    public string? Manager { get; set; }

    /// <summary>
    /// Gets or sets the inventory costing method (FIFO or AVCO).
    /// </summary>
    public string PricingMethod { get; set; } = "AVCO";

    /// <summary>
    /// Gets or sets a value indicating whether the warehouse is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time the entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }
}