using AMS.Domain.Entities;
using AMS.Domain.Entities.Tax;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities.Inventory;

/// <summary>
/// Represents a product or service in the inventory system.
/// Used for inventory management, invoicing, and cost calculation.
/// </summary>
public class Product : BaseAuditEntity
{
    /// <summary>
    /// Unique product/service code.
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the product/service.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// English name of the product/service.
    /// </summary>
    public string? ProductNameEn { get; set; }

    /// <summary>
    /// Product group/category ID.
    /// </summary>
    public Guid? ProductGroupId { get; set; }

    /// <summary>
    /// Type of product (Product, Service, FixedAsset).
    /// </summary>
    public ProductType Type { get; set; } = ProductType.Product;

    /// <summary>
    /// Unit of measure (e.g., piece, kg, hour).
    /// </summary>
    public string? UnitOfMeasure { get; set; }

    /// <summary>
    /// Unit price.
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Currency code. Default is VND.
    /// </summary>
    public string? CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// VAT rate percentage (e.g., 0.1 = 10%).
    /// </summary>
    public decimal? VatRate { get; set; }

    /// <summary>
    /// Indicates if this item is subject to special consumption tax.
    /// </summary>
    public bool IsExciseTaxItem { get; set; }

    /// <summary>
    /// Indicates if the product is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tax code for imported goods.
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Default warehouse ID for this product.
    /// </summary>
    public Guid? WarehouseId { get; set; }

    /// <summary>
    /// Technical specifications/details.
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// Country of origin.
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    /// Navigation property to inventory transactions.
    /// </summary>
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    /// <summary>
    /// Navigation property to inventory balances.
    /// </summary>
    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();

    /// <summary>
    /// Navigation property to excise tax information.
    /// </summary>
    public ExciseTaxItem? ExciseTaxItem { get; set; }
}
