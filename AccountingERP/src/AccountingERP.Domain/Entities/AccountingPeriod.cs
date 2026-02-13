using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Exceptions;

namespace AccountingERP.Domain.Entities
{
    /// <summary>
    /// Kỳ kế toán (Accounting Period)
    /// TT99-Điều 12: Quản lý kỳ kế toán và khóa sổ
    /// </summary>
    public class AccountingPeriod : AggregateRoot
    {
        /// <summary>
        /// Năm tài chính
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// Tháng (1-12)
        /// </summary>
        public int Month { get; private set; }

        /// <summary>
        /// Quý (1-4)
        /// </summary>
        public int Quarter => (Month - 1) / 3 + 1;

        /// <summary>
        /// Ngày bắt đầu kỳ
        /// </summary>
        public DateTime StartDate => new DateTime(Year, Month, 1);

        /// <summary>
        /// Ngày kết thúc kỳ
        /// </summary>
        public DateTime EndDate => StartDate.AddMonths(1).AddDays(-1);

        /// <summary>
        /// Trạng thái kỳ
        /// </summary>
        public PeriodStatus Status { get; private set; }

        /// <summary>
        /// Ngày đóng kỳ
        /// </summary>
        public DateTime? ClosedAt { get; private set; }

        /// <summary>
        /// Ngườii đóng kỳ
        /// </summary>
        public string? ClosedBy { get; private set; }

        /// <summary>
        /// Ngày khóa vĩnh viễn
        /// </summary>
        public DateTime? LockedAt { get; private set; }

        /// <summary>
        /// Ngườii khóa vĩnh viễn
        /// </summary>
        public string? LockedBy { get; private set; }

        /// <summary>
        /// Lý do mở lại (nếu có)
        /// </summary>
        public string? ReopenReason { get; private set; }

        /// <summary>
        /// Số lần mở lại
        /// </summary>
        public int ReopenCount { get; private set; }

        /// <summary>
        /// Các khoản kiểm tra trước khi đóng
        /// </summary>
        public IReadOnlyCollection<ClosingChecklistItem> ClosingChecklist => _closingChecklist.AsReadOnly();
        private List<ClosingChecklistItem> _closingChecklist = new();

        private AccountingPeriod() { }

        /// <summary>
        /// Tạo kỳ kế toán mới
        /// </summary>
        public static AccountingPeriod Create(int year, int month)
        {
            if (year < 2000 || year > 2100)
                throw new ArgumentException("Năm tài chính không hợp lệ", nameof(year));

            if (month < 1 || month > 12)
                throw new ArgumentException("Tháng phải từ 1 đến 12", nameof(month));

            return new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                Year = year,
                Month = month,
                Status = PeriodStatus.Open,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Đóng kỳ kế toán
        /// </summary>
        public void Close(string closedBy, IEnumerable<JournalEntry> entries, TrialBalance trialBalance)
        {
            if (Status == PeriodStatus.Closed || Status == PeriodStatus.Locked)
                throw new InvalidOperationException($"Kỳ {Year}/{Month} đã đóng hoặc đã khóa.");

            if (string.IsNullOrWhiteSpace(closedBy))
                throw new ArgumentException("Ngườii đóng kỳ không được để trống", nameof(closedBy));

            // Pre-closing validations
            ValidatePreClosing(entries, trialBalance);

            // Perform closing
            Status = PeriodStatus.Closing;
            
            // Create closing entries (depreciation, etc.)
            CreateClosingEntries();
            
            Status = PeriodStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            ClosedBy = closedBy.Trim();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PeriodClosedEvent(Year, Month, ClosedAt.Value, ClosedBy));
        }

        /// <summary>
        /// Mở lại kỳ (EMERGENCY ONLY)
        /// </summary>
        public void Reopen(string reopenedBy, string reason)
        {
            if (Status == PeriodStatus.Locked)
                throw new InvalidOperationException(
                    $"Kỳ {Year}/{Month} đã khóa vĩnh viễn. Không thể mở lại.");

            if (Status != PeriodStatus.Closed)
                throw new InvalidOperationException(
                    $"Kỳ {Year}/{Month} chưa đóng. Không cần mở lại.");

            if (string.IsNullOrWhiteSpace(reopenedBy))
                throw new ArgumentException("Ngườii mở lại không được để trống", nameof(reopenedBy));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "TT99: Lý do mở lại kỳ là bắt buộc và phải được ghi nhận đầy đủ.", 
                    nameof(reason));

            if (ReopenCount >= 1)
                throw new InvalidOperationException(
                    $"Kỳ {Year}/{Month} đã được mở lại {ReopenCount} lần. " +
                    "Không thể mở lại quá 1 lần cho mỗi kỳ.");

