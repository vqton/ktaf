using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.LossCarryforward
{
    /// <summary>
    /// Entity: Năm lỗ thuế TNDN (Tax Loss Year)
    /// TT96/2015/TT-BTC Article 7: Loss carryforward 5 years
    /// 
    /// RULES:
    /// - Loss can be carried forward for 5 years
    /// - Oldest loss applied first
    /// - Cannot exceed remaining amount
    /// - Expired loss cannot be applied
    /// </summary>
    public class TaxLossYear : BaseEntity
    {
        /// <summary>
        /// Năm phát sinh lỗ (Origin Year)
        /// </summary>
        public int OriginYear { get; private set; }
        
        /// <summary>
        /// Số lỗ ban đầu (Initial Loss Amount)
        /// </summary>
        public Money InitialAmount { get; private set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Số lỗ còn lại (Remaining Loss)
        /// </summary>
        public Money RemainingAmount { get; private set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Năm hết hạn (Expiry Year = OriginYear + 5)
        /// </summary>
        public int ExpiryYear => OriginYear + 5;
        
        /// <summary>
        /// Đã hết hạn chưa?
        /// </summary>
        public bool IsExpired(int currentYear) => currentYear > ExpiryYear;
        
        /// <summary>
        /// Đã sử dụng hết chưa?
        /// </summary>
        public bool IsFullyUtilized => RemainingAmount.Amount <= 0;
        
        /// <summary>
        /// Có thể sử dụng không?
        /// </summary>
        public bool CanBeUtilized(int currentYear) => !IsExpired(currentYear) && !IsFullyUtilized;
        
        /// <summary>
        /// Kỳ kế toán liên quan
        /// </summary>
        public Guid AccountingPeriodId { get; private set; }
        
        /// <summary>
        /// Lịch sử sử dụng lỗ
        /// </summary>
        private List<LossUtilization> _utilizations = new();
        public IReadOnlyCollection<LossUtilization> Utilizations => _utilizations.AsReadOnly();
        
        /// <summary>
        /// Mô tả/Mã số thuế DN
        /// </summary>
        public string? TaxCode { get; private set; }
        
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; private set; }

        private TaxLossYear() : base() { }

        /// <summary>
        /// Tạo năm lỗ mới
        /// </summary>
        public static TaxLossYear Create(
            int originYear,
            Money initialAmount,
            Guid accountingPeriodId,
            string? taxCode = null,
            string? notes = null)
        {
            if (originYear < 2000 || originYear > 2100)
                throw new ArgumentException("Năm phát sinh lỗ không hợp lệ", nameof(originYear));
            
            if (initialAmount.Amount <= 0)
                throw new ArgumentException("Số lỗ phải lớn hơn 0", nameof(initialAmount));

            return new TaxLossYear
            {
                OriginYear = originYear,
                InitialAmount = initialAmount,
                RemainingAmount = initialAmount,
                AccountingPeriodId = accountingPeriodId,
                TaxCode = taxCode?.Trim(),
                Notes = notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Áp dụng lỗ vào thu nhập chịu thuế
        /// </summary>
        /// <param name="amount">Số lỗ muốn áp dụng</param>
        /// <param name="targetYear">Năm áp dụng</param>
        /// <param name="targetPeriodId">Kỳ kế toán áp dụng</param>
        /// <param name="appliedBy">Ngườii áp dụng</param>
        /// <returns>Số lỗ thực tế được áp dụng</returns>
        public Money ApplyLoss(
            Money amount,
            int targetYear,
            Guid targetPeriodId,
            string appliedBy)
        {
            // Validation 1: Kiểm tra hết hạn
            if (IsExpired(targetYear))
                throw new InvalidOperationException(
                    $"Lỗ năm {OriginYear} đã hết hạn (hết hạn năm {ExpiryYear}). " +
                    "Theo TT96/2015, lỗ chỉ được chuyển lỗ trong vòng 5 năm.");

            // Validation 2: Kiểm tra còn lại
            if (IsFullyUtilized)
                throw new InvalidOperationException(
                    $"Lỗ năm {OriginYear} đã được sử dụng hết.");

            // Validation 3: Không âm
            if (amount.Amount <= 0)
                throw new ArgumentException("Số lỗ áp dụng phải lớn hơn 0", nameof(amount));

            // Tính toán số lỗ thực tế có thể áp dụng
            var applicableAmount = amount > RemainingAmount ? RemainingAmount : amount;

            // Tạo bản ghi sử dụng
            var utilization = LossUtilization.Create(
                Id,
                targetYear,
                targetPeriodId,
                applicableAmount,
                RemainingAmount.Subtract(applicableAmount),
                appliedBy);

            _utilizations.Add(utilization);

            // Cập nhật số lỗ còn lại
            RemainingAmount = RemainingAmount.Subtract(applicableAmount);
            UpdatedAt = DateTime.UtcNow;

            return applicableAmount;
        }

        /// <summary>
        /// Tính toán số lỗ có thể áp dụng (không thay đổi trạng thái)
        /// </summary>
        public Money CalculateApplicableAmount(Money requestedAmount, int currentYear)
        {
            if (IsExpired(currentYear) || IsFullyUtilized)
                return Money.Zero(Currency.VND);

            return requestedAmount > RemainingAmount ? RemainingAmount : requestedAmount;
        }

        /// <summary>
        /// Hủy bỏ việc áp dụng lỗ (khi điều chỉnh tờ khai)
        /// </summary>
        public void ReverseUtilization(Guid utilizationId, string reason, string reversedBy)
        {
            var utilization = _utilizations.FirstOrDefault(u => u.Id == utilizationId);
            if (utilization == null)
                throw new InvalidOperationException("Không tìm thấy bản ghi sử dụng lỗ");

            if (utilization.IsReversed)
                throw new InvalidOperationException("Bản ghi này đã được đảo ngược trước đó");

            // Hoàn trả số lỗ
            RemainingAmount = RemainingAmount.Add(utilization.Amount);
            utilization.MarkAsReversed(reason, reversedBy);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Lấy tổng số lỗ đã sử dụng
        /// </summary>
        public Money GetTotalUtilizedAmount()
        {
            var utilized = _utilizations
                .Where(u => !u.IsReversed)
                .Sum(u => u.Amount.Amount);
            
            return Money.Create(utilized, Currency.VND);
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của năm lỗ
        /// </summary>
        public bool IsValid(int currentYear)
        {
            return CanBeUtilized(currentYear) || IsFullyUtilized;
        }
    }

    /// <summary>
    /// Bản ghi sử dụng lỗ
    /// </summary>
    public class LossUtilization : BaseEntity
    {
        /// <summary>
        /// ID năm lỗ
        /// </summary>
        public Guid TaxLossYearId { get; private set; }
        
        /// <summary>
        /// Năm áp dụng lỗ
        /// </summary>
        public int TargetYear { get; private set; }
        
        /// <summary>
        /// Kỳ kế toán áp dụng
        /// </summary>
        public Guid TargetPeriodId { get; private set; }
        
        /// <summary>
        /// Số lỗ đã áp dụng
        /// </summary>
        public Money Amount { get; private set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Số lỗ còn lại sau khi áp dụng
        /// </summary>
        public Money RemainingAfter { get; private set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Ngườii áp dụng
        /// </summary>
        public string AppliedBy { get; private set; } = string.Empty;
        
        /// <summary>
        /// Đã đảo ngược?
        /// </summary>
        public bool IsReversed { get; private set; }
        
        /// <summary>
        /// Lý do đảo ngược
        /// </summary>
        public string? ReversalReason { get; private set; }
        
        /// <summary>
        /// Ngườii đảo ngược
        /// </summary>
        public string? ReversedBy { get; private set; }
        
        /// <summary>
        /// Ngày đảo ngược
        /// </summary>
        public DateTime? ReversedAt { get; private set; }

        private LossUtilization() : base() { }

        public static LossUtilization Create(
            Guid taxLossYearId,
            int targetYear,
            Guid targetPeriodId,
            Money amount,
            Money remainingAfter,
            string appliedBy)
        {
            return new LossUtilization
            {
                TaxLossYearId = taxLossYearId,
                TargetYear = targetYear,
                TargetPeriodId = targetPeriodId,
                Amount = amount,
                RemainingAfter = remainingAfter,
                AppliedBy = appliedBy.Trim(),
                IsReversed = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsReversed(string reason, string reversedBy)
        {
            IsReversed = true;
            ReversalReason = reason.Trim();
            ReversedBy = reversedBy.Trim();
            ReversedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
