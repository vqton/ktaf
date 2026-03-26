using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface IInventoryReportService
{
    Task<ServiceResult<InventoryBalanceReportDto>> GetStockBalanceReportAsync(int year, int month, Guid? warehouseId = null, Guid? productId = null);
    Task<ServiceResult<InventoryMovementReportDto>> GetInventoryMovementReportAsync(DateTime fromDate, DateTime toDate, Guid? warehouseId = null, Guid? productId = null);
    Task<ServiceResult<InventoryValuationReportDto>> GetInventoryValuationReportAsync(int year, int month, Guid? warehouseId = null);
    Task<ServiceResult<List<InventoryTurnoverDto>>> GetInventoryTurnoverReportAsync(int year, int month, Guid? warehouseId = null);
    Task<ServiceResult<List<InventoryAgingDto>>> GetInventoryAgingReportAsync(int year, int month, Guid? warehouseId = null);
}

public class InventoryBalanceReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public List<InventoryBalanceDetailDto> Items { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}

public class InventoryBalanceDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal OpeningQuantity { get; set; }
    public decimal OpeningAmount { get; set; }
    public decimal InQuantity { get; set; }
    public decimal InAmount { get; set; }
    public decimal OutQuantity { get; set; }
    public decimal OutAmount { get; set; }
    public decimal ClosingQuantity { get; set; }
    public decimal ClosingAmount { get; set; }
}

public class InventoryMovementReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public List<InventoryMovementDetailDto> Items { get; set; } = new();
}

public class InventoryMovementDetailDto
{
    public DateTime TransactionDate { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string? LotNumber { get; set; }
    public string? Notes { get; set; }
}

public class InventoryValuationReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string PricingMethod { get; set; } = string.Empty;
    public List<InventoryValuationDetailDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class InventoryValuationDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
}

public class InventoryTurnoverDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TurnoverRate { get; set; }
}

public class InventoryAgingDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public int DaysInStock { get; set; }
    public string AgingCategory { get; set; } = string.Empty;
}