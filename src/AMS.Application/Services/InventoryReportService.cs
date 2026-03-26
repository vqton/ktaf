using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Inventory;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class InventoryReportService : IInventoryReportService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReportService(
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<InventoryBalanceReportDto>> GetStockBalanceReportAsync(int year, int month, Guid? warehouseId = null, Guid? productId = null)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var openingDate = fromDate.AddDays(-1);
        var transactions = await _inventoryRepository.GetTransactionsByDateRangeAsync(fromDate, toDate, warehouseId, productId);
        var openingTransactions = await _inventoryRepository.GetTransactionsByDateRangeAsync(DateTime.MinValue, openingDate, warehouseId, productId);

        var products = productId.HasValue
            ? new[] { await _productRepository.GetByIdAsync(productId.Value) }.Where(p => p != null)
            : await _productRepository.GetAllAsync();

        var warehouses = warehouseId.HasValue
            ? new[] { await _warehouseRepository.GetByIdAsync(warehouseId.Value) }.Where(w => w is not null)
            : await _warehouseRepository.GetAllAsync();

        var result = new InventoryBalanceReportDto
        {
            Year = year,
            Month = month,
            WarehouseId = warehouseId,
            WarehouseName = warehouseId.HasValue ? (await _warehouseRepository.GetByIdAsync(warehouseId.Value))?.WarehouseName ?? "" : "Tất cả"
        };

        foreach (var product in products.Where(p => p != null))
        {
            var p = product!;
            var productTransactions = transactions.Where(t => t.ProductId == p.Id).ToList();
            var openingTx = openingTransactions.Where(t => t.ProductId == p.Id).ToList();

            var openingQty = openingTx.Where(t => IsInTransaction(t.TransactionType)).Sum(t => t.Quantity) -
                            openingTx.Where(t => IsOutTransaction(t.TransactionType)).Sum(t => t.Quantity);

            var inQty = productTransactions.Where(t => IsInTransaction(t.TransactionType)).Sum(t => t.Quantity);
            var inAmt = productTransactions.Where(t => IsInTransaction(t.TransactionType)).Sum(t => t.TotalAmount);
            var outQty = productTransactions.Where(t => IsOutTransaction(t.TransactionType)).Sum(t => t.Quantity);
            var outAmt = productTransactions.Where(t => IsOutTransaction(t.TransactionType)).Sum(t => t.TotalAmount);

            var closingQty = openingQty + inQty - outQty;
            var avgCost = inQty > 0 ? inAmt / inQty : 0;
            var closingAmt = closingQty * avgCost;

            if (openingQty != 0 || inQty != 0 || outQty != 0 || closingQty != 0)
            {
                result.Items.Add(new InventoryBalanceDetailDto
                {
                    ProductId = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Unit = p.UnitOfMeasure ?? "",
                    OpeningQuantity = openingQty,
                    OpeningAmount = openingQty * avgCost,
                    InQuantity = inQty,
                    InAmount = inAmt,
                    OutQuantity = outQty,
                    OutAmount = outAmt,
                    ClosingQuantity = closingQty,
                    ClosingAmount = closingAmt
                });

                result.TotalQuantity += closingQty;
                result.TotalAmount += closingAmt;
            }
        }

        return ServiceResult<InventoryBalanceReportDto>.Success(result);
    }

    public async Task<ServiceResult<InventoryMovementReportDto>> GetInventoryMovementReportAsync(DateTime fromDate, DateTime toDate, Guid? warehouseId = null, Guid? productId = null)
    {
        var transactions = await _inventoryRepository.GetTransactionsByDateRangeAsync(fromDate, toDate, warehouseId, productId);

        var result = new InventoryMovementReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            WarehouseId = warehouseId,
            WarehouseName = warehouseId.HasValue ? (await _warehouseRepository.GetByIdAsync(warehouseId.Value))?.WarehouseName ?? "" : "Tất cả"
        };

        foreach (var t in transactions.OrderBy(x => x.TransactionDate))
        {
            var product = await _productRepository.GetByIdAsync(t.ProductId);
            result.Items.Add(new InventoryMovementDetailDto
            {
                TransactionDate = t.TransactionDate,
                VoucherNo = t.VoucherId.ToString(),
                TransactionType = GetTransactionTypeName(t.TransactionType),
                ProductId = t.ProductId,
                ProductCode = product?.ProductCode ?? "",
                ProductName = product?.ProductName ?? "",
                Quantity = t.Quantity,
                UnitCost = t.UnitCost,
                TotalAmount = t.TotalAmount,
                LotNumber = t.LotNumber,
                Notes = t.Description
            });
        }

        return ServiceResult<InventoryMovementReportDto>.Success(result);
    }

    public async Task<ServiceResult<InventoryValuationReportDto>> GetInventoryValuationReportAsync(int year, int month, Guid? warehouseId = null)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var balances = await _inventoryRepository.GetAllBalancesAsync(warehouseId);
        var products = await _productRepository.GetAllAsync();

        var pricingMethod = PricingMethod.BQGQ;

        var result = new InventoryValuationReportDto
        {
            Year = year,
            Month = month,
            WarehouseId = warehouseId,
            WarehouseName = warehouseId.HasValue ? (await _warehouseRepository.GetByIdAsync(warehouseId.Value))?.WarehouseName ?? "" : "Tất cả",
            PricingMethod = pricingMethod.ToString()
        };

        foreach (var balance in balances.Where(b => b.Quantity > 0))
        {
            var product = products.FirstOrDefault(p => p.Id == balance.ProductId);
            if (product == null) continue;

            var totalAmount = balance.Quantity * balance.UnitCost;
            result.Items.Add(new InventoryValuationDetailDto
            {
                ProductId = balance.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Unit = product.UnitOfMeasure ?? "",
                Quantity = balance.Quantity,
                UnitCost = balance.UnitCost,
                TotalAmount = totalAmount
            });

            result.TotalAmount += totalAmount;
        }

        return ServiceResult<InventoryValuationReportDto>.Success(result);
    }

    public async Task<ServiceResult<List<InventoryTurnoverDto>>> GetInventoryTurnoverReportAsync(int year, int month, Guid? warehouseId = null)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        var prevDate = fromDate.AddDays(-1);

        var transactions = await _inventoryRepository.GetTransactionsByDateRangeAsync(fromDate, toDate, warehouseId, null);
        var prevTransactions = await _inventoryRepository.GetTransactionsByDateRangeAsync(DateTime.MinValue, prevDate, warehouseId, null);

        var products = await _productRepository.GetAllAsync();
        var result = new List<InventoryTurnoverDto>();

        foreach (var product in products)
        {
            var productTx = transactions.Where(t => t.ProductId == product.Id).ToList();
            var prevTx = prevTransactions.Where(t => t.ProductId == product.Id).ToList();

            var opening = prevTx.Where(t => IsInTransaction(t.TransactionType)).Sum(t => t.Quantity) -
                         prevTx.Where(t => IsOutTransaction(t.TransactionType)).Sum(t => t.Quantity);

            var totalIn = productTx.Where(t => IsInTransaction(t.TransactionType)).Sum(t => t.Quantity);
            var totalOut = productTx.Where(t => IsOutTransaction(t.TransactionType)).Sum(t => t.Quantity);
            var closing = opening + totalIn - totalOut;

            var avgStock = (opening + closing) / 2;
            var turnoverRate = avgStock > 0 ? totalOut / avgStock : 0;

            result.Add(new InventoryTurnoverDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                OpeningBalance = opening,
                TotalIn = totalIn,
                TotalOut = totalOut,
                ClosingBalance = closing,
                TurnoverRate = Math.Round(turnoverRate, 2)
            });
        }

        return ServiceResult<List<InventoryTurnoverDto>>.Success(result);
    }

    public async Task<ServiceResult<List<InventoryAgingDto>>> GetInventoryAgingReportAsync(int year, int month, Guid? warehouseId = null)
    {
        var balances = await _inventoryRepository.GetAllBalancesAsync(warehouseId);
        var products = await _productRepository.GetAllAsync();

        var result = new List<InventoryAgingDto>();

        foreach (var balance in balances.Where(b => b.Quantity > 0))
        {
            var product = products.FirstOrDefault(p => p.Id == balance.ProductId);
            if (product == null) continue;

            var agingCategory = balance.ModifiedAt.HasValue && (DateTime.UtcNow - balance.ModifiedAt.Value).Days > 90
                ? "Over 90 days"
                : (balance.ModifiedAt.HasValue && (DateTime.UtcNow - balance.ModifiedAt.Value).Days > 60
                    ? "61-90 days"
                    : (balance.ModifiedAt.HasValue && (DateTime.UtcNow - balance.ModifiedAt.Value).Days > 30
                        ? "31-60 days"
                        : "Current"));

            result.Add(new InventoryAgingDto
            {
                ProductId = balance.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                CurrentStock = balance.Quantity,
                DaysInStock = balance.ModifiedAt.HasValue ? (DateTime.UtcNow - balance.ModifiedAt.Value).Days : 0,
                AgingCategory = agingCategory
            });
        }

        return ServiceResult<List<InventoryAgingDto>>.Success(result);
    }

    private static bool IsInTransaction(InventoryTransactionType type)
    {
        return type == InventoryTransactionType.PurchaseIn || 
               type == InventoryTransactionType.ReturnIn;
    }

    private static bool IsOutTransaction(InventoryTransactionType type)
    {
        return type == InventoryTransactionType.SaleOut || 
               type == InventoryTransactionType.ReturnOut || 
               type == InventoryTransactionType.Transfer ||
               type == InventoryTransactionType.InternalOut ||
               type == InventoryTransactionType.SampleOut;
    }

    private static string GetTransactionTypeName(InventoryTransactionType type)
    {
        return type switch
        {
            InventoryTransactionType.PurchaseIn => "Nhập mua",
            InventoryTransactionType.SaleOut => "Xuất bán",
            InventoryTransactionType.ReturnOut => "Xuất trả",
            InventoryTransactionType.ReturnIn => "Nhập trả",
            InventoryTransactionType.Transfer => "Chuyển kho",
            InventoryTransactionType.Adjustment => "Điều chỉnh",
            InventoryTransactionType.SampleOut => "Xuất mẫu",
            InventoryTransactionType.InternalOut => "Xuất nội bộ",
            _ => type.ToString()
        };
    }
}