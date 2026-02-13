using System;
using System.Collections.Generic;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.FinancialReporting
{
    /// <summary>
    /// Báo cáo tài chính cơ sở (Base class)
    /// TT99/2025 Compliance
    /// </summary>
    public abstract class FinancialStatement
    {
        public Guid Id { get; protected set; }
        public string ReportCode { get; protected set; } = string.Empty;
        public string ReportName { get; protected set; } = string.Empty;
        public DateTime ReportDate { get; protected set; }
        public AccountingPeriod Period { get; protected set; } = null!;
        public Currency Currency { get; protected set; }
        public DateTime GeneratedAt { get; protected set; }
        public string GeneratedBy { get; protected set; } = string.Empty;
        
        protected FinancialStatement()
        {
            Id = Guid.NewGuid();
            GeneratedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// B01-DN: Bảng cân đối kế toán (Balance Sheet)
    /// </summary>
    public class BalanceSheet : FinancialStatement
    {
        // Assets (Tài sản)
        public List<BalanceSheetItem> Assets { get; private set; } = new();
        public Money TotalAssets { get; private set; } = Money.Zero(Currency.VND);
        
        // Liabilities (Nợ phải trả)
        public List<BalanceSheetItem> Liabilities { get; private set; } = new();
        public Money TotalLiabilities { get; private set; } = Money.Zero(Currency.VND);
        
        // Equity (Vốn chủ sở hữu)
        public List<BalanceSheetItem> Equity { get; private set; } = new();
        public Money TotalEquity { get; private set; } = Money.Zero(Currency.VND);
        
        // Validation: Assets = Liabilities + Equity
        public bool IsBalanced => TotalAssets == TotalLiabilities.Add(TotalEquity);
        
        public static BalanceSheet Create(
            AccountingPeriod period,
            string generatedBy,
            Currency currency = Currency.VND)
        {
            return new BalanceSheet
            {
                ReportCode = "B01-DN",
                ReportName = "Bảng cân đối kế toán",
                Period = period,
                Currency = currency,
                GeneratedBy = generatedBy
            };
        }
        
        public void AddAssetItem(string accountCode, string accountName, Money amount, int displayOrder)
        {
            Assets.Add(new BalanceSheetItem { AccountCode = accountCode, AccountName = accountName, Amount = amount, DisplayOrder = displayOrder, Category = "Assets" });
            RecalculateTotals();
        }
        
        public void AddLiabilityItem(string accountCode, string accountName, Money amount, int displayOrder)
        {
            Liabilities.Add(new BalanceSheetItem { AccountCode = accountCode, AccountName = accountName, Amount = amount, DisplayOrder = displayOrder, Category = "Liabilities" });
            RecalculateTotals();
        }
        
        public void AddEquityItem(string accountCode, string accountName, Money amount, int displayOrder)
        {
            Equity.Add(new BalanceSheetItem { AccountCode = accountCode, AccountName = accountName, Amount = amount, DisplayOrder = displayOrder, Category = "Equity" });
            RecalculateTotals();
        }
        
        private void RecalculateTotals()
        {
            TotalAssets = Assets.Any() ? Money.Sum(Assets.Select(a => a.Amount)) : Money.Zero(Currency.VND);
            TotalLiabilities = Liabilities.Any() ? Money.Sum(Liabilities.Select(l => l.Amount)) : Money.Zero(Currency.VND);
            TotalEquity = Equity.Any() ? Money.Sum(Equity.Select(e => e.Amount)) : Money.Zero(Currency.VND);
        }
    }

    /// <summary>
    /// B02-DN: Báo cáo kết quả hoạt động kinh doanh (Income Statement)
    /// </summary>
    public class IncomeStatement : FinancialStatement
    {
        // Revenue (Doanh thu)
        public List<IncomeStatementItem> RevenueItems { get; private set; } = new();
        public Money TotalRevenue { get; private set; } = Money.Zero(Currency.VND);
        
        // Cost of Goods Sold (Giá vốn hàng bán)
        public Money CostOfGoodsSold { get; private set; } = Money.Zero(Currency.VND);
        
        // Gross Profit (Lợi nhuận gộp)
        public Money GrossProfit => TotalRevenue.Subtract(CostOfGoodsSold);
        
        // Operating Expenses (Chi phí bán hàng + Chi phí quản lý)
        public List<IncomeStatementItem> OperatingExpenses { get; private set; } = new();
        public Money TotalOperatingExpenses { get; private set; } = Money.Zero(Currency.VND);
        
        // Operating Profit (Lợi nhuận từ hoạt động kinh doanh)
        public Money OperatingProfit => GrossProfit.Subtract(TotalOperatingExpenses);
        
        // Financial Income/Expense (Thu nhập/Chi phí tài chính)
        public Money FinancialIncome { get; private set; } = Money.Zero(Currency.VND);
        public Money FinancialExpenses { get; private set; } = Money.Zero(Currency.VND);
        public Money NetFinancialResult => FinancialIncome.Subtract(FinancialExpenses);
        
        // Other Income/Expense (Thu nhập/Chi phí khác)
        public Money OtherIncome { get; private set; } = Money.Zero(Currency.VND);
        public Money OtherExpenses { get; private set; } = Money.Zero(Currency.VND);
        public Money NetOtherResult => OtherIncome.Subtract(OtherExpenses);
        
        // Profit Before Tax (Lợi nhuận trước thuế)
        public Money ProfitBeforeTax => OperatingProfit.Add(NetFinancialResult).Add(NetOtherResult);
        
        // Income Tax Expense (Chi phí thuế TNDN)
        public Money IncomeTaxExpense { get; private set; } = Money.Zero(Currency.VND);
        
        // Net Profit (Lợi nhuận sau thuế)
        public Money NetProfit => ProfitBeforeTax.Subtract(IncomeTaxExpense);
        
        public static IncomeStatement Create(
            AccountingPeriod period,
            string generatedBy,
            Currency currency = Currency.VND)
        {
            return new IncomeStatement
            {
                ReportCode = "B02-DN",
                ReportName = "Báo cáo kết quả hoạt động kinh doanh",
                Period = period,
                Currency = currency,
                GeneratedBy = generatedBy
            };
        }
        
        public void AddRevenueItem(string accountCode, string accountName, Money amount)
        {
            RevenueItems.Add(new IncomeStatementItem { AccountCode = accountCode, AccountName = accountName, Amount = amount, ItemType = "Revenue" });
            TotalRevenue = Money.Sum(RevenueItems.Select(r => r.Amount));
        }
        
        public void SetCostOfGoodsSold(Money amount) => CostOfGoodsSold = amount;
        
        public void AddOperatingExpense(string accountCode, string accountName, Money amount)
        {
            OperatingExpenses.Add(new IncomeStatementItem { AccountCode = accountCode, AccountName = accountName, Amount = amount, ItemType = "OperatingExpense" });
            TotalOperatingExpenses = Money.Sum(OperatingExpenses.Select(e => e.Amount));
        }
        
        public void SetFinancialIncome(Money amount) => FinancialIncome = amount;
        public void SetFinancialExpenses(Money amount) => FinancialExpenses = amount;
        public void SetOtherIncome(Money amount) => OtherIncome = amount;
        public void SetOtherExpenses(Money amount) => OtherExpenses = amount;
        public void SetIncomeTaxExpense(Money amount) => IncomeTaxExpense = amount;
    }

    /// <summary>
    /// B03-DN: Báo cáo lưu chuyển tiền tệ (Cash Flow Statement - Indirect Method)
    /// </summary>
    public class CashFlowStatement : FinancialStatement
    {
        // Operating Activities (Hoạt động kinh doanh)
        public Money NetProfit { get; private set; } = Money.Zero(Currency.VND);
        public List<CashFlowAdjustment> OperatingAdjustments { get; private set; } = new();
        public Money NetCashFromOperating { get; private set; } = Money.Zero(Currency.VND);
        
        // Investing Activities (Hoạt động đầu tư)
        public List<CashFlowItem> InvestingActivities { get; private set; } = new();
        public Money NetCashFromInvesting { get; private set; } = Money.Zero(Currency.VND);
        
        // Financing Activities (Hoạt động tài chính)
        public List<CashFlowItem> FinancingActivities { get; private set; } = new();
        public Money NetCashFromFinancing { get; private set; } = Money.Zero(Currency.VND);
        
        // Net Increase/Decrease in Cash
        public Money NetIncreaseInCash => NetCashFromOperating.Add(NetCashFromInvesting).Add(NetCashFromFinancing);
        
        // Cash at Beginning and End
        public Money CashAtBeginning { get; private set; } = Money.Zero(Currency.VND);
        public Money CashAtEnd => CashAtBeginning.Add(NetIncreaseInCash);
        
        public static CashFlowStatement Create(
            AccountingPeriod period,
            string generatedBy,
            Currency currency = Currency.VND)
        {
            return new CashFlowStatement
            {
                ReportCode = "B03-DN",
                ReportName = "Báo cáo lưu chuyển tiền tệ",
                Period = period,
                Currency = currency,
                GeneratedBy = generatedBy
            };
        }
        
        public void SetNetProfit(Money amount) => NetProfit = amount;
        public void SetCashAtBeginning(Money amount) => CashAtBeginning = amount;
        
        public void AddOperatingAdjustment(string description, Money amount, bool isPositive)
        {
            OperatingAdjustments.Add(new CashFlowAdjustment { Description = description, Amount = amount, IsPositive = isPositive });
            RecalculateOperatingCashFlow();
        }
        
        public void AddInvestingActivity(string description, Money amount, bool isInflow)
        {
            InvestingActivities.Add(new CashFlowItem { Description = description, Amount = amount, IsInflow = isInflow });
            NetCashFromInvesting = isInflow ? NetCashFromInvesting.Add(amount) : NetCashFromInvesting.Subtract(amount);
        }
        
        public void AddFinancingActivity(string description, Money amount, bool isInflow)
        {
            FinancingActivities.Add(new CashFlowItem { Description = description, Amount = amount, IsInflow = isInflow });
            NetCashFromFinancing = isInflow ? NetCashFromFinancing.Add(amount) : NetCashFromFinancing.Subtract(amount);
        }
        
        private void RecalculateOperatingCashFlow()
        {
            var adjustments = OperatingAdjustments.Sum(a => a.IsPositive ? a.Amount.Amount : -a.Amount.Amount);
            NetCashFromOperating = Money.Create(NetProfit.Amount + adjustments, NetProfit.Currency);
        }
    }

    // Helper classes for report items
    public class BalanceSheetItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public Money Amount { get; set; } = Money.Zero(Currency.VND);
        public int DisplayOrder { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class IncomeStatementItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public Money Amount { get; set; } = Money.Zero(Currency.VND);
        public string ItemType { get; set; } = string.Empty;
    }

    public class CashFlowAdjustment
    {
        public string Description { get; set; } = string.Empty;
        public Money Amount { get; set; } = Money.Zero(Currency.VND);
        public bool IsPositive { get; set; }
    }

    public class CashFlowItem
    {
        public string Description { get; set; } = string.Empty;
        public Money Amount { get; set; } = Money.Zero(Currency.VND);
        public bool IsInflow { get; set; }
    }
}
