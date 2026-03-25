using AMS.Domain.Enums;

namespace AMS.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductNameEn { get; set; }
    public Guid? ProductGroupId { get; set; }
    public string? ProductGroupName { get; set; }
    public ProductType Type { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? VatRate { get; set; }
    public bool IsExciseTaxItem { get; set; }
    public bool IsActive { get; set; }
    public string? TaxCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Specification { get; set; }
    public string? Origin { get; set; }
}

public class CreateProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductNameEn { get; set; }
    public Guid? ProductGroupId { get; set; }
    public ProductType Type { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? VatRate { get; set; }
    public bool IsExciseTaxItem { get; set; }
    public string? TaxCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Specification { get; set; }
    public string? Origin { get; set; }
}

public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public Guid VoucherId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
}

public class CreateInventoryTransactionDto
{
    public Guid VoucherId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
}

public class InventoryBalanceDto
{
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
}

public class InventoryReportDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal OpeningQuantity { get; set; }
    public decimal OpeningValue { get; set; }
    public decimal InQuantity { get; set; }
    public decimal InValue { get; set; }
    public decimal OutQuantity { get; set; }
    public decimal OutValue { get; set; }
    public decimal ClosingQuantity { get; set; }
    public decimal ClosingValue { get; set; }
}
