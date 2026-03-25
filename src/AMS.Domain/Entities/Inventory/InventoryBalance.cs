using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Inventory;

/// <summary>
/// Represents the current inventory balance for a product in a specific warehouse.
/// Updated in real-time based on inventory transactions.
/// </summary>
public class InventoryBalance : BaseEntity
{
    /// <summary>
    /// Foreign key to the product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Foreign key to the warehouse.
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Current quantity in stock.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Current unit cost (based on FIFO or AVCO method).
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total inventory value (Quantity × UnitCost).
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Date of the last inventory transaction.
    /// </summary>
    public DateTime LastTransactionDate { get; set; }

    /// <summary>
    /// Navigation property to the product.
    /// </summary>
    public Product? Product { get; set; }
}
