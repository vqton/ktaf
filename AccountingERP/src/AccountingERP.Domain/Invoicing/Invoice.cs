using System;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Invoicing
{
    /// <summary>
    /// Strongly-typed ID for Invoice
    /// </summary>
    public readonly struct InvoiceId : IEquatable<InvoiceId>
    {
        public Guid Value { get; }
        
        public InvoiceId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("InvoiceId cannot be empty", nameof(value));
            Value = value;
        }
        
        public static InvoiceId New() => new(Guid.NewGuid());
        public static InvoiceId FromGuid(Guid guid) => new(guid);
        
        public bool Equals(InvoiceId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is InvoiceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        
        public static bool operator ==(InvoiceId left, InvoiceId right) => left.Equals(right);
        public static bool operator !=(InvoiceId left, InvoiceId right) => !left.Equals(right);
    }

    /// <summary>
    /// Trạng thái hóa đơn
    /// </summary>
    public enum InvoiceStatus
    {
        Draft = 0,
        Issued = 1,
        Cancelled = 2,
        Replaced = 3
    }

    /// <summary>
    /// Loại hóa đơn
    /// </summary>
    public enum InvoiceType
    {
        Sales = 1,      // Hóa đơn bán hàng
        Purchase = 2    // Hóa đơn mua hàng
    }

    /// <summary>
    /// Aggregate: Hóa đơn (Invoice)
    /// TT78/2021/TT-BTC: Quản lý hóa đơn điện tử
    /// 
    /// HARD ENFORCEMENT RULES:
    /// 1. Không thể phát hành hóa đơn không có bút toán kế toán tương ứng
    /// 2. Không thể tạo bút toán doanh thu không có hóa đơn
    /// 3. Hủy hóa đơn phải tạo bút toán đảo ngược
    /// </summary>
    public class Invoice : AggregateRoot
    {
        public InvoiceId InvoiceId { get; private set; }
        public string InvoiceNumber { get; private set; } = string.Empty;
        public string InvoiceSeries { get; private set; } = string.Empty;
        public DateTime IssueDate { get; private set; }
        public InvoiceType Type { get; private set; }
        public InvoiceStatus Status { get; private set; }
        
        // Customer info
        public string CustomerCode { get; private set; } = string.Empty;
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerTaxCode { get; private set; } = string.Empty;
        
        // Amounts
        public Money SubTotal { get; private set; } = Money.Zero(Currency.VND);
        public Money VatAmount { get; private set; } = Money.Zero(Currency.VND);
        public Money TotalAmount { get; private set; } = Money.Zero(Currency.VND);
        public int VatRate { get; private set; }
        public Currency Currency { get; private set; }
        
        // Hard Enforcement: Link to JournalEntry
        public JournalEntryId? JournalEntryId { get; private set; }
        public JournalEntryId? ReversalJournalEntryId { get; private set; }
        
        // Line items
        private List<InvoiceLine> _lines = new();
        public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();
        
        // E-Invoice integration
        public string? ExternalInvoiceId { get; private set; }
        public string? VerificationCode { get; private set; }
        
        // Cancellation
        public string? CancellationReason { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        
        private Invoice() { } // EF Core
        
        /// <summary>
        /// Tạo hóa đơn mới (Draft status)
        /// </summary>
        public static Invoice CreateDraft(
            string invoiceNumber,
            string invoiceSeries,
            DateTime issueDate,
            InvoiceType type,
            string customerCode,
            string customerName,
            string customerTaxCode,
            int vatRate,
            string createdBy,
            Currency currency = Currency.VND)
        {
            ValidateInvoiceNumber(invoiceNumber);
            ValidateVatRate(vatRate);
            ValidateCustomerInfo(customerCode, customerName);
            
            var invoice = new Invoice();
            invoice.InvoiceId = InvoiceId.New();
            invoice.InvoiceNumber = invoiceNumber.Trim();
            invoice.InvoiceSeries = invoiceSeries?.Trim() ?? "1C25TAA";
            invoice.IssueDate = issueDate.Date;
            invoice.Type = type;
            invoice.Status = InvoiceStatus.Draft;
            invoice.CustomerCode = customerCode.Trim();
            invoice.CustomerName = customerName.Trim();
            invoice.CustomerTaxCode = customerTaxCode?.Trim() ?? string.Empty;
            invoice.VatRate = vatRate;
            invoice.Currency = currency;
            invoice.SubTotal = Money.Zero(currency);
            invoice.VatAmount = Money.Zero(currency);
            invoice.TotalAmount = Money.Zero(currency);
            // Note: CreatedAt and Id are set by BaseEntity constructor
            // CreatedBy will be tracked at application layer
            
            return invoice;
        }
        
        /// <summary>
        /// Thêm dòng hàng hóa/dịch vụ
        /// </summary>
        public void AddLine(
            string productCode,
            string productName,
            string unit,
            decimal quantity,
            decimal unitPrice,
            decimal? discount = null)
        {
            if (Status != InvoiceStatus.Draft)
                throw new InvalidOperationException($"Không thể thêm dòng vào hóa đơn đã {Status}.");
            
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));
            
            if (unitPrice < 0)
                throw new ArgumentException("Đơn giá không được âm", nameof(unitPrice));
            
            var line = InvoiceLine.Create(
                InvoiceId,
                productCode,
                productName,
                unit,
                quantity,
                unitPrice,
                Currency,
                discount);
            
            _lines.Add(line);
            RecalculateTotals();
            // UpdatedAt is inherited from BaseEntity
        }
        
        /// <summary>
        /// Phát hành hóa đơn - HARD ENFORCEMENT: Phải tạo bút toán kế toán
        /// </summary>
        public void Issue(string verificationCode, JournalEntryId? journalEntryId, string issuedBy)
        {
            if (Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Chỉ có thể phát hành hóa đơn ở trạng thái Draft.");
            
            if (!_lines.Any())
                throw new InvalidOperationException("TT78: Hóa đơn phải có ít nhất một dòng hàng hóa/dịch vụ.");
            
            if (string.IsNullOrWhiteSpace(verificationCode))
                throw new ArgumentException("TT78: Mã tra cứu không được để trống.", nameof(verificationCode));
            
            // HARD ENFORCEMENT: Must have corresponding JournalEntry
            if (journalEntryId == null || journalEntryId.Value.Value == Guid.Empty)
                throw new InvoiceAccountingMismatchException(
                    "Không thể phát hành hóa đơn không có bút toán kế toán tương ứng. " +
                    "Theo TT78/2021, hóa đơn phải khớp với sổ kế toán.");
            
            VerificationCode = verificationCode.Trim();
            JournalEntryId = journalEntryId.Value;
            Status = InvoiceStatus.Issued;
            // UpdatedAt is inherited from BaseEntity
            
            AddDomainEvent(new InvoiceIssuedEvent(InvoiceId, InvoiceNumber, IssueDate, journalEntryId.Value));
        }
        
        /// <summary>
        /// Hủy hóa đơn - HARD ENFORCEMENT: Phải tạo bút toán đảo ngược
        /// </summary>
        public void Cancel(string reason, JournalEntryId? reversalJournalEntryId, string cancelledBy)
        {
            if (Status != InvoiceStatus.Issued)
                throw new InvalidOperationException("Chỉ có thể hủy hóa đơn đã phát hành.");
            
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("TT78: Lý do hủy không được để trống.", nameof(reason));
            
            // HARD ENFORCEMENT: Must create reversal JournalEntry
            if (reversalJournalEntryId == null || reversalJournalEntryId.Value.Value == Guid.Empty)
                throw new InvoiceAccountingMismatchException(
                    "Không thể hủy hóa đơn không tạo bút toán đảo ngược. " +
                    "Hủy hóa đơn phải ghi nhận điều chỉnh trong sổ kế toán.");
            
            // HARD ENFORCEMENT: Must have original JournalEntry to reverse
            if (JournalEntryId == null)
                throw new InvoiceAccountingMismatchException(
                    "Hóa đơn không có bút toán gốc để đảo ngược.");
            
            Status = InvoiceStatus.Cancelled;
            CancellationReason = reason.Trim();
            CancelledAt = DateTime.UtcNow;
            ReversalJournalEntryId = reversalJournalEntryId.Value;
            // UpdatedAt is inherited from BaseEntity
            
            AddDomainEvent(new InvoiceCancelledEvent(InvoiceId, InvoiceNumber, reason, reversalJournalEntryId.Value));
        }
        
        /// <summary>
        /// Cập nhật External Invoice ID từ hệ thống e-invoice
        /// </summary>
        public void SetExternalInvoiceId(string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("External ID không được trống", nameof(externalId));
            
            ExternalInvoiceId = externalId;
            // UpdatedAt is inherited from BaseEntity
        }
        
        private void RecalculateTotals()
        {
            var subTotal = _lines.Sum(l => l.LineTotal.Amount);
            var vatAmount = subTotal * VatRate / 100m;
            
            SubTotal = Money.Create(subTotal, Currency);
            VatAmount = Money.Create(vatAmount, Currency);
            TotalAmount = Money.Create(subTotal + vatAmount, Currency);
        }
        
        private static void ValidateInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new ArgumentException("TT78: Số hóa đơn không được để trống.", nameof(invoiceNumber));
            
            if (invoiceNumber.Length > 20)
                throw new ArgumentException("TT78: Số hóa đơn không được vượt quá 20 ký tự.", nameof(invoiceNumber));
        }
        
        private static void ValidateVatRate(int vatRate)
        {
            var validRates = new[] { 0, 5, 8, 10 };
            if (!validRates.Contains(vatRate))
                throw new ArgumentException($"TT219: Thuế suất GTGT {vatRate}% không hợp lệ. Các mức hợp lệ: 0%, 5%, 8%, 10%.", nameof(vatRate));
        }
        
        private static void ValidateCustomerInfo(string customerCode, string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
                throw new ArgumentException("Mã khách hàng không được để trống", nameof(customerCode));
            
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Tên khách hàng không được để trống", nameof(customerName));
        }
    }
}
