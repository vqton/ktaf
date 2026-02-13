using System;

namespace AccountingERP.Domain.Events
{
    /// <summary>
    /// Sự kiện: Hóa đơn VAT đã phát hành
    /// </summary>
    public class VatInvoiceIssuedEvent : DomainEvent
    {
        public Guid InvoiceId { get; }
        public string InvoiceNumber { get; }
        public DateTime IssueDate { get; }

        public VatInvoiceIssuedEvent(Guid invoiceId, string invoiceNumber, DateTime issueDate)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            IssueDate = issueDate;
        }
    }

    /// <summary>
    /// Sự kiện: Hóa đơn VAT đã bị hủy
    /// </summary>
    public class VatInvoiceCancelledEvent : DomainEvent
    {
        public Guid InvoiceId { get; }
        public string InvoiceNumber { get; }
        public string Reason { get; }

        public VatInvoiceCancelledEvent(Guid invoiceId, string invoiceNumber, string reason)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            Reason = reason;
        }
    }

    /// <summary>
    /// Sự kiện: Phát hiện chênh lệch VAT
    /// </summary>
    public class VatMismatchDetectedEvent : DomainEvent
    {
        public int Year { get; }
        public int Month { get; }
        public decimal Difference { get; }
        public string Description { get; }

        public VatMismatchDetectedEvent(int year, int month, decimal difference, string description)
        {
            Year = year;
            Month = month;
            Difference = difference;
            Description = description;
        }
    }

    /// <summary>
    /// Sự kiện: Thuế GTGT đầu vào không được khấu trừ
    /// </summary>
    public class NonDeductibleVatDetectedEvent : DomainEvent
    {
        public Guid InvoiceId { get; }
        public string InvoiceNumber { get; }
        public decimal VatAmount { get; }
        public string Reason { get; }

        public NonDeductibleVatDetectedEvent(Guid invoiceId, string invoiceNumber, decimal vatAmount, string reason)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            VatAmount = vatAmount;
            Reason = reason;
        }
    }

    /// <summary>
    /// Sự kiện: Doanh thu chưa xuất hóa đơn
    /// </summary>
    public class RevenueWithoutInvoiceDetectedEvent : DomainEvent
    {
        public Guid EntryId { get; }
        public string EntryNumber { get; }
        public decimal Amount { get; }
        public DateTime EntryDate { get; }

        public RevenueWithoutInvoiceDetectedEvent(Guid entryId, string entryNumber, decimal amount, DateTime entryDate)
        {
            EntryId = entryId;
            EntryNumber = entryNumber;
            Amount = amount;
            EntryDate = entryDate;
        }
    }

    /// <summary>
    /// Sự kiện: Khoảng cách số hóa đơn bất thường
    /// </summary>
    public class InvoiceNumberGapDetectedEvent : DomainEvent
    {
        public string InvoiceSeries { get; }
        public int MissingFrom { get; }
        public int MissingTo { get; }
        public int GapSize { get; }

        public InvoiceNumberGapDetectedEvent(string invoiceSeries, int missingFrom, int missingTo, int gapSize)
        {
            InvoiceSeries = invoiceSeries;
            MissingFrom = missingFrom;
            MissingTo = missingTo;
            GapSize = gapSize;
        }
    }
}