            Status = PeriodStatus.Open;
            ReopenReason = reason.Trim();
            ReopenCount++;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = reopenedBy.Trim();

            AddDomainEvent(new PeriodReopenedEvent(Year, Month, DateTime.UtcNow, reopenedBy, reason));
        }

        /// <summary>
        /// Khóa vĩnh viễn kỳ (sau khi quyết toán thuế)
        /// </summary>
        public void Lock(string lockedBy)
        {
            if (Status != PeriodStatus.Closed)
                throw new InvalidOperationException(
                    $"Kỳ {Year}/{Month} phải được đóng trước khi khóa vĩnh viễn.");

            if (string.IsNullOrWhiteSpace(lockedBy))
                throw new ArgumentException("Ngườii khóa kỳ không được để trống", nameof(lockedBy));

            Status = PeriodStatus.Locked;
            LockedAt = DateTime.UtcNow;
            LockedBy = lockedBy.Trim();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PeriodLockedEvent(Year, Month, LockedAt.Value, lockedBy));
        }

        /// <summary>
        /// Kiểm tra có thể ghi sổ vào kỳ này không
        /// </summary>
        public bool CanPostEntries()
        {
            return Status == PeriodStatus.Open;
        }

        /// <summary>
        /// Kiểm tra kỳ có mở không
        /// </summary>
        public void EnsureOpen()
        {
            if (!CanPostEntries())
                throw new PeriodClosedException(
                    $"TT99: Không thể ghi sổ vào kỳ {Year}/{Month} vì kỳ đã {Status}.");
        }

        /// <summary>
        /// Thêm mục kiểm tra đóng kỳ
        /// </summary>
        public void AddClosingChecklistItem(string description, bool isPassed, string? notes = null)
        {
            _closingChecklist.Add(new ClosingChecklistItem
            {
                Description = description,
                IsPassed = isPassed,
                Notes = notes,
                CheckedAt = DateTime.UtcNow
            });
        }

        private void ValidatePreClosing(IEnumerable<JournalEntry> entries, TrialBalance trialBalance)
        {
            // Check 1: All entries posted
            var unpostedEntries = entries.Where(e => !e.IsPosted).ToList();
            if (unpostedEntries.Any())
                throw new BusinessRuleViolationException(
                    $"TT99: Còn {unpostedEntries.Count} bút toán chưa ghi sổ trong kỳ {Year}/{Month}. " +
                    "Không thể đóng kỳ.");

            // Check 2: Trial balance balanced
            if (!trialBalance.IsBalanced)
                throw new BusinessRuleViolationException(
                    "TT99: Bảng cân đối số phát sinh không cân bằng. " +
                    $"Tổng Nợ: {trialBalance.TotalDebit:N2}, Tổng Có: {trialBalance.TotalCredit:N2}. " +
                    "Không thể đóng kỳ.");

            // Check 3: No negative balances (except contra accounts)
            var negativeBalances = trialBalance.Accounts
                .Where(a => a.Balance < 0 && !IsContraAccount(a.AccountCode))
                .ToList();
            
            if (negativeBalances.Any())
                throw new BusinessRuleViolationException(
                    $"TT99: Các tài khoản sau có số dư âm: " +
                    $"{string.Join(", ", negativeBalances.Select(a => a.AccountCode))}. " +
                    "Không thể đóng kỳ.");
        }

        private void CreateClosingEntries()
        {
            // This would create automatic closing entries like:
            // - Depreciation
            // - Amortization
            // - Accruals
            // - Provisions
            // Implementation depends on specific business rules
        }

        private static bool IsContraAccount(string accountCode)
        {
            // Contra accounts can have negative balances
            var contraAccounts = new[] { "214", "129", "229", "319" };
            return contraAccounts.Any(ca => accountCode.StartsWith(ca));
        }

        public override string ToString() => $"{Year}/{Month:D2}";
    }

    /// <summary>
    /// Mục kiểm tra trong checklist đóng kỳ
    /// </summary>
    public class ClosingChecklistItem
    {
        public string Description { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
        public string? Notes { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// Bảng cân đối số phát sinh
    /// </summary>
    public class TrialBalance
    {
        public List<TrialBalanceAccount> Accounts { get; set; } = new();
        public decimal TotalDebit => Accounts.Sum(a => a.DebitAmount);
        public decimal TotalCredit => Accounts.Sum(a => a.CreditAmount);
        public bool IsBalanced => TotalDebit == TotalCredit;
    }

    public class TrialBalanceAccount
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance => DebitAmount - CreditAmount;
    }
}
