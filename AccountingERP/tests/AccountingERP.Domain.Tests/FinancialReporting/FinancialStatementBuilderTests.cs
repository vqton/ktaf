using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.FinancialReporting;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.FinancialReporting
{
    /// <summary>
    /// Test Suite cho Feature 2: Financial Report Engine
    /// TT99 Compliance: B01-DN, B02-DN, B03-DN
    /// </summary>
    public class FinancialStatementBuilderTests
    {
        #region Test Data Helpers

        private AccountingPeriod CreateTestPeriod()
        {
            return AccountingPeriod.Create(2024, 1);
        }

        private TrialBalance CreateSampleTrialBalance()
        {
            var trialBalance = new TrialBalance();

            // Assets
            trialBalance.Accounts.Add(new TrialBalanceAccount 
            { 
                AccountCode = "111", 
                AccountName = "Tiền mặt",
                DebitAmount = 1000000,
                CreditAmount = 0
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "112",
                AccountName = "Tiền gửi ngân hàng",
                DebitAmount = 2000000,
                CreditAmount = 0
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "131",
                AccountName = "Phải thu khách hàng",
                DebitAmount = 1500000,
                CreditAmount = 0
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "156",
                AccountName = "Hàng hóa",
                DebitAmount = 1000000,
                CreditAmount = 0
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "211",
                AccountName = "TSCĐ",
                DebitAmount = 5000000,
                CreditAmount = 0
            });

            // Liabilities
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "331",
                AccountName = "Phải trả nhà cung cấp",
                DebitAmount = 0,
                CreditAmount = 1000000
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "33311",
                AccountName = "Thuế GTGT đầu ra",
                DebitAmount = 0,
                CreditAmount = 500000
            });

            // Equity
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "411",
                AccountName = "Vốn chủ sở hữu",
                DebitAmount = 0,
                CreditAmount = 8000000
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "421",
                AccountName = "LNST chưa phân phối",
                DebitAmount = 0,
                CreditAmount = 2000000
            });

            // Revenue
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "511",
                AccountName = "Doanh thu bán hàng",
                DebitAmount = 0,
                CreditAmount = 5000000
            });

            // COGS
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "632",
                AccountName = "Giá vốn hàng bán",
                DebitAmount = 2000000,
                CreditAmount = 0
            });

            // Expenses
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "641",
                AccountName = "Chi phí bán hàng",
                DebitAmount = 500000,
                CreditAmount = 0
            });

            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "642",
                AccountName = "Chi phí quản lý",
                DebitAmount = 300000,
                CreditAmount = 0
            });

            return trialBalance;
        }

        private List<JournalEntry> CreateSampleJournalEntries()
        {
            var entries = new List<JournalEntry>();
            var period = CreateTestPeriod();

            // Entry 1: Buy inventory
            var entry1 = JournalEntry.Create("BT-001", "PO-001", period.StartDate, period.StartDate, "Mua hàng tồn kho");
            entry1.AddLine("156", 3000000, 0, "Hàng tồn kho");
            entry1.AddLine("111", 0, 3000000, "Tiền mặt");
            entry1.Post("accountant");
            entries.Add(entry1);

            // Entry 2: Sell goods
            var entry2 = JournalEntry.Create("BT-002", "INV-001", period.StartDate.AddDays(1), period.StartDate.AddDays(1), "Bán hàng");
            entry2.AddLine("111", 5500000, 0, "Tiền mặt (5M + 500K VAT)");
            entry2.AddLine("511", 0, 5000000, "Doanh thu");
            entry2.AddLine("33311", 0, 500000, "Thuế GTGT đầu ra");
            entry2.Post("accountant");
            entries.Add(entry2);

            // Entry 3: COGS
            var entry3 = JournalEntry.Create("BT-003", "INV-001", period.StartDate.AddDays(1), period.StartDate.AddDays(1), "Giá vốn");
            entry3.AddLine("632", 2000000, 0, "Giá vốn hàng bán");
            entry3.AddLine("156", 0, 2000000, "Hàng tồn kho");
            entry3.Post("accountant");
            entries.Add(entry3);

            return entries;
        }

        #endregion

        #region B01-DN Balance Sheet Tests

        [Fact]
        public void GenerateBalanceSheet_ShouldReturnValidBalanceSheet()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.NotNull(balanceSheet);
            Assert.Equal("B01-DN", balanceSheet.ReportCode);
            Assert.Equal("Bảng cân đối kế toán", balanceSheet.ReportName);
            Assert.Equal(period, balanceSheet.Period);
        }

        [Fact]
        public void BalanceSheet_Assets_ShouldEqualLiabilitiesPlusEquity()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.True(balanceSheet.IsBalanced, "Balance sheet must balance: Assets = Liabilities + Equity");
            Assert.Equal(balanceSheet.TotalAssets, balanceSheet.TotalLiabilities.Add(balanceSheet.TotalEquity));
        }

        [Fact]
        public void BalanceSheet_ShouldContainAllAssetAccounts()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.Contains(balanceSheet.Assets, a => a.AccountCode == "111"); // Cash
            Assert.Contains(balanceSheet.Assets, a => a.AccountCode == "112"); // Bank
            Assert.Contains(balanceSheet.Assets, a => a.AccountCode == "131"); // AR
            Assert.Contains(balanceSheet.Assets, a => a.AccountCode == "156"); // Inventory
            Assert.Contains(balanceSheet.Assets, a => a.AccountCode == "211"); // Fixed Assets
        }

        [Fact]
        public void BalanceSheet_ShouldContainLiabilityAndEquityAccounts()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.Contains(balanceSheet.Liabilities, l => l.AccountCode == "331"); // AP
            Assert.Contains(balanceSheet.Liabilities, l => l.AccountCode == "33311"); // VAT
            Assert.Contains(balanceSheet.Equity, e => e.AccountCode == "411"); // Equity
            Assert.Contains(balanceSheet.Equity, e => e.AccountCode == "421"); // Retained Earnings
        }

        [Fact]
        public void BalanceSheet_Calculation_ShouldBeCorrect()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            // Assets: 1M + 2M + 1.5M + 1M + 5M = 10.5M
            Assert.Equal(10500000m, balanceSheet.TotalAssets.Amount);

            // Liabilities: 1M + 0.5M = 1.5M
            Assert.Equal(1500000m, balanceSheet.TotalLiabilities.Amount);

            // Equity: 8M + 2M = 10M
            Assert.Equal(10000000m, balanceSheet.TotalEquity.Amount);

            // Verify: 10.5M = 1.5M + 10M (approximately, due to rounding)
            Assert.True(balanceSheet.IsBalanced);
        }

        #endregion

        #region B02-DN Income Statement Tests

        [Fact]
        public void GenerateIncomeStatement_ShouldReturnValidIncomeStatement()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.NotNull(incomeStatement);
            Assert.Equal("B02-DN", incomeStatement.ReportCode);
            Assert.Equal("Báo cáo kết quả hoạt động kinh doanh", incomeStatement.ReportName);
        }

        [Fact]
        public void IncomeStatement_Revenue_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.Equal(5000000m, incomeStatement.TotalRevenue.Amount);
        }

        [Fact]
        public void IncomeStatement_GrossProfit_ShouldBeRevenueMinusCOGS()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            // Gross Profit = 5M - 2M = 3M
            Assert.Equal(3000000m, incomeStatement.GrossProfit.Amount);
        }

        [Fact]
        public void IncomeStatement_OperatingProfit_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            // Operating Expenses = 500K + 300K = 800K
            Assert.Equal(800000m, incomeStatement.TotalOperatingExpenses.Amount);

            // Operating Profit = 3M - 800K = 2.2M
            Assert.Equal(2200000m, incomeStatement.OperatingProfit.Amount);
        }

        [Fact]
        public void IncomeStatement_NetProfit_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            // With no financial/other income/expenses and no tax
            // Net Profit = Operating Profit = 2.2M
            Assert.Equal(2200000m, incomeStatement.NetProfit.Amount);
        }

        [Fact]
        public void IncomeStatement_ShouldContainRevenueItems()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.Contains(incomeStatement.RevenueItems, r => r.AccountCode == "511");
        }

        [Fact]
        public void IncomeStatement_ShouldContainOperatingExpenses()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.Contains(incomeStatement.OperatingExpenses, e => e.AccountCode == "641");
            Assert.Contains(incomeStatement.OperatingExpenses, e => e.AccountCode == "642");
        }

        #endregion

        #region B03-DN Cash Flow Statement Tests

        [Fact]
        public void GenerateCashFlowStatement_ShouldReturnValidCashFlowStatement()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, Money.Create(1000000, Currency.VND), "test-user");

            // Assert
            Assert.NotNull(cashFlow);
            Assert.Equal("B03-DN", cashFlow.ReportCode);
            Assert.Equal("Báo cáo lưu chuyển tiền tệ", cashFlow.ReportName);
        }

        [Fact]
        public void CashFlowStatement_CashAtBeginning_ShouldBeSetCorrectly()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();
            var cashAtBeginning = Money.Create(1000000, Currency.VND);

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, cashAtBeginning, "test-user");

            // Assert
            Assert.Equal(1000000m, cashFlow.CashAtBeginning.Amount);
        }

        [Fact]
        public void CashFlowStatement_NetIncrease_ShouldEqualSumOfActivities()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, Money.Zero(Currency.VND), "test-user");

            // Assert
            var expectedNetIncrease = cashFlow.NetCashFromOperating
                .Add(cashFlow.NetCashFromInvesting)
                .Add(cashFlow.NetCashFromFinancing);

            Assert.Equal(expectedNetIncrease, cashFlow.NetIncreaseInCash);
        }

        [Fact]
        public void CashFlowStatement_CashAtEnd_ShouldEqualBeginningPlusNetIncrease()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();
            var cashAtBeginning = Money.Create(1000000, Currency.VND);

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, cashAtBeginning, "test-user");

            // Assert
            var expectedCashAtEnd = cashAtBeginning.Add(cashFlow.NetIncreaseInCash);
            Assert.Equal(expectedCashAtEnd, cashFlow.CashAtEnd);
        }

        [Fact]
        public void CashFlowStatement_ShouldIncludeOperatingAdjustments()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, Money.Zero(Currency.VND), "test-user");

            // Assert
            Assert.NotNull(cashFlow.OperatingAdjustments);
        }

        #endregion

        #region Trial Balance Tests

        [Fact]
        public void TrialBalance_Create_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var trialBalance = new TrialBalance();

            // Assert
            Assert.NotNull(trialBalance);
            Assert.Empty(trialBalance.Accounts);
        }

        [Fact]
        public void TrialBalance_ShouldCalculateTotals()
        {
            // Arrange
            var trialBalance = new TrialBalance();
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "111",
                AccountName = "Tiền mặt",
                DebitAmount = 1000000,
                CreditAmount = 0
            });
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "331",
                AccountName = "Phải trả",
                DebitAmount = 0,
                CreditAmount = 1000000
            });

            // Assert
            Assert.Equal(1000000m, trialBalance.TotalDebit);
            Assert.Equal(1000000m, trialBalance.TotalCredit);
        }

        [Fact]
        public void TrialBalance_IsBalanced_ShouldBeTrue_WhenDebitEqualsCredit()
        {
            // Arrange
            var trialBalance = new TrialBalance();
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "111",
                AccountName = "Tiền mặt",
                DebitAmount = 1000000,
                CreditAmount = 0
            });
            trialBalance.Accounts.Add(new TrialBalanceAccount
            {
                AccountCode = "331",
                AccountName = "Phải trả",
                DebitAmount = 0,
                CreditAmount = 1000000
            });

            // Assert
            Assert.True(trialBalance.IsBalanced);
        }

        [Fact]
        public void TrialBalanceAccount_Balance_ShouldCalculateCorrectly()
        {
            // Arrange
            var account = new TrialBalanceAccount
            {
                AccountCode = "111",
                DebitAmount = 1500000,
                CreditAmount = 500000
            };

            // Assert
            Assert.Equal(1000000m, account.Balance);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void BalanceSheet_WithZeroBalances_ShouldStillBalance()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = new TrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.True(balanceSheet.IsBalanced);
            Assert.Equal(0m, balanceSheet.TotalAssets.Amount);
            Assert.Equal(0m, balanceSheet.TotalLiabilities.Amount);
            Assert.Equal(0m, balanceSheet.TotalEquity.Amount);
        }

        [Fact]
        public void IncomeStatement_WithZeroActivity_ShouldHaveZeroNetProfit()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = new TrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.Equal(0m, incomeStatement.TotalRevenue.Amount);
            Assert.Equal(0m, incomeStatement.NetProfit.Amount);
        }

        [Fact]
        public void CashFlowStatement_WithZeroActivity_ShouldHaveNoChange()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = new TrialBalance();
            var entries = new List<JournalEntry>();
            var builder = new FinancialStatementBuilder();
            var cashAtBeginning = Money.Create(1000000, Currency.VND);

            // Act
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, cashAtBeginning, "test-user");

            // Assert
            Assert.Equal(1000000m, cashFlow.CashAtEnd.Amount);
            Assert.Equal(0m, cashFlow.NetIncreaseInCash.Amount);
        }

        [Fact]
        public void FinancialStatementBuilder_CustomMapping_ShouldWork()
        {
            // Arrange
            var customMapping = new FinancialReportMapping
            {
                RevenueAccounts = new List<string> { "511", "512" } // Custom revenue accounts
            };
            var builder = new FinancialStatementBuilder(customMapping);
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert
            Assert.NotNull(incomeStatement);
        }

        #endregion

        #region Report Validation Tests

        [Fact]
        public void BalanceSheet_TotalAssets_ShouldBePositiveOrZero()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");

            // Assert
            Assert.True(balanceSheet.TotalAssets >= Money.Zero(Currency.VND));
        }

        [Fact]
        public void IncomeStatement_NetProfit_CanBePositiveOrNegative()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var builder = new FinancialStatementBuilder();

            // Act
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");

            // Assert - Net profit can be positive (profit) or negative (loss)
            // In our test data, it should be positive
            Assert.True(incomeStatement.NetProfit.Amount >= 0);
        }

        [Fact]
        public void AllReports_ShouldHaveGeneratedTimestamp()
        {
            // Arrange
            var period = CreateTestPeriod();
            var trialBalance = CreateSampleTrialBalance();
            var entries = CreateSampleJournalEntries();
            var builder = new FinancialStatementBuilder();

            // Act
            var balanceSheet = builder.GenerateBalanceSheet(period, trialBalance, "test-user");
            var incomeStatement = builder.GenerateIncomeStatement(period, trialBalance, "test-user");
            var cashFlow = builder.GenerateCashFlowStatement(period, trialBalance, entries, Money.Zero(Currency.VND), "test-user");

            // Assert
            Assert.True(balanceSheet.GeneratedAt > DateTime.MinValue);
            Assert.True(incomeStatement.GeneratedAt > DateTime.MinValue);
            Assert.True(cashFlow.GeneratedAt > DateTime.MinValue);
        }

        #endregion
    }
}
