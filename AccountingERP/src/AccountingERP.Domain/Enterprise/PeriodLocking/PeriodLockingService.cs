using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;

namespace AccountingERP.Domain.Enterprise.PeriodLocking
{
    /// <summary>
    /// Service for managing accounting period locking and unlocking
    /// Ensures compliance with audit requirements for period closure
    /// </summary>
    public class PeriodLockingService
    {
        private readonly MockPeriodRepository _repository;

        public PeriodLockingService(MockPeriodRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Close an accounting period
        /// </summary>
        public PeriodLockResult ClosePeriod(Guid periodId, string performedBy, string reason)
        {
            // Authorization check
            if (!HasCloseAuthorization(performedBy))
            {
                return PeriodLockResult.Failure("Insufficient authorization to close period");
            }

            var period = _repository.GetById(periodId);
            if (period == null)
            {
                return PeriodLockResult.Failure("Period not found");
            }

            // Check if already closed
            if (period.Status == PeriodStatus.Closed)
            {
                return PeriodLockResult.Failure("Period is already closed");
            }

            // Check for unposted entries
            var entries = _repository.GetEntriesForPeriod(periodId);
            var unpostedEntries = entries.Where(e => !e.IsPosted).ToList();
            if (unpostedEntries.Any())
            {
                return PeriodLockResult.Failure($"Cannot close period with {unpostedEntries.Count} unposted entries");
            }

            // Check trial balance
            if (period.TrialBalanceStatus == TrialBalanceStatus.Unbalanced)
            {
                return PeriodLockResult.Failure("Cannot close period with trial balance imbalance");
            }

            // Close the period (simple version without validation - already done above)
            period.Close(performedBy);

            // Record history
            _repository.AddHistory(new PeriodLockHistory
            {
                Id = Guid.NewGuid(),
                PeriodId = periodId,
                Action = PeriodLockAction.Close,
                PerformedBy = performedBy,
                PerformedAt = DateTime.UtcNow,
                Reason = reason
            });

            return PeriodLockResult.Success();
        }

        /// <summary>
        /// Reopen a closed accounting period
        /// </summary>
        public PeriodLockResult ReopenPeriod(Guid periodId, string performedBy, string reason)
        {
            // Authorization check - reopening requires higher privileges
            if (!HasReopenAuthorization(performedBy))
            {
                return PeriodLockResult.Failure("Insufficient authorization to reopen period");
            }

            var period = _repository.GetById(periodId);
            if (period == null)
            {
                return PeriodLockResult.Failure("Period not found");
            }

            // Check if open
            if (period.Status == PeriodStatus.Open)
            {
                return PeriodLockResult.Failure("Period is already open");
            }

            // Check if subsequent periods are closed
            var allPeriods = _repository.GetAll();
            var subsequentClosedPeriods = allPeriods
                .Where(p => p.Status == PeriodStatus.Closed && 
                           ((p.Year == period.Year && p.Month > period.Month) || 
                            (p.Year > period.Year)))
                .ToList();

            if (subsequentClosedPeriods.Any())
            {
                return PeriodLockResult.Failure("Cannot reopen period when subsequent periods are closed");
            }

            // Reopen the period
            period.Reopen(performedBy, reason);

            // Record history
            _repository.AddHistory(new PeriodLockHistory
            {
                Id = Guid.NewGuid(),
                PeriodId = periodId,
                Action = PeriodLockAction.Reopen,
                PerformedBy = performedBy,
                PerformedAt = DateTime.UtcNow,
                Reason = reason
            });

            return PeriodLockResult.Success();
        }

        /// <summary>
        /// Check if new entries can be added to a period
        /// </summary>
        public bool CanAddEntryToPeriod(Guid periodId)
        {
            var period = _repository.GetById(periodId);
            return period?.Status == PeriodStatus.Open;
        }

        /// <summary>
        /// Check if an entry can be modified
        /// </summary>
        public bool CanModifyEntry(Guid entryId)
        {
            // Find the entry and its period
            var allPeriods = _repository.GetAll();
            foreach (var period in allPeriods)
            {
                var entries = _repository.GetEntriesForPeriod(period.Id);
                if (entries.Any(e => e.Id == entryId))
                {
                    return period.Status == PeriodStatus.Open;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a period is closed
        /// </summary>
        public bool IsPeriodClosed(Guid periodId)
        {
            var period = _repository.GetById(periodId);
            return period?.Status == PeriodStatus.Closed;
        }

        /// <summary>
        /// Get all closed periods
        /// </summary>
        public List<AccountingPeriod> GetClosedPeriods()
        {
            return _repository.GetAll()
                .Where(p => p.Status == PeriodStatus.Closed)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month)
                .ToList();
        }

        private bool HasCloseAuthorization(string user)
        {
            // In real implementation, check roles/permissions
            var authorizedRoles = new[] { "accountant", "admin", "manager" };
            return authorizedRoles.Contains(user.ToLower());
        }

        private bool HasReopenAuthorization(string user)
        {
            // Reopening requires admin privileges
            var authorizedRoles = new[] { "admin" };
            return authorizedRoles.Contains(user.ToLower());
        }
    }

    /// <summary>
    /// Result of a period locking operation
    /// </summary>
    public class PeriodLockResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        public static PeriodLockResult Success() => new() { IsSuccess = true };
        public static PeriodLockResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    /// <summary>
    /// History record for period locking actions
    /// </summary>
    public class PeriodLockHistory
    {
        public Guid Id { get; set; }
        public Guid PeriodId { get; set; }
        public PeriodLockAction Action { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Actions that can be performed on period locking
    /// </summary>
    public enum PeriodLockAction
    {
        Close,
        Reopen
    }

    /// <summary>
    /// Mock repository interface - replace with actual repository in production
    /// </summary>
    public interface MockPeriodRepository
    {
        void Add(AccountingPeriod period);
        void AddEntry(JournalEntry entry);
        AccountingPeriod? GetById(Guid id);
        List<AccountingPeriod> GetAll();
        List<JournalEntry> GetEntriesForPeriod(Guid periodId);
        void AddHistory(PeriodLockHistory history);
        List<PeriodLockHistory> GetPeriodHistory(Guid periodId);
    }
}
