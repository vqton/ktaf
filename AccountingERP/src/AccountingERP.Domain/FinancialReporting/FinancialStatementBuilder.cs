using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.FinancialReporting
{
    /// <summary>
    /// Domain Service: Xây dựng báo cáo tài chính
    /// TT99/2025 Compliance: B01-DN, B02-DN, B03-DN
    /// </summary>
    public interface IFinancialStatementBuilder
    {
        BalanceSheet GenerateBalanceSheet(AccountingPeriod period, TrialBalance trialBalance, string generatedBy);
        IncomeStatement GenerateIncomeStatement(AccountingPeriod period, TrialBalance trialBalance, string generatedBy);
        CashFlowStatement GenerateCashFlowStatement(AccountingPeriod period, TrialBalance trialBalance, IEnumerable<JournalEntry> entries, Money cashAtBeginning, string generatedBy);
    }

    /// <summary>
    /// Cấu hình mapping tài khoản cho báo cáo tài chính
    /// Cho phép tùy chỉnh mà không cần hardcode
    /// </summary>
    public class FinancialReportMapping
    {
        // Assets (Tài sản)
        public List<string> CurrentAssetAccounts { get; set; } = new() { "111", "112", "113", "128", "131", "133", "136", "138", "141", "151", "152", "153", "154", "155", "156", "157", "158", "161" };
        public List<string> NonCurrentAssetAccounts { get; set; } = new() { "211", "212", "213", "214", "217", "221", "222", "228", "229", "241", "242" };
        
        // Liabilities (Nợ phải trả) - Group 3 accounts
        public List<string> CurrentLiabilityAccounts { get; set; } = new() { "311", "315", "319", "331", "333", "334", "335", "338", "341", "343" };
        public List<string> NonCurrentLiabilityAccounts { get; set; } = new() { "342", "347" };
        
        // Equity (Vốn chủ sở hữu) - Group 4 accounts
        public List<string> EquityAccounts { get; set; } = new() { "411", "4111", "4112", "412", "413", "414", "415", "417", "418", "419", "421" };
        
        // Revenue (Doanh thu)
        public List<string> RevenueAccounts { get; set; } = new() { "511", "512", "515", "517", "518" };
        
        // Cost of Goods Sold
        public List<string> CogsAccounts { get; set; } = new() { "632" };
        
        // Operating Expenses
        public List<string> SellingExpenseAccounts { get; set; } = new() { "641" };
        public List<string> AdminExpenseAccounts { get; set; } = new() { "642" };
        
        // Financial Income/Expense
        public List<string> FinancialIncomeAccounts { get; set; } = new() { "515" };
        public List<string> FinancialExpenseAccounts { get; set; } = new() { "635" };
        
        // Other Income/Expense
        public List<string> OtherIncomeAccounts { get; set; } = new() { "711" };
        public List<string> OtherExpenseAccounts { get; set; } = new() { "811" };
        
        // Tax
        public List<string> IncomeTaxAccounts { get; set; } = new() { "821" };
        
        // Cash Flow - Operating
        public List<string> OperatingCashAccounts { get; set; } = new() { "111", "112" };
        
        // Cash Flow - Investing
        public List<string> InvestingAccounts { get; set; } = new() { "211", "212", "213", "214", "217", "221", "222" };
        
        // Cash Flow - Financing
        public List<string> FinancingAccounts { get; set; } = new() { "411", "412", "413", "414", "415" };
    }

    /// <summary>
    /// Implementation of Financial Statement Builder
    /// </summary>
    public class FinancialStatementBuilder : IFinancialStatementBuilder
    {
        private readonly FinancialReportMapping _mapping;

        public FinancialStatementBuilder(FinancialReportMapping? mapping = null)
        {
            _mapping = mapping ?? new FinancialReportMapping();
        }

        /// <summary>
        /// Generate B01-DN: Balance Sheet
        /// Rule: Assets = Liabilities + Equity
        /// </summary>
        public BalanceSheet GenerateBalanceSheet(AccountingPeriod period, TrialBalance trialBalance, string generatedBy)
        {
            var balanceSheet = BalanceSheet.Create(period, generatedBy);

            // Build Assets
            int assetOrder = 1;
            foreach (var accountCode in _mapping.CurrentAssetAccounts.OrderBy(a => a))
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null && account.Balance > 0)
                {
                    var accountName = GetAccountName(accountCode);
                    balanceSheet.AddAssetItem(accountCode, $"{accountCode} - {accountName}", Money.Create(account.Balance, Currency.VND), assetOrder++);
                }
            }

            foreach (var accountCode in _mapping.NonCurrentAssetAccounts.OrderBy(a => a))
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null && account.Balance > 0)
                {
                    var accountName = GetAccountName(accountCode);
                    balanceSheet.AddAssetItem(accountCode, $"{accountCode} - {accountName}", Money.Create(account.Balance, Currency.VND), assetOrder++);
                }
            }

            // Build Liabilities
            int liabilityOrder = 1;
            foreach (var accountCode in _mapping.CurrentLiabilityAccounts.OrderBy(a => a))
            {
                var accounts = trialBalance.Accounts.Where(a => a.AccountCode.StartsWith(accountCode)).ToList();
                foreach (var account in accounts)
                {
                    if (account.Balance < 0)
                    {
                        var accountName = GetAccountName(account.AccountCode);
                        balanceSheet.AddLiabilityItem(account.AccountCode, $"{account.AccountCode} - {accountName}", Money.Create(Math.Abs(account.Balance), Currency.VND), liabilityOrder++);
                    }
                }
            }

            foreach (var accountCode in _mapping.NonCurrentLiabilityAccounts.OrderBy(a => a))
            {
                var accounts = trialBalance.Accounts.Where(a => a.AccountCode.StartsWith(accountCode)).ToList();
                foreach (var account in accounts)
                {
                    if (account.Balance < 0)
                    {
                        var accountName = GetAccountName(account.AccountCode);
                        balanceSheet.AddLiabilityItem(account.AccountCode, $"{account.AccountCode} - {accountName}", Money.Create(Math.Abs(account.Balance), Currency.VND), liabilityOrder++);
                    }
                }
            }

            // Build Equity
            int equityOrder = 1;
            foreach (var accountCode in _mapping.EquityAccounts.OrderBy(a => a))
            {
                var accounts = trialBalance.Accounts.Where(a => a.AccountCode.StartsWith(accountCode)).ToList();
                foreach (var account in accounts)
                {
                    if (account.Balance < 0)
                    {
                        var accountName = GetAccountName(account.AccountCode);
                        balanceSheet.AddEquityItem(account.AccountCode, $"{account.AccountCode} - {accountName}", Money.Create(Math.Abs(account.Balance), Currency.VND), equityOrder++);
                    }
                }
            }

            return balanceSheet;
        }

        /// <summary>
        /// Generate B02-DN: Income Statement
        /// </summary>
        public IncomeStatement GenerateIncomeStatement(AccountingPeriod period, TrialBalance trialBalance, string generatedBy)
        {
            var incomeStatement = IncomeStatement.Create(period, generatedBy);

            // Revenue
            foreach (var accountCode in _mapping.RevenueAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null && account.CreditAmount > 0)
                {
                    var accountName = GetAccountName(accountCode);
                    incomeStatement.AddRevenueItem(accountCode, accountName, Money.Create(account.CreditAmount, Currency.VND));
                }
            }

            // Cost of Goods Sold
            var cogs = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.CogsAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    cogs = cogs.Add(Money.Create(account.DebitAmount, Currency.VND));
                }
            }
            incomeStatement.SetCostOfGoodsSold(cogs);

            // Operating Expenses
            foreach (var accountCode in _mapping.SellingExpenseAccounts.Concat(_mapping.AdminExpenseAccounts))
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null && account.DebitAmount > 0)
                {
                    var accountName = GetAccountName(accountCode);
                    incomeStatement.AddOperatingExpense(accountCode, accountName, Money.Create(account.DebitAmount, Currency.VND));
                }
            }

            // Financial Income/Expenses
            var financialIncome = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.FinancialIncomeAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    financialIncome = financialIncome.Add(Money.Create(account.CreditAmount, Currency.VND));
                }
            }
            incomeStatement.SetFinancialIncome(financialIncome);

            var financialExpenses = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.FinancialExpenseAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    financialExpenses = financialExpenses.Add(Money.Create(account.DebitAmount, Currency.VND));
                }
            }
            incomeStatement.SetFinancialExpenses(financialExpenses);

            // Other Income/Expenses
            var otherIncome = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.OtherIncomeAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    otherIncome = otherIncome.Add(Money.Create(account.CreditAmount, Currency.VND));
                }
            }
            incomeStatement.SetOtherIncome(otherIncome);

            var otherExpenses = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.OtherExpenseAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    otherExpenses = otherExpenses.Add(Money.Create(account.DebitAmount, Currency.VND));
                }
            }
            incomeStatement.SetOtherExpenses(otherExpenses);

            // Income Tax
            var incomeTax = Money.Zero(Currency.VND);
            foreach (var accountCode in _mapping.IncomeTaxAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null)
                {
                    incomeTax = incomeTax.Add(Money.Create(account.DebitAmount, Currency.VND));
                }
            }
            incomeStatement.SetIncomeTaxExpense(incomeTax);

            return incomeStatement;
        }

        /// <summary>
        /// Generate B03-DN: Cash Flow Statement (Indirect Method)
        /// </summary>
        public CashFlowStatement GenerateCashFlowStatement(
            AccountingPeriod period, 
            TrialBalance trialBalance, 
            IEnumerable<JournalEntry> entries,
            Money cashAtBeginning,
            string generatedBy)
        {
            var cashFlow = CashFlowStatement.Create(period, generatedBy);
            
            cashFlow.SetCashAtBeginning(cashAtBeginning);

            // Calculate Net Profit from Income Statement logic
            var incomeStatement = GenerateIncomeStatement(period, trialBalance, generatedBy);
            cashFlow.SetNetProfit(incomeStatement.NetProfit);

            // Operating Adjustments (Indirect Method)
            // Add back non-cash expenses
            AddDepreciationAdjustment(cashFlow, trialBalance);
            AddWorkingCapitalChanges(cashFlow, trialBalance, entries);

            // Investing Activities
            AddInvestingActivities(cashFlow, entries);

            // Financing Activities
            AddFinancingActivities(cashFlow, entries);

            return cashFlow;
        }

        private void AddDepreciationAdjustment(CashFlowStatement cashFlow, TrialBalance trialBalance)
        {
            // Depreciation accounts typically 214 (Accumulated Depreciation)
            var depreciationAccounts = new[] { "214", "2141", "2142", "2143" };
            foreach (var accountCode in depreciationAccounts)
            {
                var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account != null && account.CreditAmount > 0)
                {
                    cashFlow.AddOperatingAdjustment($"Khấu hao TSCĐ (TK {accountCode})", Money.Create(account.CreditAmount, Currency.VND), true);
                }
            }
        }

        private void AddWorkingCapitalChanges(CashFlowStatement cashFlow, TrialBalance trialBalance, IEnumerable<JournalEntry> entries)
        {
            // Accounts Receivable change
            var arAccount = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == "131");
            if (arAccount != null && arAccount.Balance != 0)
            {
                cashFlow.AddOperatingAdjustment(
                    "Thay đổi phải thu khách hàng", 
                    Money.Create(Math.Abs(arAccount.Balance), Currency.VND), 
                    arAccount.Balance < 0);
            }

            // Inventory change
            var inventoryAccount = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == "156");
            if (inventoryAccount != null && inventoryAccount.Balance != 0)
            {
                cashFlow.AddOperatingAdjustment(
                    "Thay đổi hàng tồn kho",
                    Money.Create(Math.Abs(inventoryAccount.Balance), Currency.VND),
                    inventoryAccount.Balance > 0);
            }

            // Accounts Payable change
            var apAccount = trialBalance.Accounts.FirstOrDefault(a => a.AccountCode == "331");
            if (apAccount != null && apAccount.Balance != 0)
            {
                cashFlow.AddOperatingAdjustment(
                    "Thay đổi phải trả nhà cung cấp",
                    Money.Create(Math.Abs(apAccount.Balance), Currency.VND),
                    apAccount.Balance < 0);
            }
        }

        private void AddInvestingActivities(CashFlowStatement cashFlow, IEnumerable<JournalEntry> entries)
        {
            foreach (var entry in entries.Where(e => e.IsPosted))
            {
                foreach (var line in entry.Lines)
                {
                    if (_mapping.InvestingAccounts.Any(acc => line.AccountCode.StartsWith(acc)))
                    {
                        var description = entry.Description;
                        var isInflow = line.IsCredit;
                        cashFlow.AddInvestingActivity(description, line.GetMoney(), isInflow);
                    }
                }
            }
        }

        private void AddFinancingActivities(CashFlowStatement cashFlow, IEnumerable<JournalEntry> entries)
        {
            foreach (var entry in entries.Where(e => e.IsPosted))
            {
                foreach (var line in entry.Lines)
                {
                    if (_mapping.FinancingAccounts.Any(acc => line.AccountCode.StartsWith(acc)))
                    {
                        var description = entry.Description;
                        var isInflow = line.IsCredit;
                        cashFlow.AddFinancingActivity(description, line.GetMoney(), isInflow);
                    }
                }
            }
        }

        private string GetAccountName(string accountCode)
        {
            // In production, this would lookup from Account repository
            // For now, return a descriptive name based on code
            return accountCode switch
            {
                "111" => "Tiền mặt",
                "112" => "Tiền gửi ngân hàng",
                "131" => "Phải thu khách hàng",
                "133" => "Thuế GTGT được khấu trừ",
                "156" => "Hàng hóa",
                "211" => "TSCĐ hữu hình",
                "214" => "Hao mòn TSCĐ",
                "331" => "Phải trả nhà cung cấp",
                "333" => "Thuế và các khoản phải nộp NN",
                "411" => "Vốn đầu tư của chủ sở hữu",
                "421" => "Lợi nhuận sau thuế chưa phân phối",
                "511" => "Doanh thu bán hàng",
                "632" => "Giá vốn hàng bán",
                "641" => "Chi phí bán hàng",
                "642" => "Chi phí quản lý doanh nghiệp",
                "821" => "Chi phí thuế TNDN",
                _ => $"TK {accountCode}"
            };
        }
    }
}
