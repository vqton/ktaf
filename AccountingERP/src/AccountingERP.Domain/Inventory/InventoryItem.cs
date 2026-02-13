using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.ValueObjects;
using AccountingERP.Domain.Events;

namespace AccountingERP.Domain.Inventory
{
    /// <summary>
    /// Phương pháp tính giá xuất kho
    /// TT99/2025 Compliance
    /// </summary>
    public enum CostingMethod
    {
        FIFO = 1,           // First In First Out - Nhập trước xuất trước
        WeightedAverage = 2 // Bình quân gia quyền
    }

    /// <summary>
    /// Aggregate: Mặt hàng tồn kho (Inventory Item)
    /// </summary>
    public class InventoryItem : AggregateRoot
    {
        public string ItemCode { get; private set; } = string.Empty;
        public string ItemName { get; private set; } = string.Empty;
        public string? Unit { get; private set; }
        public CostingMethod CostingMethod { get; private set; }
        public decimal CurrentQuantity { get; private set; }
        public Money CurrentValue { get; private set; } = Money.Zero(Currency.VND);
        public decimal AverageCost => CurrentQuantity > 0 ? CurrentValue.Amount / CurrentQuantity : 0;
        public bool IsActive { get; private set; }
        
        // Lịch sử giao dịch
        private List<InventoryTransaction> _transactions = new();
        public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

        private InventoryItem() { }

        public static InventoryItem Create(
            string itemCode,
            string itemName,
            string? unit,
            CostingMethod costingMethod)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Mã hàng hóa không được để trống", nameof(itemCode));
            
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Tên hàng hóa không được để trống", nameof(itemName));

            return new InventoryItem
            {
                Id = Guid.NewGuid(),
                ItemCode = itemCode.Trim(),
                ItemName = itemName.Trim(),
                Unit = unit?.Trim() ?? "cái",
                CostingMethod = costingMethod,
                CurrentQuantity = 0,
                CurrentValue = Money.Zero(Currency.VND),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Nhập kho
        /// </summary>
        public InventoryTransaction Receive(
            decimal quantity,
            Money unitCost,
            string referenceNumber,
            DateTime transactionDate,
            string createdBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng nhập phải lớn hơn 0", nameof(quantity));
            
            if (unitCost.Amount <= 0)
                throw new ArgumentException("Đơn giá nhập phải lớn hơn 0", nameof(unitCost));

            var transaction = InventoryTransaction.Create(
                Id,
                TransactionType.Receipt,
                quantity,
                unitCost,
                referenceNumber,
                transactionDate,
                createdBy);

            _transactions.Add(transaction);
            
            // Cập nhật tồn kho
            UpdateInventoryAfterReceipt(quantity, unitCost);
            
            return transaction;
        }

        /// <summary>
        /// Xuất kho
        /// </summary>
        public InventoryTransaction Issue(
            decimal quantity,
            string referenceNumber,
            DateTime transactionDate,
            string createdBy,
            bool allowNegativeStock = false)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng xuất phải lớn hơn 0", nameof(quantity));

            if (!allowNegativeStock && quantity > CurrentQuantity)
                throw new InvalidOperationException(
                    $"Không đủ tồn kho. Tồn hiện tại: {CurrentQuantity}, Yêu cầu xuất: {quantity}");

            Money unitCost;
            if (CostingMethod == CostingMethod.FIFO)
            {
                unitCost = CalculateFIFOCost(quantity);
            }
            else
            {
                unitCost = Money.Create(AverageCost, Currency.VND);
            }

            var transaction = InventoryTransaction.Create(
                Id,
                TransactionType.Issue,
                quantity,
                unitCost,
                referenceNumber,
                transactionDate,
                createdBy);

            _transactions.Add(transaction);
            
            // Cập nhật tồn kho
            UpdateInventoryAfterIssue(quantity, unitCost);
            
            return transaction;
        }

        /// <summary>
        /// Tính giá xuất kho theo FIFO
        /// </summary>
        private Money CalculateFIFOCost(decimal quantityToIssue)
        {
            var receipts = _transactions
                .Where(t => t.Type == TransactionType.Receipt && t.RemainingQuantity > 0)
                .OrderBy(t => t.TransactionDate)
                .ToList();

            decimal remainingToIssue = quantityToIssue;
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
                // Not enough stock - use average cost for the remaining
                totalCost += remainingToIssue * AverageCost;
            }

            return Money.Create(totalCost / quantityToIssue, Currency.VND);
        }

        private void UpdateInventoryAfterReceipt(decimal quantity, Money unitCost)
        {
            if (CostingMethod == CostingMethod.WeightedAverage && CurrentQuantity > 0)
            {
                // Bình quân gia quyền
                var totalValue = CurrentValue.Add(unitCost.Multiply(quantity));
                var totalQuantity = CurrentQuantity + quantity;
                CurrentValue = totalValue;
                CurrentQuantity = totalQuantity;
            }
            else
            {
                // FIFO hoặc lần đầu nhập
                CurrentValue = CurrentValue.Add(unitCost.Multiply(quantity));
                CurrentQuantity += quantity;
            }
        }

        private void UpdateInventoryAfterIssue(decimal quantity, Money unitCost)
        {
            var issueValue = unitCost.Multiply(quantity);
            CurrentValue = CurrentValue.Subtract(issueValue);
            CurrentQuantity -= quantity;

            if (CurrentQuantity == 0)
            {
                CurrentValue = Money.Zero(Currency.VND);
            }
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }

    /// <summary>
    /// Giao dịch tồn kho
    /// </summary>
    public class InventoryTransaction : BaseEntity
    {
        public Guid InventoryItemId { get; private set; }
        public TransactionType Type { get; private set; }
        public decimal Quantity { get; private set; }
        public Money UnitCost { get; private set; } = Money.Zero(Currency.VND);
        public Money TotalCost => UnitCost.Multiply(Quantity);
        public string ReferenceNumber { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }
        public decimal RemainingQuantity { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        // Liên kết với JournalEntry (tự động tạo)
        public Guid? JournalEntryId { get; private set; }

        private InventoryTransaction() { }

        public static InventoryTransaction Create(
            Guid inventoryItemId,
            TransactionType type,
            decimal quantity,
            Money unitCost,
            string referenceNumber,
            DateTime transactionDate,
            string createdBy)
        {
            return new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                InventoryItemId = inventoryItemId,
                Type = type,
                Quantity = quantity,
                UnitCost = unitCost,
                ReferenceNumber = referenceNumber?.Trim() ?? string.Empty,
                TransactionDate = transactionDate,
                RemainingQuantity = type == TransactionType.Receipt ? quantity : 0,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void ReduceRemainingQuantity(decimal amount)
        {
            if (amount > RemainingQuantity)
                throw new InvalidOperationException("Số lượng giảm vượt quá tồn còn lại của lô");
            
            RemainingQuantity -= amount;
        }

        public void LinkToJournalEntry(Guid journalEntryId)
        {
            JournalEntryId = journalEntryId;
        }
    }

    public enum TransactionType
    {
        Receipt = 1,    // Nhập kho
        Issue = 2,      // Xuất kho
        Adjustment = 3, // Điều chỉnh
        Transfer = 4    // Chuyển kho
    }
}
