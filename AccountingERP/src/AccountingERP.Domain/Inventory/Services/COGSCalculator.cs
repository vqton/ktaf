using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Inventory;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Inventory.Services
{
    /// <summary>
    /// Domain Service: Tính toán giá vốn hàng bán (COGS)
    /// TT99/2025 Compliance
    /// </summary>
    public interface ICOGSCalculator
    {
        Money CalculateCOGS(InventoryItem item, decimal quantitySold);
        Money CalculateAverageCost(InventoryItem item);
        IEnumerable<COGSAllocation> CalculateCOGSWithDetail(InventoryItem item, decimal quantitySold);
    }

    /// <summary>
    /// Chi tiết phân bổ giá vốn
    /// </summary>
    public class COGSAllocation
    {
        public Guid TransactionId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal Quantity { get; set; }
        public Money UnitCost { get; set; } = Money.Zero(Currency.VND);
        public Money TotalCost => UnitCost.Multiply(Quantity);
    }

    /// <summary>
    /// Implementation of COGS Calculator
    /// </summary>
    public class COGSCalculator : ICOGSCalculator
    {
        /// <summary>
        /// Tính giá vốn cho một lần bán hàng
        /// </summary>
        public Money CalculateCOGS(InventoryItem item, decimal quantitySold)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (quantitySold <= 0)
                throw new ArgumentException("Số lượng bán phải lớn hơn 0", nameof(quantitySold));

            if (item.CostingMethod == CostingMethod.FIFO)
            {
                return CalculateFIFOCOGS(item, quantitySold);
            }
            else
            {
                return CalculateWeightedAverageCOGS(item, quantitySold);
            }
        }

        /// <summary>
        /// Tính giá vốn bình quân hiện tại
        /// </summary>
        public Money CalculateAverageCost(InventoryItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.CurrentQuantity == 0)
                return Money.Zero(Currency.VND);

            return Money.Create(item.CurrentValue.Amount / item.CurrentQuantity, Currency.VND);
        }

        /// <summary>
        /// Tính giá vốn với chi tiết theo từng lô (FIFO)
        /// </summary>
        public IEnumerable<COGSAllocation> CalculateCOGSWithDetail(InventoryItem item, decimal quantitySold)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var allocations = new List<COGSAllocation>();
            
            if (item.CostingMethod == CostingMethod.FIFO)
            {
                var receipts = item.Transactions
                    .Where(t => t.Type == TransactionType.Receipt && t.RemainingQuantity > 0)
                    .OrderBy(t => t.TransactionDate)
                    .ToList();

                decimal remainingToAllocate = quantitySold;

                foreach (var receipt in receipts)
                {
                    if (remainingToAllocate <= 0) break;

                    decimal quantityFromReceipt = Math.Min(remainingToAllocate, receipt.RemainingQuantity);
                    
                    allocations.Add(new COGSAllocation
                    {
                        TransactionId = receipt.Id,
                        ReceiptDate = receipt.TransactionDate,
                        Quantity = quantityFromReceipt,
                        UnitCost = receipt.UnitCost
                    });

                    remainingToAllocate -= quantityFromReceipt;
                }

                if (remainingToAllocate > 0)
                {
                    // Sử dụng giá bình quân cho phần còn thiếu
                    allocations.Add(new COGSAllocation
                    {
                        TransactionId = Guid.Empty,
                        ReceiptDate = DateTime.Now,
                        Quantity = remainingToAllocate,
                        UnitCost = CalculateAverageCost(item)
                    });
                }
            }
            else
            {
                // Weighted Average - chỉ có 1 dòng
                allocations.Add(new COGSAllocation
                {
                    TransactionId = Guid.Empty,
                    ReceiptDate = DateTime.Now,
                    Quantity = quantitySold,
                    UnitCost = CalculateAverageCost(item)
                });
            }

            return allocations;
        }

        private Money CalculateFIFOCOGS(InventoryItem item, decimal quantitySold)
        {
            var receipts = item.Transactions
                .Where(t => t.Type == TransactionType.Receipt && t.RemainingQuantity > 0)
                .OrderBy(t => t.TransactionDate)
                .ToList();

            decimal remainingToIssue = quantitySold;
            decimal totalCost = 0;

            foreach (var receipt in receipts)
            {
                if (remainingToIssue <= 0) break;

                decimal quantityFromThisReceipt = Math.Min(remainingToIssue, receipt.RemainingQuantity);
                totalCost += quantityFromThisReceipt * receipt.UnitCost.Amount;
                remainingToIssue -= quantityFromThisReceipt;
            }

            if (remainingToIssue > 0)
            {
                // Không đủ tồn kho - sử dụng giá bình quân cho phần còn thiếu
                var avgCost = item.CurrentQuantity > 0 
                    ? item.CurrentValue.Amount / item.CurrentQuantity 
                    : 0;
                totalCost += remainingToIssue * avgCost;
            }

            return Money.Create(totalCost, Currency.VND);
        }

        private Money CalculateWeightedAverageCOGS(InventoryItem item, decimal quantitySold)
        {
            if (item.CurrentQuantity == 0)
                return Money.Zero(Currency.VND);

            var averageCost = item.CurrentValue.Amount / item.CurrentQuantity;
            var cogs = quantitySold * averageCost;

            return Money.Create(cogs, Currency.VND);
        }
    }
}
