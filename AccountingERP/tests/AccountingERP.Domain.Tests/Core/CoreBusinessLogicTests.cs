using System;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.Core
{
    /// <summary>
    /// Test Suite cho Core Business Logic
    /// Tổng: 147 test cases theo specification
    /// </summary>
    public class CoreBusinessLogicTests
    {
        #region Money Value Object Tests (15 tests)

        [Theory]
        [InlineData(0)]
        [InlineData(0.01)]
        [InlineData(100)]
        [InlineData(999999999.99)]
        public void Money_Create_WithValidAmount_ShouldSucceed(decimal amount)
        {
            var money = Money.Create(amount, Currency.VND);
            Assert.Equal(amount, money.Amount);
            Assert.Equal(Currency.VND, money.Currency);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1)]
        [InlineData(-1000000)]
        public void Money_Create_WithNegativeAmount_ShouldThrowArgumentException(decimal amount)
        {
            var ex = Assert.Throws<ArgumentException>(() => Money.Create(amount, Currency.VND));
            Assert.Contains("không được âm", ex.Message);
        }

        [Fact]
        public void Money_Add_SameCurrency_ShouldReturnSum()
        {
            var money1 = Money.VND(1000000);
            var money2 = Money.VND(500000);
            var result = money1.Add(money2);
            Assert.Equal(1500000m, result.Amount);
            Assert.Equal(Currency.VND, result.Currency);
        }

        [Fact]
        public void Money_Add_DifferentCurrency_ShouldThrowInvalidOperationException()
        {
            var money1 = Money.VND(1000000);
            var money2 = Money.USD(100);
            Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        }

        [Fact]
        public void Money_Subtract_ResultNegative_ShouldThrowInvalidOperationException()
        {
            var money1 = Money.VND(100000);
            var money2 = Money.VND(200000);
            Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
        }

        [Fact]
        public void Money_Multiply_WithNegativeFactor_ShouldThrowArgumentException()
        {
            var money = Money.VND(100000);
            Assert.Throws<ArgumentException>(() => money.Multiply(-2));
        }

        [Fact]
        public void Money_Equals_SameAmountAndCurrency_ShouldReturnTrue()
        {
            var money1 = Money.VND(100000);
            var money2 = Money.VND(100000);
            Assert.Equal(money1, money2);
            Assert.True(money1.Equals(money2));
        }

        [Fact]
        public void Money_Equals_DifferentAmount_ShouldReturnFalse()
        {
            var money1 = Money.VND(100000);
            var money2 = Money.VND(200000);
            Assert.NotEqual(money1, money2);
            Assert.False(money1.Equals(money2));
        }

        [Fact]
        public void Money_Equals_DifferentCurrency_ShouldReturnFalse()
        {
            var money1 = Money.VND(100000);
            var money2 = Money.USD(100000);
            Assert.NotEqual(money1, money2);
        }

        [Fact]
        public void Money_ToString_VND_ShouldFormatWithTwoDecimals()
        {
            var money = Money.VND(1000000);
            var result = money.ToString();
            Assert.Contains("VND", result);
            Assert.Contains("1,000,000.00", result);
        }

        [Fact]
        public void Money_Create_ZeroAmount_ShouldSucceed()
        {
            var money = Money.VND(0);
            Assert.Equal(0, money.Amount);
        }

        [Fact]
        public void Money_WithAmount_ShouldReturnNewMoney()
        {
            var money = Money.VND(100000);
            var newMoney = money.WithAmount(200000);
            Assert.Equal(200000m, newMoney.Amount);
            Assert.Equal(Currency.VND, newMoney.Currency);
        }

        [Fact]
        public void Money_Sum_Collection_ShouldWork()
        {
            var monies = new[] { Money.VND(100000), Money.VND(200000), Money.VND(300000) };
            var sum = Money.Sum(monies);
            Assert.Equal(600000m, sum.Amount);
        }

        [Fact]
        public void Money_Comparison_Operators_ShouldWork()
        {
            var small = Money.VND(100000);
            var large = Money.VND(200000);
            Assert.True(large > small);
            Assert.True(small < large);
            Assert.True(large >= small);
            Assert.True(small <= large);
        }

        [Fact]
        public void Money_Divide_ByZero_ShouldThrowArgumentException()
        {
            var money = Money.VND(100000);
            Assert.Throws<ArgumentException>(() => money.Divide(0));
        }

        #endregion

        #region AccountCode Value Object Tests (12 tests)

        [Theory]
        [InlineData("111")]
        [InlineData("112")]
        [InlineData("131")]
        [InlineData("331")]
        [InlineData("511")]
        [InlineData("632")]
        [InlineData("1111")]
        [InlineData("5111")]
        [InlineData("6321")]
        public void AccountCode_Create_WithValidCode_ShouldSucceed(string code)
        {
            var accountCode = AccountCode.Create(code);
            Assert.Equal(code, accountCode.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void AccountCode_Create_WithNullOrEmpty_ShouldThrowArgumentException(string code)
        {
            Assert.Throws<ArgumentException>(() => AccountCode.Create(code));
        }

        [Theory]
        [InlineData("11")]      // Too short
        [InlineData("1")]       // Too short
        [InlineData("11111")]   // Too long
        [InlineData("111111")]  // Too long
        [InlineData("abc")]     // Not digits
        [InlineData("11a")]     // Mixed
        public void AccountCode_Create_WithInvalidFormat_ShouldThrowArgumentException(string code)
        {
            Assert.Throws<ArgumentException>(() => AccountCode.Create(code));
        }

        [Fact]
        public void AccountCode_Create_With911_ShouldThrowInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => AccountCode.Create("911"));
        }

        [Fact]
        public void AccountCode_IsLevel1_CodeWith3Digits_ShouldReturnTrue()
        {
            var code = AccountCode.Create("111");
            Assert.True(code.IsLevel1);
            Assert.False(code.IsLevel2);
        }

        [Fact]
        public void AccountCode_IsLevel2_CodeWith4Digits_ShouldReturnTrue()
        {
            var code = AccountCode.Create("1111");
            Assert.True(code.IsLevel2);
            Assert.False(code.IsLevel1);
        }

        [Fact]
        public void AccountCode_GetLevel1Code_Level2Code_ShouldReturnFirst3Digits()
        {
            var code = AccountCode.Create("1111");
            var level1 = code.GetLevel1Code();
            Assert.Equal("111", level1.Value);
        }

        [Fact]
        public void AccountCode_IsAssetAccount_CodeStartingWith1_ShouldReturnTrue()
        {
            var code = AccountCode.Create("111");
            Assert.True(code.IsAssetAccount);
            Assert.False(code.IsLiabilityAccount);
        }

        [Fact]
        public void AccountCode_IsLiabilityAccount_CodeStartingWith3_ShouldReturnTrue()
        {
            var code = AccountCode.Create("331");
            Assert.True(code.IsLiabilityAccount);
            Assert.False(code.IsAssetAccount);
        }

        [Fact]
        public void AccountCode_IsRevenueAccount_CodeStartingWith5_ShouldReturnTrue()
        {
            var code = AccountCode.Create("511");
            Assert.True(code.IsRevenueAccount);
        }

        [Fact]
        public void AccountCode_IsExpenseAccount_CodeStartingWith6_ShouldReturnTrue()
        {
            var code = AccountCode.Create("632");
            Assert.True(code.IsExpenseAccount);
        }

        [Fact]
        public void AccountCode_IsSameLevel1Group_SameGroup_ShouldReturnTrue()
        {
            var code1 = AccountCode.Create("1111");
            var code2 = AccountCode.Create("1112");
            Assert.True(code1.IsSameLevel1Group(code2));
        }

        #endregion

        #region JournalEntry Tests (25 tests)

        [Fact]
        public void JournalEntry_Create_WithValidData_ShouldSucceed()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-2026-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Bán hàng tiền mặt"
            );

            Assert.NotNull(entry);
            Assert.Equal("BT-202602-00001", entry.EntryNumber);
            Assert.Equal("INV-2026-001", entry.OriginalDocumentNumber);
            Assert.Equal(JournalEntryStatus.Draft, entry.Status);
            Assert.False(entry.IsPosted);
        }

        [Fact]
        public void JournalEntry_Create_WithoutSourceDocumentNumber_ShouldThrowArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "", // Empty
                    entryDate: new DateTime(2020, 1, 15),
                    originalDocumentDate: new DateTime(2020, 1, 14),
                    description: "Test"
                ));
            
            Assert.Contains("TT99", ex.Message);
            Assert.Contains("chứng từ gốc", ex.Message);
        }

        [Fact]
        public void JournalEntry_Create_WithFutureEntryDate_ShouldThrowArgumentException()
        {
            var futureDate = DateTime.Now.AddDays(2);
            
            var ex = Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "INV-001",
                    entryDate: futureDate,
                    originalDocumentDate: DateTime.Now,
                    description: "Test"
                ));
            
            Assert.Contains("tương lai", ex.Message);
        }

        [Fact]
        public void JournalEntry_Create_WithSourceDocDateAfterEntryDate_ShouldThrowArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "INV-001",
                    entryDate: new DateTime(2020, 1, 15),
                    originalDocumentDate: new DateTime(2020, 1, 16), // After entry date
                    description: "Test"
                ));
            
            Assert.Contains("sau ngày ghi sổ", ex.Message);
        }

        [Fact]
        public void JournalEntry_AddLine_DebitAndCredit_ShouldSucceed()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");

            Assert.Equal(2, entry.LineCount);
            Assert.Equal(1000000m, entry.TotalDebit);
            Assert.Equal(1000000m, entry.TotalCredit);
            Assert.True(entry.IsBalanced);
        }

        [Fact]
        public void JournalEntry_AddLine_With911Account_ShouldThrowInvalidOperationException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            Assert.Throws<InvalidOperationException>(() =>
                entry.AddLine("911", 1000000, 0, "Test"));
        }

        [Fact]
        public void JournalEntry_Post_BalancedEntry_ShouldSucceed()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");

            entry.Post("accountant");

            Assert.True(entry.IsPosted);
            Assert.Equal(JournalEntryStatus.Posted, entry.Status);
            Assert.NotNull(entry.PostedDate);
        }

        [Fact]
        public void JournalEntry_Post_UnbalancedEntry_ShouldThrowInvalidOperationException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 500000, "Doanh thu - thiếu"); // Not balanced

            var ex = Assert.Throws<InvalidOperationException>(() => entry.Post("accountant"));
            Assert.Contains("không cân bằng", ex.Message);
        }

        [Fact]
        public void JournalEntry_Post_WithoutLines_ShouldThrowInvalidOperationException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            Assert.Throws<InvalidOperationException>(() => entry.Post("accountant"));
        }

        [Fact]
        public void JournalEntry_AddLine_AfterPosting_ShouldThrowInvalidOperationException()
        {
            var entry = CreatePostedEntry();
            
            Assert.Throws<InvalidOperationException>(() =>
                entry.AddLine("111", 1000000, 0, "Test"));
        }

        [Fact]
        public void JournalEntry_Post_AlreadyPosted_ShouldThrowInvalidOperationException()
        {
            var entry = CreatePostedEntry();
            
            Assert.Throws<InvalidOperationException>(() =>
                entry.Post("accountant"));
        }

        [Fact]
        public void JournalEntry_AddLine_DebitAndCreditOnSameLine_ShouldThrowArgumentException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            Assert.Throws<ArgumentException>(() =>
                entry.AddLine("111", 500000, 500000, "Both debit and credit"));
        }

        [Fact]
        public void JournalEntry_AddLine_NegativeAmount_ShouldThrowArgumentException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            Assert.Throws<ArgumentException>(() =>
                entry.AddLine("111", -1000000, 0, "Negative amount"));
        }

        [Fact]
        public void JournalEntry_AddLine_Exceed99Lines_ShouldThrowInvalidOperationException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            // Add 99 lines
            for (int i = 0; i < 99; i++)
            {
                entry.AddLine("111", 1000, 0, $"Line {i}");
            }

            // 100th line should fail
            Assert.Throws<InvalidOperationException>(() =>
                entry.AddLine("111", 1000, 0, "Line 100"));
        }

        [Fact]
        public void JournalEntry_CreateReversal_PostedEntry_ShouldCreateReversal()
        {
            var entry = CreatePostedEntry();
            
            var reversal = entry.CreateReversal(
                "BT-202602-REV-001",
                "Sai sót cần điều chỉnh",
                "accountant"
            );

            Assert.NotNull(reversal);
            Assert.Equal("BT-202602-REV-001", reversal.EntryNumber);
            Assert.Contains("Đảo bút toán", reversal.Description);
            Assert.Equal(2, reversal.LineCount); // Same number of lines
            Assert.Equal(JournalEntryStatus.Cancelled, entry.Status);
        }

        [Fact]
        public void JournalEntry_CreateReversal_NotPosted_ShouldThrowInvalidOperationException()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");

            Assert.Throws<InvalidOperationException>(() =>
                entry.CreateReversal("REV-001", "Test", "accountant"));
        }

        [Fact]
        public void JournalEntry_CreateReversal_WithoutReason_ShouldThrowArgumentException()
        {
            var entry = CreatePostedEntry();
            
            Assert.Throws<ArgumentException>(() =>
                entry.CreateReversal("REV-001", "", "accountant"));
        }

        [Fact]
        public void JournalEntry_Reversal_ShouldSwapDebitCredit()
        {
            var entry = CreatePostedEntry();
            
            var reversal = entry.CreateReversal(
                "BT-202602-REV-001",
                "Sai sót",
                "accountant"
            );

            // Original: Dr 111 (1M), Cr 511 (1M)
            // Reversal should be: Cr 111 (1M), Dr 511 (1M)
            var line111 = reversal.Lines.FirstOrDefault(l => l.AccountCode == "111");
            var line511 = reversal.Lines.FirstOrDefault(l => l.AccountCode == "511");

            Assert.NotNull(line111);
            Assert.NotNull(line511);
            Assert.Equal(0, line111.DebitAmount); // Was debit, now credit
            Assert.Equal(1000000m, line111.CreditAmount);
            Assert.Equal(1000000m, line511.DebitAmount); // Was credit, now debit
            Assert.Equal(0, line511.CreditAmount);
        }

        [Fact]
        public void JournalEntry_Create_SourceDocDateMoreThan1YearBefore_ShouldThrowArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "INV-001",
                    entryDate: new DateTime(2020, 1, 15),
                    originalDocumentDate: new DateTime(2018, 1, 14), // More than 1 year before (2020-01-15 minus 1 year = 2019-01-15)
                    description: "Test"
                ));
            
            Assert.Contains("quá 1 năm", ex.Message);
        }

        [Fact]
        public void JournalEntry_Create_FutureSourceDocDate_ShouldThrowArgumentException()
        {
            var futureDate = DateTime.Now.AddDays(1);
            
            Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "INV-001",
                    entryDate: futureDate,
                    originalDocumentDate: futureDate.AddDays(1), // Future
                    description: "Test"
                ));
        }

        [Fact]
        public void JournalEntry_Create_WithEmptyDescription_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                JournalEntry.Create(
                    entryNumber: "BT-202602-00001",
                    originalDocumentNumber: "INV-001",
                    entryDate: new DateTime(2020, 1, 15),
                    originalDocumentDate: new DateTime(2020, 1, 14),
                    description: ""
                ));
        }

        [Fact]
        public void JournalEntry_TotalAmount_ShouldEqualTotalDebit()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Test"
            );

            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("112", 2000000, 0, "Tiền gửi ngân hàng");
            entry.AddLine("511", 0, 3000000, "Doanh thu");

            Assert.Equal(3000000m, entry.TotalAmount);
            Assert.Equal(3000000m, entry.TotalDebit);
            Assert.Equal(3000000m, entry.TotalCredit);
        }

        private JournalEntry CreatePostedEntry()
        {
            var entry = JournalEntry.Create(
                entryNumber: "BT-202602-00001",
                originalDocumentNumber: "INV-001",
                entryDate: new DateTime(2020, 1, 15),
                originalDocumentDate: new DateTime(2020, 1, 14),
                description: "Bán hàng tiền mặt"
            );

            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");
            entry.Post("accountant");

            return entry;
        }

        #endregion

        #region AccountingPeriod Tests (15 tests)

        [Fact]
        public void AccountingPeriod_Create_WithValidData_ShouldSucceed()
        {
            var period = AccountingPeriod.Create(2026, 2);
            
            Assert.Equal(2026, period.Year);
            Assert.Equal(2, period.Month);
            Assert.Equal(1, period.Quarter);
            Assert.Equal(PeriodStatus.Open, period.Status);
            Assert.True(period.CanPostEntries());
        }

        [Theory]
        [InlineData(1999)]
        [InlineData(2101)]
        public void AccountingPeriod_Create_WithInvalidYear_ShouldThrowArgumentException(int year)
        {
            Assert.Throws<ArgumentException>(() => AccountingPeriod.Create(year, 2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void AccountingPeriod_Create_WithInvalidMonth_ShouldThrowArgumentException(int month)
        {
            Assert.Throws<ArgumentException>(() => AccountingPeriod.Create(2026, month));
        }

        [Fact]
        public void AccountingPeriod_Close_WithValidData_ShouldSucceed()
        {
            var period = AccountingPeriod.Create(2026, 2);
            var entries = Enumerable.Empty<JournalEntry>();
            var trialBalance = new TrialBalance { Accounts = new() };

            period.Close("accountant", entries, trialBalance);

            Assert.Equal(PeriodStatus.Closed, period.Status);
            Assert.NotNull(period.ClosedAt);
            Assert.Equal("accountant", period.ClosedBy);
        }

        [Fact]
        public void AccountingPeriod_Close_AlreadyClosed_ShouldThrowInvalidOperationException()
        {
            var period = CreateClosedPeriod();
            var entries = Enumerable.Empty<JournalEntry>();
            var trialBalance = new TrialBalance { Accounts = new() };

            Assert.Throws<InvalidOperationException>(() =>
                period.Close("accountant", entries, trialBalance));
        }

        [Fact]
        public void AccountingPeriod_Reopen_WithValidReason_ShouldSucceed()
        {
            var period = CreateClosedPeriod();
            
            period.Reopen("cfo", "Sai sót cần điều chỉnh");

            Assert.Equal(PeriodStatus.Open, period.Status);
            Assert.Equal("Sai sót cần điều chỉnh", period.ReopenReason);
            Assert.Equal(1, period.ReopenCount);
        }

        [Fact]
        public void AccountingPeriod_Reopen_LockedPeriod_ShouldThrowInvalidOperationException()
        {
            var period = CreateLockedPeriod();

            Assert.Throws<InvalidOperationException>(() =>
                period.Reopen("cfo", "Test"));
        }

        [Fact]
        public void AccountingPeriod_Reopen_WithoutReason_ShouldThrowArgumentException()
        {
            var period = CreateClosedPeriod();

            Assert.Throws<ArgumentException>(() =>
                period.Reopen("cfo", ""));
        }

        [Fact]
        public void AccountingPeriod_Reopen_Twice_ShouldThrowInvalidOperationException()
        {
            var period = CreateClosedPeriod();
            period.Reopen("cfo", "First reopen");
            period.Close("accountant", Enumerable.Empty<JournalEntry>(), new TrialBalance());

            Assert.Throws<InvalidOperationException>(() =>
                period.Reopen("cfo", "Second reopen - should fail"));
        }

        [Fact]
        public void AccountingPeriod_Lock_WithValidData_ShouldSucceed()
        {
            var period = CreateClosedPeriod();
            
            period.Lock("ceo");

            Assert.Equal(PeriodStatus.Locked, period.Status);
            Assert.NotNull(period.LockedAt);
            Assert.Equal("ceo", period.LockedBy);
        }

        [Fact]
        public void AccountingPeriod_Lock_NotClosed_ShouldThrowInvalidOperationException()
        {
            var period = AccountingPeriod.Create(2026, 2);

            Assert.Throws<InvalidOperationException>(() =>
                period.Lock("ceo"));
        }

        [Fact]
        public void AccountingPeriod_EnsureOpen_WhenClosed_ShouldThrowPeriodClosedException()
        {
            var period = CreateClosedPeriod();

            Assert.Throws<PeriodClosedException>(() =>
                period.EnsureOpen());
        }

        [Fact]
        public void AccountingPeriod_StartDate_ShouldReturnFirstDayOfMonth()
        {
            var period = AccountingPeriod.Create(2026, 2);
            Assert.Equal(new DateTime(2026, 2, 1), period.StartDate);
        }

        [Fact]
        public void AccountingPeriod_EndDate_ShouldReturnLastDayOfMonth()
        {
            var period = AccountingPeriod.Create(2026, 2);
            Assert.Equal(new DateTime(2026, 2, 28), period.EndDate);
        }

        [Fact]
        public void AccountingPeriod_ToString_ShouldReturnYearMonthFormat()
        {
            var period = AccountingPeriod.Create(2026, 2);
            Assert.Equal("2026/02", period.ToString());
        }

        private AccountingPeriod CreateClosedPeriod()
        {
            var period = AccountingPeriod.Create(2026, 2);
            period.Close("accountant", Enumerable.Empty<JournalEntry>(), new TrialBalance());
            return period;
        }

        private AccountingPeriod CreateLockedPeriod()
        {
            var period = CreateClosedPeriod();
            period.Lock("ceo");
            return period;
        }

        #endregion

        #region JournalEntryLine Tests (8 tests)

        [Fact]
        public void JournalEntryLine_CreateDebit_WithValidData_ShouldSucceed()
        {
            var line = JournalEntryLine.CreateDebit(
                Guid.NewGuid(),
                "111",
                1000000,
                "Tiền mặt"
            );

            Assert.Equal("111", line.AccountCode);
            Assert.Equal(1000000m, line.DebitAmount);
            Assert.Equal(0, line.CreditAmount);
            Assert.True(line.IsDebit);
            Assert.False(line.IsCredit);
        }

        [Fact]
        public void JournalEntryLine_CreateCredit_WithValidData_ShouldSucceed()
        {
            var line = JournalEntryLine.CreateCredit(
                Guid.NewGuid(),
                "511",
                1000000,
                "Doanh thu"
            );

            Assert.Equal("511", line.AccountCode);
            Assert.Equal(0, line.DebitAmount);
            Assert.Equal(1000000m, line.CreditAmount);
            Assert.False(line.IsDebit);
            Assert.True(line.IsCredit);
        }

        [Fact]
        public void JournalEntryLine_Create_With911Account_ShouldThrowInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "911", 1000000, "Test"));
        }

        [Fact]
        public void JournalEntryLine_Create_WithInvalidAccountCode_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "11", 1000000, "Test"));
        }

        [Fact]
        public void JournalEntryLine_Create_WithZeroAmount_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "111", 0, "Test"));
        }

        [Fact]
        public void JournalEntryLine_Create_WithNegativeAmount_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "111", -1000000, "Test"));
        }

        [Fact]
        public void JournalEntryLine_Create_WithEmptyDescription_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "111", 1000000, ""));
        }

        [Fact]
        public void JournalEntryLine_Amount_ShouldReturnPositiveValue()
        {
            var debitLine = JournalEntryLine.CreateDebit(Guid.NewGuid(), "111", 1000000, "Test");
            var creditLine = JournalEntryLine.CreateCredit(Guid.NewGuid(), "511", 2000000, "Test");

            Assert.Equal(1000000m, debitLine.Amount);
            Assert.Equal(2000000m, creditLine.Amount);
        }

        #endregion

        // TOTAL: 75 tests above
        // Remaining tests for full coverage would include:
        // - Additional edge cases
        // - Tax calculation tests (to be added when Tax entities are implemented)
        // - Integration tests
        // - Stress tests
        // - Boundary tests
    }
}
