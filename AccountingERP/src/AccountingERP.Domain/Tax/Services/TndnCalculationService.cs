using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.Services
{
    /// <summary>
    /// Domain Service: Tính toán thuế TNDN
    /// TT78/2014/TT-BTC: Thuế thu nhập doanh nghiệp
    /// </summary>
    public interface ITndnCalculationService
    {
        TndnReport CalculateTndnReport(AccountingPeriod period, IEnumerable<JournalEntry> entries);
        Money CalculateTaxableIncome(Money accountingProfit, IEnumerable<TaxAdjustment> adjustments);
        Money CalculateCurrentTax(Money taxableIncome, decimal taxRate = 0.20m);
        IEnumerable<TaxAdjustment> CalculateAdjustments(IEnumerable<JournalEntryLine> expenseLines);
        bool IsNonDeductibleExpense(string accountCode, string description, string expenseCategory);
    }

    /// <summary>
    /// Implementation of TNDN Calculation Service
    /// </summary>
    public class TndnCalculationService : ITndnCalculationService
    {
        /// <summary>
        /// Tính toán báo cáo TNDN
        /// </summary>
        public TndnReport CalculateTndnReport(AccountingPeriod period, IEnumerable<JournalEntry> entries)
        {
            var postedEntries = entries.Where(e => e.IsPosted && e.EntryDate >= period.StartDate && e.EntryDate <= period.EndDate);
            
            // Calculate accounting profit from entries
            var revenue = CalculateTotalRevenue(postedEntries);
            var expenses = CalculateTotalExpenses(postedEntries);
            var accountingProfit = revenue.Subtract(expenses);

            // Get all expense lines for adjustment analysis
            var expenseLines = postedEntries
                .SelectMany(e => e.Lines)
                .Where(l => l.IsDebit && IsExpenseAccount(l.AccountCode));

            // Calculate tax adjustments
            var adjustments = CalculateAdjustments(expenseLines).ToList();
            
            var nonDeductibleExpenses = adjustments
                .Where(a => a.Type == AdjustmentType.NonDeductibleExpense)
                .Sum(a => a.Amount.Amount);

            var taxExemptIncome = adjustments
                .Where(a => a.Type == AdjustmentType.TaxExemptIncome)
                .Sum(a => a.Amount.Amount);

            var temporaryDifferences = adjustments
                .Where(a => a.Type == AdjustmentType.TemporaryDifference)
                .Sum(a => a.Amount.Amount);

            // Calculate taxable income
            var taxableIncome = CalculateTaxableIncome(
                accountingProfit, 
                adjustments.Where(a => a.Type != AdjustmentType.DeferredTax));

            // Calculate tax
            var currentTax = CalculateCurrentTax(taxableIncome);

            // Calculate effective tax rate
            var effectiveRate = accountingProfit.Amount > 0
                ? currentTax.Amount / accountingProfit.Amount 
                : 0;

            return new TndnReport
            {
                Period = period,
                AccountingProfit = accountingProfit,
                TaxableIncome = taxableIncome,
                CurrentTax = currentTax,
                NonDeductibleExpenses = Money.VND(nonDeductibleExpenses),
                TaxExemptIncome = Money.VND(taxExemptIncome),
                TemporaryDifferences = Money.VND(temporaryDifferences),
                EffectiveTaxRate = effectiveRate,
                Adjustments = adjustments,
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tính thu nhập chịu thuế
        /// </summary>
        public Money CalculateTaxableIncome(Money accountingProfit, IEnumerable<TaxAdjustment> adjustments)
        {
            var taxableIncome = accountingProfit.Amount;

            foreach (var adjustment in adjustments)
            {
                switch (adjustment.Type)
                {
                    case AdjustmentType.NonDeductibleExpense:
                        // Add back non-deductible expenses
                        taxableIncome += adjustment.Amount.Amount;
                        break;
                    case AdjustmentType.TaxExemptIncome:
                        // Subtract tax-exempt income
                        taxableIncome -= adjustment.Amount.Amount;
                        break;
                    case AdjustmentType.TemporaryDifference:
                        // Handle temporary differences (not permanent)
                        taxableIncome += adjustment.Amount.Amount;
                        break;
                }
            }

            return Money.VND(Math.Max(0, taxableIncome));
        }

        /// <summary>
        /// Tính thuế TNDN hiện hành
        /// </summary>
        public Money CalculateCurrentTax(Money taxableIncome, decimal taxRate = 0.20m)
        {
            if (taxableIncome.Amount <= 0)
                return Money.VND(0);

            var tax = taxableIncome.Amount * taxRate;
            return Money.VND(tax);
        }

        /// <summary>
        /// Tính các khoản điều chỉnh thuế
        /// </summary>
        public IEnumerable<TaxAdjustment> CalculateAdjustments(IEnumerable<JournalEntryLine> expenseLines)
        {
            var adjustments = new List<TaxAdjustment>();

            foreach (var line in expenseLines)
            {
                if (IsNonDeductibleExpense(line.AccountCode, line.Description, string.Empty))
                {
                    adjustments.Add(new TaxAdjustment
                    {
                        Type = AdjustmentType.NonDeductibleExpense,
                        Description = $"Chi phí không được trừ: {line.Description}",
                        Amount = line.GetMoney(),
                        AccountCode = line.AccountCode,
                        Reason = GetNonDeductibleReason(line.AccountCode, line.Description)
                    });
                }
            }

            return adjustments;
        }

        /// <summary>
        /// Kiểm tra chi phí có được trừ khi tính thuế không
        /// TT78-Điều 4: Các khoản chi không được trừ
        /// </summary>
        public bool IsNonDeductibleExpense(string accountCode, string description, string expenseCategory)
        {
            // List of non-deductible expense types
            var nonDeductiblePatterns = new Dictionary<string, string>
            {
                ["PENALTY"] = "Tiền phạt vi phạm hành chính",
                ["INTEREST_LATE"] = "Lãi chậm nộp thuế",
                ["ENTERTAINMENT"] = "Chi phí tiếp khách vượt mức",
                ["VEHICLE_PASSENGER"] = "Khấu hao xe chở ngườii",
                ["WELFARE_UNREGULATED"] = "Phúc lợi không theo quy định",
                ["RESERVES_UNAPPROVED"] = "Dự phòng không đúng quy định",
                ["DEPRECIATION_EXCESS"] = "Khấu hao vượt mức",
                ["WAGES_UNDOCUMENTED"] = "Lương không có hợp đồng",
                ["INTEREST_EXCESS"] = "Lãi vay vượt 1.5 lần lãi cơ bản",
                ["DONATIONS_EXCESS"] = "Tài trợ vượt 10% TNTT",
                ["UNRELATED"] = "Chi phí không liên quan đến SXKD",
                ["PERSONAL"] = "Chi phí cá nhân"
            };

            // Check by account code
            var nonDeductibleAccounts = new[] { "8111", "8112" }; // Various penalties
            if (nonDeductibleAccounts.Contains(accountCode))
                return true;

            // Check by description patterns
            var descriptionUpper = description.ToUpper();
            foreach (var pattern in nonDeductiblePatterns.Keys)
            {
                if (descriptionUpper.Contains(pattern))
                    return true;
            }

            // Check specific keywords
            var nonDeductibleKeywords = new[] 
            { 
                "PHẠT", "LÃI CHẬM", "GIẢI TRÍ", "TIẾP KHÁCH", "CAFE", "KARAOKE", 
                "MASSAGE", "DU LỊCH", "CÁ NHÂN" 
            };
            
            if (nonDeductibleKeywords.Any(k => descriptionUpper.Contains(k)))
                return true;

            return false;
        }

        private string GetNonDeductibleReason(string accountCode, string description)
        {
            if (accountCode == "8111") return "Tiền phạt vi phạm hành chính";
            if (accountCode == "8112") return "Tiền phạt vi phạm hợp đồng";
            
            var desc = description.ToUpper();
            if (desc.Contains("PHẠT")) return "Tiền phạt không được trừ";
            if (desc.Contains("LÃI CHẬM")) return "Lãi chậm nộp thuế không được trừ";
            if (desc.Contains("GIẢI TRÍ") || desc.Contains("CAFE")) return "Chi phí giải trí không được trừ";
            if (desc.Contains("CÁ NHÂN")) return "Chi phí cá nhân không được trừ";
            
            return "Chi phí không được trừ khi tính thuế TNDN";
        }

        private Money CalculateTotalRevenue(IEnumerable<JournalEntry> entries)
        {
            var revenueLines = entries
                .SelectMany(e => e.Lines)
                .Where(l => IsRevenueAccount(l.AccountCode) && l.IsCredit);
            
            return Money.VND(revenueLines.Sum(l => l.Amount));
        }

        private Money CalculateTotalExpenses(IEnumerable<JournalEntry> entries)
        {
            var expenseLines = entries
                .SelectMany(e => e.Lines)
                .Where(l => IsExpenseAccount(l.AccountCode) && l.IsDebit);
            
            return Money.VND(expenseLines.Sum(l => l.Amount));
        }

        private bool IsRevenueAccount(string accountCode)
        {
            return accountCode.StartsWith("5") || accountCode.StartsWith("7");
        }

        private bool IsExpenseAccount(string accountCode)
        {
            return accountCode.StartsWith("6") || accountCode.StartsWith("8");
        }
    }

    /// <summary>
    /// Báo cáo TNDN
    /// </summary>
    public class TndnReport
    {
        public AccountingPeriod Period { get; set; }
        public Money AccountingProfit { get; set; }
        public Money TaxableIncome { get; set; }
        public Money CurrentTax { get; set; }
        public Money NonDeductibleExpenses { get; set; }
        public Money TaxExemptIncome { get; set; }
        public Money TemporaryDifferences { get; set; }
        public decimal EffectiveTaxRate { get; set; }
        public List<TaxAdjustment> Adjustments { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Điều chỉnh thuế
    /// </summary>
    public class TaxAdjustment
    {
        public AdjustmentType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public Money Amount { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public enum AdjustmentType
    {
        NonDeductibleExpense,   // Chi phí không được trừ
        TaxExemptIncome,        // Thu nhập được miễn thuế
        TemporaryDifference,    // Chênh lệch tạm thời
        PermanentDifference,    // Chênh lệch vĩnh viễn
        DeferredTax             // Thuế TNDN hoãn lại
    }
}
