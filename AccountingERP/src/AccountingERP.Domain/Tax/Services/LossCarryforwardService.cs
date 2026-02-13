using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Tax.LossCarryforward;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.Services
{
    /// <summary>
    /// Domain Service: Quản lý chuyển lỗ thuế TNDN
    /// TT96/2015/TT-BTC Article 7
    /// </summary>
    public interface ILossCarryforwardService
    {
        /// <summary>
        /// Áp dụng lỗ vào thu nhập chịu thuế
        /// </summary>
        LossApplicationResult ApplyLossToTaxableIncome(
            IEnumerable<TaxLossYear> availableLosses,
            Money taxableIncome,
            int targetYear,
            Guid targetPeriodId,
            string appliedBy);

        /// <summary>
        /// Lấy danh sách lỗ có thể áp dụng
        /// </summary>
        IEnumerable<TaxLossYear> GetApplicableLosses(
            IEnumerable<TaxLossYear> allLosses,
            int currentYear);

        /// <summary>
        /// Hết hạn các năm lỗ đã quá 5 năm
        /// </summary>
        ExpirationResult ExpireLosses(
            IEnumerable<TaxLossYear> losses,
            int currentYear,
            string expiredBy);

        /// <summary>
        /// Tính toán thu nhập chịu thuế sau khi áp dụng lỗ
        /// </summary>
        Money CalculateTaxableIncomeAfterLoss(
            Money grossTaxableIncome,
            IEnumerable<TaxLossYear> availableLosses,
            int currentYear);
    }

    /// <summary>
    /// Kết quả áp dụng lỗ
    /// </summary>
    public class LossApplicationResult
    {
        /// <summary>
        /// Số lỗ đã áp dụng
        /// </summary>
        public Money TotalLossApplied { get; set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Thu nhập chịu thuế sau khi áp dụng lỗ
        /// </summary>
        public Money TaxableIncomeAfterLoss { get; set; } = Money.Zero(Currency.VND);
        
        /// <summary>
        /// Chi tiết áp dụng từng năm
        /// </summary>
        public List<LossApplicationDetail> Details { get; set; } = new();
        
        /// <summary>
        /// Có áp dụng thành công không?
        /// </summary>
        public bool IsSuccessful => Details.Any();
    }

    /// <summary>
    /// Chi tiết áp dụng lỗ từng năm
    /// </summary>
    public class LossApplicationDetail
    {
        public int OriginYear { get; set; }
        public Guid TaxLossYearId { get; set; }
        public Money AmountApplied { get; set; } = Money.Zero(Currency.VND);
        public Money RemainingBefore { get; set; } = Money.Zero(Currency.VND);
        public Money RemainingAfter { get; set; } = Money.Zero(Currency.VND);
    }

    /// <summary>
    /// Kết quả hết hạn lỗ
    /// </summary>
    public class ExpirationResult
    {
        public int ExpiredCount { get; set; }
        public Money TotalExpiredAmount { get; set; } = Money.Zero(Currency.VND);
        public List<ExpiredLossInfo> ExpiredLosses { get; set; } = new();
    }

    public class ExpiredLossInfo
    {
        public int OriginYear { get; set; }
        public Money RemainingAmount { get; set; } = Money.Zero(Currency.VND);
        public int ExpiryYear { get; set; }
    }

    /// <summary>
    /// Implementation
    /// </summary>
    public class LossCarryforwardService : ILossCarryforwardService
    {
        /// <summary>
        /// Áp dụng lỗ vào thu nhập chịu thuế
        /// RULE: Oldest loss applied first
        /// </summary>
        public LossApplicationResult ApplyLossToTaxableIncome(
            IEnumerable<TaxLossYear> availableLosses,
            Money taxableIncome,
            int targetYear,
            Guid targetPeriodId,
            string appliedBy)
        {
            var result = new LossApplicationResult();
            
            if (taxableIncome.Amount <= 0)
            {
                result.TaxableIncomeAfterLoss = taxableIncome;
                return result;
            }

            // Sắp xếp: năm cũ nhất trước (FIFO)
            var sortedLosses = availableLosses
                .Where(l => l.CanBeUtilized(targetYear))
                .OrderBy(l => l.OriginYear)
                .ToList();

            var remainingIncome = taxableIncome;

            foreach (var lossYear in sortedLosses)
            {
                if (remainingIncome.Amount <= 0) break;

                // Tính số lỗ có thể áp dụng
                var applicableAmount = lossYear.CalculateApplicableAmount(remainingIncome, targetYear);
                
                if (applicableAmount.Amount <= 0) continue;

                try
                {
                    // Áp dụng lỗ
                    var actuallyApplied = lossYear.ApplyLoss(
                        applicableAmount,
                        targetYear,
                        targetPeriodId,
                        appliedBy);

                    result.TotalLossApplied = result.TotalLossApplied.Add(actuallyApplied);
                    remainingIncome = remainingIncome.Subtract(actuallyApplied);

                    result.Details.Add(new LossApplicationDetail
                    {
                        OriginYear = lossYear.OriginYear,
                        TaxLossYearId = lossYear.Id,
                        AmountApplied = actuallyApplied,
                        RemainingBefore = lossYear.RemainingAmount.Add(actuallyApplied),
                        RemainingAfter = lossYear.RemainingAmount
                    });
                }
                catch (InvalidOperationException)
                {
                    // Bỏ qua năm lỗ không áp dụng được (có thể hết hạn trong quá trình)
                    continue;
                }
            }

            result.TaxableIncomeAfterLoss = remainingIncome;
            return result;
        }

        /// <summary>
        /// Lấy danh sách lỗ có thể áp dụng
        /// </summary>
        public IEnumerable<TaxLossYear> GetApplicableLosses(
            IEnumerable<TaxLossYear> allLosses,
            int currentYear)
        {
            return allLosses
                .Where(l => l.CanBeUtilized(currentYear))
                .OrderBy(l => l.OriginYear);
        }

        /// <summary>
        /// Hết hạn các năm lỗ đã quá 5 năm
        /// </summary>
        public ExpirationResult ExpireLosses(
            IEnumerable<TaxLossYear> losses,
            int currentYear,
            string expiredBy)
        {
            var result = new ExpirationResult();

            foreach (var loss in losses.Where(l => l.IsExpired(currentYear) && !l.IsFullyUtilized))
            {
                result.ExpiredCount++;
                result.TotalExpiredAmount = result.TotalExpiredAmount.Add(loss.RemainingAmount);
                
                result.ExpiredLosses.Add(new ExpiredLossInfo
                {
                    OriginYear = loss.OriginYear,
                    RemainingAmount = loss.RemainingAmount,
                    ExpiryYear = loss.ExpiryYear
                });

                // Đánh dấu đã hết hạn (không còn sử dụng được)
                loss.ApplyLoss(
                    loss.RemainingAmount, // Áp dụng toàn bộ số còn lại
                    currentYear,
                    Guid.Empty, // System
                    $"SYSTEM_EXPIRATION_{expiredBy}");
            }

            return result;
        }

        /// <summary>
        /// Tính toán thu nhập chịu thuế sau khi áp dụng lỗ
        /// </summary>
        public Money CalculateTaxableIncomeAfterLoss(
            Money grossTaxableIncome,
            IEnumerable<TaxLossYear> availableLosses,
            int currentYear)
        {
            if (grossTaxableIncome.Amount <= 0)
                return grossTaxableIncome;

            var applicableLosses = GetApplicableLosses(availableLosses, currentYear);
            var totalAvailableLoss = applicableLosses
                .Sum(l => l.RemainingAmount.Amount);

            var deductibleLoss = Math.Min(grossTaxableIncome.Amount, totalAvailableLoss);
            
            return Money.Create(grossTaxableIncome.Amount - deductibleLoss, Currency.VND);
        }
    }
}
