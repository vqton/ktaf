using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities.Inventory;

/// <summary>
/// Represents an inventory transaction (receipt, issue, or adjustment).
/// Tracks all movements of goods in and out of warehouses.
/// </summary>
public class InventoryTransaction : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the source voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Foreign key to the product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Foreign key to the warehouse.
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Type of transaction (IN, OUT, ADJ).
    /// </summary>
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Date of the transaction.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Quantity of goods.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit cost at the time of transaction.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total amount (Quantity × UnitCost).
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Lot/batch number for tracking.
    /// </summary>
    public string? LotNumber { get; set; }

    /// <summary>
    /// Expiry date for perishable goods.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Description of the transaction.
    /// </summary>
    public string? Description { get; set; }
}
