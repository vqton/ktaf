using AMS.Domain.Entities;

namespace AMS.Domain.Entities.DM;

/// <summary>
/// Represents a warehouse/-storage location for inventory management.
/// </summary>
public class Warehouse : BaseAuditEntity
{
    /// <summary>
    /// Unique warehouse code.
    /// </summary>
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the warehouse.
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the warehouse.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Name of the warehouse manager.
    /// </summary>
    public string? Manager { get; set; }

    /// <summary>
    /// Inventory costing method (FIFO or AVCO).
    /// </summary>
    public string PricingMethod { get; set; } = "AVCO";

    /// <summary>
    /// Indicates if the warehouse is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
