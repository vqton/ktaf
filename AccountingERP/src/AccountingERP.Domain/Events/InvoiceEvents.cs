using System;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Invoicing;

namespace AccountingERP.Domain.Events
{
    /// <summary>
    /// Event: Hóa đơn đã được phát hành
    /// </summary>
    public class InvoiceIssuedEvent : DomainEvent
    {
        public InvoiceId InvoiceId { get; }
        public string InvoiceNumber { get; }
        public DateTime IssueDate { get; }
        public JournalEntryId JournalEntryId { get; }
        
        public InvoiceIssuedEvent(
            InvoiceId invoiceId,
            string invoiceNumber,
            DateTime issueDate,
            JournalEntryId journalEntryId)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            IssueDate = issueDate;
            JournalEntryId = journalEntryId;
        }
    }
    
    /// <summary>
    /// Event: Hóa đơn đã bị hủy
    /// </summary>
    public class InvoiceCancelledEvent : DomainEvent
    {
        public InvoiceId InvoiceId { get; }
        public string InvoiceNumber { get; }
        public string Reason { get; }
        public JournalEntryId ReversalJournalEntryId { get; }
        
        public InvoiceCancelledEvent(
            InvoiceId invoiceId,
            string invoiceNumber,
            string reason,
            JournalEntryId reversalJournalEntryId)
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            Reason = reason;
            ReversalJournalEntryId = reversalJournalEntryId;
        }
    }
}
