using System;

namespace AccountingERP.Domain.Events
{
    /// <summary>
    /// Sự kiện: Bút toán đã được ghi sổ
    /// </summary>
    public class JournalEntryPostedEvent : DomainEvent
    {
        public Guid EntryId { get; }
        public string EntryNumber { get; }
        public DateTime PostedAt { get; }
        public string PostedBy { get; }
        public decimal TotalAmount { get; }

        public JournalEntryPostedEvent(
            Guid entryId,
            string entryNumber,
            DateTime postedAt,
            string postedBy,
            decimal totalAmount)
        {
            EntryId = entryId;
            EntryNumber = entryNumber;
            PostedAt = postedAt;
            PostedBy = postedBy;
            TotalAmount = totalAmount;
        }
    }

    /// <summary>
    /// Sự kiện: Kỳ kế toán đã đóng
    /// </summary>
    public class PeriodClosedEvent : DomainEvent
    {
        public int Year { get; }
        public int Month { get; }
        public DateTime ClosedAt { get; }
        public string ClosedBy { get; }

        public PeriodClosedEvent(int year, int month, DateTime closedAt, string closedBy)
        {
            Year = year;
            Month = month;
            ClosedAt = closedAt;
            ClosedBy = closedBy;
        }
    }

    /// <summary>
    /// Sự kiện: Kỳ kế toán được mở lại
    /// </summary>
    public class PeriodReopenedEvent : DomainEvent
    {
        public int Year { get; }
        public int Month { get; }
        public DateTime ReopenedAt { get; }
        public string ReopenedBy { get; }
        public string Reason { get; }

        public PeriodReopenedEvent(int year, int month, DateTime reopenedAt, string reopenedBy, string reason)
        {
            Year = year;
            Month = month;
            ReopenedAt = reopenedAt;
            ReopenedBy = reopenedBy;
            Reason = reason;
        }
    }

    /// <summary>
    /// Sự kiện: Kỳ kế toán bị khóa vĩnh viễn
    /// </summary>
    public class PeriodLockedEvent : DomainEvent
    {
        public int Year { get; }
        public int Month { get; }
        public DateTime LockedAt { get; }
        public string LockedBy { get; }

        public PeriodLockedEvent(int year, int month, DateTime lockedAt, string lockedBy)
        {
            Year = year;
            Month = month;
            LockedAt = lockedAt;
            LockedBy = lockedBy;
        }
    }

    /// <summary>
    /// Sự kiện: Phát hiện giao dịch có giá trị lớn
    /// </summary>
    public class HighValueTransactionDetectedEvent : DomainEvent
    {
        public Guid EntryId { get; }
        public decimal Amount { get; }
        public string Description { get; }
        public AlertSeverity Severity { get; }

        public HighValueTransactionDetectedEvent(
            Guid entryId,
            decimal amount,
            string description,
            AlertSeverity severity)
        {
            EntryId = entryId;
            Amount = amount;
            Description = description;
            Severity = severity;
        }
    }

    public enum AlertSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
