using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax
{
    /// <summary>
    /// Entity: Hóa đơn VAT (VAT Invoice)
    /// TT78/2021/TT-BTC: Quản lý hóa đơn điện tử
    /// </summary>
    public class VatInvoice : AggregateRoot
    {
        /// <summary>
        /// Số hóa đơn
        /// </summary>
        public string InvoiceNumber { get; private set; }

        /// <summary>
        /// Ký hiệu hóa đơn
        /// </summary>
        public string InvoiceSeries { get; private set; }

        /// <summary>
        /// Ngày phát hành hóa đơn
        /// </summary>
        public DateTime IssueDate { get; private set; }

        /// <summary>
        /// Loại hóa đơn (Bán ra / Mua vào)
        /// </summary>
        public InvoiceType Type { get; private set; }

        /// <summary>
        /// Trạng thái hóa đơn
        /// </summary>
        public InvoiceStatus Status { get; private set; }

        /// <summary>
        /// Mã số thuế ngườii bán
        /// </summary>
        public string SellerTaxCode { get; private set; }

        /// <summary>
        /// Tên ngườii bán
        /// </summary>
        public string SellerName { get; private set; }

        /// <summary>
        /// Mã số thuế ngườii mua
        /// </summary>
        public string BuyerTaxCode { get; private set; }

        /// <summary>
        /// Tên ngườii mua
        /// </summary>
        public string BuyerName { get; private set; }

        /// <summary>
        /// Tổng tiền hàng hóa/dịch vụ (chưa thuế)
        /// </summary>
        public Money TotalAmount { get; private set; }

        /// <summary>
        /// Thuế suất GTGT (%)
        /// </summary>
        public int VatRate { get; private set; }

        /// <summary>
        /// Tiền thuế GTGT
        /// </summary>
        public Money VatAmount { get; private set; }

        /// <summary>
        /// Tổng cộng tiền thanh toán
        /// </summary>
        public Money TotalPayment { get; private set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public Currency Currency { get; private set; }

        /// <summary>
        /// Tỷ giá (nếu là ngoại tệ)
        /// </summary>
        public decimal? ExchangeRate { get; private set; }

        /// <summary>
        /// Hóa đơn gốc (nếu là hóa đơn điều chỉnh/thay thế)
        /// </summary>
        public Guid? OriginalInvoiceId { get; private set; }

        /// <summary>
        /// Lý do điều chỉnh/thay thế/hủy
        /// </summary>
        public string? AdjustmentReason { get; private set; }

        /// <summary>
        /// Mã tra cứu trên hệ thống của Tổng cục Thuế
        /// </summary>
        public string? VerificationCode { get; private set; }

        /// <summary>
        /// File XML hóa đơn điện tử
        /// </summary>
        public string? XmlContent { get; private set; }

        /// <summary>
        /// Chi tiết hóa đơn
        /// </summary>
        public IReadOnlyCollection<VatInvoiceLine> Lines => _lines.AsReadOnly();
        private List<VatInvoiceLine> _lines = new();

        private VatInvoice() { }

        /// <summary>
        /// Tạo hóa đơn VAT
        /// </summary>
        public static VatInvoice Create(
            string invoiceNumber,
            string invoiceSeries,
            DateTime issueDate,
            InvoiceType type,
            string sellerTaxCode,
            string sellerName,
            string buyerTaxCode,
            string buyerName,
            int vatRate,
            Currency currency = Currency.VND,
            decimal? exchangeRate = null)
        {
            // Validation: Số hóa đơn không được trống
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new ArgumentException("TT78: Số hóa đơn không được để trống.", nameof(invoiceNumber));

            // Validation: Ngày phát hành không được trong tương lai
            if (issueDate > DateTime.Now.Date.AddDays(1))
                throw new ArgumentException("TT78: Ngày phát hành hóa đơn không được trong tương lai.", nameof(issueDate));

            // Validation: Thuế suất hợp lệ
            if (!IsValidVatRate(vatRate))
                throw new ArgumentException($"TT219: Thuế suất GTGT {vatRate}% không hợp lệ. Các mức hợp lệ: 0%, 5%, 8%, 10%.", nameof(vatRate));

            // Validation: MST ngườii bán
            if (string.IsNullOrWhiteSpace(sellerTaxCode))
                throw new ArgumentException("TT78: Mã số thuế ngườii bán không được để trống.", nameof(sellerTaxCode));

            // Validation: MST ngườii mua (bắt buộc với hóa đơn > 200K)
            if (type == InvoiceType.Output && string.IsNullOrWhiteSpace(buyerTaxCode))
                throw new ArgumentException("TT78: Mã số thuế ngườii mua không được để trống với hóa đơn GTGT.");

            var invoice = new VatInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber.Trim(),
                InvoiceSeries = invoiceSeries?.Trim() ?? "1C25TAA", // Default series
                IssueDate = issueDate.Date,
                Type = type,
                Status = InvoiceStatus.Valid,
                SellerTaxCode = sellerTaxCode.Trim(),
                SellerName = sellerName?.Trim() ?? string.Empty,
                BuyerTaxCode = buyerTaxCode?.Trim() ?? string.Empty,
                BuyerName = buyerName?.Trim() ?? string.Empty,
                VatRate = vatRate,
                Currency = currency,
                ExchangeRate = exchangeRate,
                TotalAmount = Money.Zero(currency),
                VatAmount = Money.Zero(currency),
                TotalPayment = Money.Zero(currency),
                CreatedAt = DateTime.UtcNow
            };

            return invoice;
        }

        /// <summary>
        /// Thêm dòng hàng hóa/dịch vụ
        /// </summary>
        public void AddLine(
            string productName,
            string unit,
            decimal quantity,
            decimal unitPrice,
            decimal? discount = null)
        {
            if (Status != InvoiceStatus.Valid)
                throw new InvalidOperationException($"Không thể thêm dòng vào hóa đơn đã {Status}.");

            var line = VatInvoiceLine.Create(
                Id,
                productName,
                unit,
                quantity,
                unitPrice,
                VatRate,
                Currency,
                discount);

            _lines.Add(line);
            RecalculateTotals();
        }

        /// <summary>
        /// Phát hành hóa đơn
        /// </summary>
        public void Issue(string verificationCode)
        {
            if (!_lines.Any())
                throw new InvalidOperationException("TT78: Hóa đơn phải có ít nhất một dòng hàng hóa/dịch vụ.");

            if (string.IsNullOrWhiteSpace(verificationCode))
                throw new ArgumentException("TT78: Mã tra cứu không được để trống.", nameof(verificationCode));

            VerificationCode = verificationCode.Trim();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new VatInvoiceIssuedEvent(Id, InvoiceNumber, IssueDate));
        }

        /// <summary>
        /// Điều chỉnh hóa đơn
        /// </summary>
        public VatInvoice CreateAdjustment(
            string newInvoiceNumber,
            string reason,
            decimal adjustmentAmount)
        {
            if (Status != InvoiceStatus.Valid)
                throw new InvalidOperationException("Chỉ có thể điều chỉnh hóa đơn đang hiệu lực.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("TT78: Lý do điều chỉnh không được để trống.", nameof(reason));

            var adjustment = Create(
                newInvoiceNumber,
                InvoiceSeries,
                DateTime.Now,
                Type,
                SellerTaxCode,
                SellerName,
                BuyerTaxCode,
                BuyerName,
                VatRate,
                Currency,
                ExchangeRate);

            adjustment.OriginalInvoiceId = this.Id;
            adjustment.AdjustmentReason = reason.Trim();

            Status = InvoiceStatus.Adjusted;
            UpdatedAt = DateTime.UtcNow;

            return adjustment;
        }

        /// <summary>
        /// Hủy hóa đơn
        /// </summary>
        public void Cancel(string reason)
        {
            if (Status != InvoiceStatus.Valid)
                throw new InvalidOperationException("Chỉ có thể hủy hóa đơn đang hiệu lực.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("TT78: Lý do hủy không được để trống.", nameof(reason));

            Status = InvoiceStatus.Cancelled;
            AdjustmentReason = reason.Trim();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new VatInvoiceCancelledEvent(Id, InvoiceNumber, reason));
        }

        /// <summary>
        /// Thay thế hóa đơn
        /// </summary>
        public VatInvoice CreateReplacement(string newInvoiceNumber, string reason)
        {
            if (Status != InvoiceStatus.Valid)
                throw new InvalidOperationException("Chỉ có thể thay thế hóa đơn đang hiệu lực.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("TT78: Lý do thay thế không được để trống.", nameof(reason));

            var replacement = Create(
                newInvoiceNumber,
                InvoiceSeries,
                DateTime.Now,
                Type,
                SellerTaxCode,
                SellerName,
                BuyerTaxCode,
                BuyerName,
                VatRate,
                Currency,
                ExchangeRate);

            // Copy all lines
            foreach (var line in _lines)
            {
                replacement.AddLine(
                    line.ProductName,
                    line.Unit,
                    line.Quantity,
                    line.UnitPrice,
                    line.Discount);
            }

            replacement.OriginalInvoiceId = this.Id;
            replacement.AdjustmentReason = $"Thay thế hóa đơn {InvoiceNumber}: {reason}";

            Status = InvoiceStatus.Replaced;
            UpdatedAt = DateTime.UtcNow;

            return replacement;
        }

        private void RecalculateTotals()
        {
            var total = _lines.Sum(l => l.TotalAmount.Amount);
            var vat = _lines.Sum(l => l.VatAmount.Amount);

            TotalAmount = Money.Create(total, Currency);
            VatAmount = Money.Create(vat, Currency);
            TotalPayment = Money.Create(total + vat, Currency);
        }

        private static bool IsValidVatRate(int rate)
        {
            return rate == 0 || rate == 5 || rate == 8 || rate == 10;
        }

        /// <summary>
        /// Kiểm tra hóa đơn có hợp lệ không
        /// </summary>
        public bool IsValid => Status == InvoiceStatus.Valid;

        /// <summary>
        /// Kiểm tra hóa đơn đã phát hành chưa
        /// </summary>
        public bool IsIssued => !string.IsNullOrWhiteSpace(VerificationCode);
    }

    /// <summary>
    /// Chi tiết hóa đơn VAT
    /// </summary>
    public class VatInvoiceLine
    {
        public Guid Id { get; private set; }
        public Guid VatInvoiceId { get; private set; }
        public int LineNumber { get; private set; }
        public string ProductName { get; private set; }
        public string Unit { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Discount { get; private set; }
        public Money TotalAmount { get; private set; }
        public int VatRate { get; private set; }
        public Money VatAmount { get; private set; }
        public Money TotalPayment { get; private set; }

        private VatInvoiceLine() { }

        public static VatInvoiceLine Create(
            Guid vatInvoiceId,
            string productName,
            string unit,
            decimal quantity,
            decimal unitPrice,
            int vatRate,
            Currency currency,
            decimal? discount = null)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Tên hàng hóa/dịch vụ không được để trống.", nameof(productName));

            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(quantity));

            if (unitPrice < 0)
                throw new ArgumentException("Đơn giá không được âm.", nameof(unitPrice));

            var disc = discount ?? 0;
            var amount = (quantity * unitPrice) - disc;
            var vat = amount * vatRate / 100;

            return new VatInvoiceLine
            {
                Id = Guid.NewGuid(),
                VatInvoiceId = vatInvoiceId,
                ProductName = productName.Trim(),
                Unit = unit?.Trim() ?? "cái",
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = disc,
                TotalAmount = Money.Create(amount, currency),
                VatRate = vatRate,
                VatAmount = Money.Create(vat, currency),
                TotalPayment = Money.Create(amount + vat, currency)
            };
        }
    }

    public enum InvoiceType
    {
        Input = 1,   // Hóa đơn mua vào
        Output = 2   // Hóa đơn bán ra
    }

    public enum InvoiceStatus
    {
        Draft = 1,      // Bản nháp
        Valid = 2,      // Hiệu lực
        Adjusted = 3,   // Đã điều chỉnh
        Replaced = 4,   // Đã thay thế
        Cancelled = 5   // Đã hủy
    }
}
