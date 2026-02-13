using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Enterprise.PeriodLocking;
using Xunit;

namespace AccountingERP.Domain.Tests.Enterprise.PeriodLocking
{
    /// <summary>
    /// Enterprise-grade period locking tests
    /// Ensures accounting periods can be closed and remain immutable
    /// </summary>
    public class PeriodLockingTests
    {
        private readonly PeriodLockingService _service;
        private readonly MockPeriodRepository _repository;

        public PeriodLockingTests()
        {
            _repository = new MockPeriodRepository();
            _service = new PeriodLockingService(_repository);
        }

        [Fact]
        public void ClosePeriod_OpenPeriod_ShouldSucceed()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            _repository.Add(period);

            // Act
            var result = _service.ClosePeriod(period.Id, "accountant", "Year-end closing");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PeriodStatus.Closed, period.Status);
            Assert.NotNull(period.ClosedAt);
            Assert.Equal("accountant", period.ClosedBy);
        }

        [Fact]
        public void ClosePeriod_AlreadyClosedPeriod_ShouldFail()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Closed);
            period.Close("accountant");
            _repository.Add(period);

            // Act
            var result = _service.ClosePeriod(period.Id, "accountant", "Try again");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("already closed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClosePeriod_WithUnpostedEntries_ShouldFail()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            var unpostedEntry = JournalEntry.Create(
                "BT-2024-001", "INV-001", 
                new DateTime(2024, 1, 15), 
                new DateTime(2024, 1, 14), 
                "Test entry"
            );
            // Entry is NOT posted
            _repository.Add(period);
            _repository.AddEntry(unpostedEntry);

            // Act
            var result = _service.ClosePeriod(period.Id, "accountant", "Closing");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("unposted", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClosePeriod_WithImbalance_ShouldFail()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            period.SetTrialBalanceStatus(TrialBalanceStatus.Unbalanced);
            _repository.Add(period);

            // Act
            var result = _service.ClosePeriod(period.Id, "accountant", "Closing");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("imbalance", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClosePeriod_ShouldPreventNewEntries()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            _repository.Add(period);
            _service.ClosePeriod(period.Id, "accountant", "Closing");

            // Act
            var canAddEntry = _service.CanAddEntryToPeriod(period.Id);

            // Assert
            Assert.False(canAddEntry);
        }

        [Fact]
        public void ClosePeriod_ShouldPreventEntryModification()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            var entry = CreatePostedEntry(period);
            _repository.Add(period);
            _repository.AddEntry(entry);
            _service.ClosePeriod(period.Id, "accountant", "Closing");

            // Act
            var canModify = _service.CanModifyEntry(entry.Id);

            // Assert
            Assert.False(canModify);
        }

        [Fact]
        public void ReopenPeriod_WithHigherPeriodsClosed_ShouldFail()
        {
            // Arrange
            var jan = CreateTestPeriod(2024, 1, PeriodStatus.Closed);
            var feb = CreateTestPeriod(2024, 2, PeriodStatus.Closed);
            jan.Close("accountant");
            feb.Close("accountant");
            _repository.Add(jan);
            _repository.Add(feb);

            // Act
            var result = _service.ReopenPeriod(jan.Id, "admin", "Need to correct");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("subsequent period", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ReopenPeriod_WithProperAuthorization_ShouldSucceed()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Closed);
            period.Close("accountant");
            _repository.Add(period);

            // Act
            var result = _service.ReopenPeriod(period.Id, "admin", "Correction needed");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PeriodStatus.Open, period.Status);
            Assert.NotNull(period.ReopenedAt);
            Assert.Equal("admin", period.ReopenedBy);
        }

        [Fact]
        public void GetClosedPeriods_ShouldReturnOnlyClosedPeriods()
        {
            // Arrange
            var openPeriod = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            var closedPeriod = CreateTestPeriod(2024, 2, PeriodStatus.Closed);
            closedPeriod.Close("accountant");
            _repository.Add(openPeriod);
            _repository.Add(closedPeriod);

            // Act
            var closedPeriods = _service.GetClosedPeriods();

            // Assert
            Assert.Single(closedPeriods);
            Assert.Equal(2, closedPeriods[0].Month);
        }

        [Fact]
        public void IsPeriodClosed_ClosedPeriod_ShouldReturnTrue()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Closed);
            period.Close("accountant");
            _repository.Add(period);

            // Act
            var isClosed = _service.IsPeriodClosed(period.Id);

            // Assert
            Assert.True(isClosed);
        }

        [Fact]
        public void ClosePeriod_ShouldRecordAuditTrail()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            _repository.Add(period);

            // Act
            _service.ClosePeriod(period.Id, "accountant", "Year-end closing");

            // Assert
            var history = _repository.GetPeriodHistory(period.Id);
            Assert.Single(history);
            Assert.Equal(PeriodLockAction.Close, history[0].Action);
            Assert.Equal("accountant", history[0].PerformedBy);
        }

        [Fact]
        public void ClosePeriod_ShouldRequireAuthorization()
        {
            // Arrange
            var period = CreateTestPeriod(2024, 1, PeriodStatus.Open);
            _repository.Add(period);

            // Act
            var result = _service.ClosePeriod(period.Id, "junior_clerk", "Closing");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("authorization", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        private AccountingPeriod CreateTestPeriod(int year, int month, PeriodStatus status)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var period = AccountingPeriod.Create(month, year, startDate, endDate);
            
            if (status == PeriodStatus.Closed)
            {
                // Will be properly closed in test
            }
            
            return period;
        }

        private JournalEntry CreatePostedEntry(AccountingPeriod period)
        {
            var entry = JournalEntry.Create(
                $"BT-{period.Year}{period.Month:00}-001",
                "INV-001",
                period.StartDate.AddDays(5),
                period.StartDate.AddDays(4),
                "Posted entry"
            );
            entry.AddLine("111", 1000000, 0, "Cash");
            entry.AddLine("511", 0, 1000000, "Revenue");
            // Link to invoice before posting (required for revenue entries)
            entry.LinkToInvoice(AccountingERP.Domain.Invoicing.InvoiceId.New());
            entry.Post("accountant");
            return entry;
        }
    }

    /// <summary>
    /// Mock repository for testing - implements domain interface
    /// </summary>
    public class MockPeriodRepository : AccountingERP.Domain.Enterprise.PeriodLocking.MockPeriodRepository
    {
        private readonly List<AccountingPeriod> _periods = new();
        private readonly List<JournalEntry> _entries = new();
        private readonly List<PeriodLockHistory> _history = new();

        public void Add(AccountingPeriod period) => _periods.Add(period);
        public void AddEntry(JournalEntry entry) => _entries.Add(entry);
        public AccountingPeriod? GetById(Guid id) => _periods.FirstOrDefault(p => p.Id == id);
        public List<AccountingPeriod> GetAll() => _periods.ToList();
        public List<JournalEntry> GetEntriesForPeriod(Guid periodId) => _entries.ToList();
        public void AddHistory(PeriodLockHistory history) => _history.Add(history);
        public List<PeriodLockHistory> GetPeriodHistory(Guid periodId) => 
            _history.Where(h => h.PeriodId == periodId).ToList();
    }
}
