using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.ValueObjects;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Invoicing;
using AccountingERP.Domain.Exceptions;

namespace AccountingERP.Domain.Entities
{
    /// <summary>
    /// Bút toán (Journal Entry) - Entity chính trong hệ thống kế toán
    /// Tuân thủ TT99/2025/TT-BTC: Bắt buộc số chứng từ gốc và ngày chứng từ gốc
    /// 
    /// HARD ENFORCEMENT (Phase 1):
    /// - Không thể ghi sổ bút toán doanh thu không có InvoiceId
    /// - Hóa đơn phát hành phải có bút toán tương ứng
    /// </summary>
    public class JournalEntry : BaseEntity
    {
        // Thông tin chứng từ (Bắt buộc theo TT99)
        public string EntryNumber { get; private set; } = string.Empty;           // Số hiệu bút toán
        public string OriginalDocumentNumber { get; private set; } = string.Empty; // Số chứng từ gốc (BẮT BUỘC)
        public DateTime EntryDate { get; private set; }                            // Ngày ghi sổ
        public DateTime OriginalDocumentDate { get; private set; }                // Ngày chứng từ gốc (BẮT BUỘC)
        
        // Nội dung
        public string Description { get; private set; } = string.Empty;          // Diễn giải
        public string? Reference { get; private set; }                           // Số tham chiếu
        
        // Thông tin bổ sung
        public Guid? CustomerId { get; private set; }
        public Guid? SupplierId { get; private set; }
        public string? AttachmentPath { get; private set; }                      // Đường dẫn file đính kèm
        
        // HARD ENFORCEMENT: Link to Invoice for revenue entries
        public InvoiceId? InvoiceId { get; private set; }
        public bool RequiresInvoiceLink => IsRevenueEntry;
        
        // Trạng thái
        public JournalEntryStatus Status { get; private set; }
        public bool IsPosted { get; private set; }
        public DateTime? PostedDate { get; private set; }
        
        // Chi tiết bút toán (1-n)
        private readonly List<JournalEntryLine> _lines = new();
        public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

        // EF Core constructor
        private JournalEntry() : base() { }

        /// <summary>
        /// Tạo bút toán mới với validation đầy đủ theo TT99
        /// </summary>
        public static JournalEntry Create(
            string entryNumber,
            string originalDocumentNumber,
            DateTime entryDate,
            DateTime originalDocumentDate,
            string description,
            string? reference = null)
        {
            // Validation: Số hiệu bút toán
            if (string.IsNullOrWhiteSpace(entryNumber))
                throw new ArgumentException("TT99: Số hiệu bút toán không được để trống", nameof(entryNumber));
            
            // Validation: Số chứng từ gốc (TT99-Đ10 - BẮT BUỘC)
            if (string.IsNullOrWhiteSpace(originalDocumentNumber))
                throw new ArgumentException(
                    "TT99-Đ10: Số chứng từ gốc không được để trống. Đây là thông tin bắt buộc.", 
                    nameof(originalDocumentNumber));

            // Validation: Ngày ghi sổ không được trong tương lai
            if (entryDate > DateTime.Now.Date.AddDays(1))
                throw new ArgumentException(
                    "TT99-Đ10: Ngày ghi sổ không được là ngày trong tương lai.",
                    nameof(entryDate));

            // Validation: Ngày chứng từ gốc không được để trống
            if (originalDocumentDate == default)
                throw new ArgumentException(
                    "TT99-Đ10: Ngày chứng từ gốc không được để trống. Đây là thông tin bắt buộc.",
                    nameof(originalDocumentDate));

            // Validation: Ngày chứng từ gốc không được sau ngày ghi sổ
            if (originalDocumentDate > entryDate)
                throw new ArgumentException(
                    "TT99-Đ10: Ngày chứng từ gốc không được sau ngày ghi sổ.",
                    nameof(originalDocumentDate));

            // Validation: Ngày chứng từ gốc không được quá 1 năm trước
            if (originalDocumentDate < entryDate.AddYears(-1))
                throw new ArgumentException(
                    "TT99: Ngày chứng từ gốc không được quá 1 năm trước ngày ghi sổ.",
                    nameof(originalDocumentDate));

            // Validation: Ngày chứng từ gốc không được trong tương lai
            if (originalDocumentDate > DateTime.Now.Date)
                throw new ArgumentException(
                    "TT99: Ngày chứng từ gốc không được là ngày trong tương lai.",
                    nameof(originalDocumentDate));

            // Validation: Diễn giải không được để trống
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(
                    "TT99: Diễn giải không được để trống.",
                    nameof(description));

            return new JournalEntry
            {
                EntryNumber = entryNumber.Trim(),
                OriginalDocumentNumber = originalDocumentNumber.Trim(),
                EntryDate = entryDate.Date,
                OriginalDocumentDate = originalDocumentDate.Date,
                Description = description?.Trim() ?? string.Empty,
                Reference = reference?.Trim(),
                Status = JournalEntryStatus.Draft,
                IsPosted = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Thêm dòng chi tiết bút toán
        /// </summary>
        public void AddLine(string accountCode, decimal debitAmount, decimal creditAmount, string description)
        {
            if (IsPosted)
                throw new InvalidOperationException("TT99: Không thể chỉnh sửa bút toán đã ghi sổ");

            if (_lines.Count >= 99)
                throw new InvalidOperationException("TT99: Bút toán không được vượt quá 99 dòng chi tiết.");

            // Validation: Không cho phép Nợ và Có cùng lúc trên một dòng
            if (debitAmount > 0 && creditAmount > 0)
                throw new ArgumentException("Không thể có cả Nợ và Có trên cùng một dòng.");

            // Validation: Phải có Nợ hoặc Có
            if (debitAmount <= 0 && creditAmount <= 0)
                throw new ArgumentException("Phải có số tiền Nợ hoặc Có.");

            // Validation: TK 911 không được sử dụng
            if (accountCode.Trim() == "911")
                throw new InvalidOperationException(
                    "TK 911 (Xác định kết quả kinh doanh) không được sử dụng trong bút toán thông thường. " +
                    "TK này chỉ dùng để kết chuyển cuối kỳ.");

            var line = JournalEntryLine.Create(Id, accountCode, debitAmount, creditAmount, description);
            _lines.Add(line);
        }

        /// <summary>
        /// Ghi sổ bút toán
        /// </summary>
        public void Post(string postedBy)
        {
            if (IsPosted)
                throw new InvalidOperationException("Bút toán đã được ghi sổ");

            if (!_lines.Any())
                throw new InvalidOperationException("TT99: Bút toán phải có ít nhất một dòng chi tiết");

            // Kiểm tra cân bằng Nợ/Có
            var totalDebit = _lines.Sum(l => l.DebitAmount);
            var totalCredit = _lines.Sum(l => l.CreditAmount);
            
            if (totalDebit != totalCredit)
                throw new InvalidOperationException(
                    $"TT99: Bút toán không cân bằng. Tổng Nợ: {totalDebit:N2}, Tổng Có: {totalCredit:N2}");

            if (string.IsNullOrWhiteSpace(postedBy))
                throw new ArgumentException("Ngườii ghi sổ không được để trống", nameof(postedBy));

            // HARD ENFORCEMENT: Revenue entries must have InvoiceId
            if (IsRevenueEntry && InvoiceId == null)
                throw new InvoiceAccountingMismatchException(
                    "Không thể ghi sổ bút toán doanh thu không có hóa đơn. " +
                    "Theo TT78/2021, bút toán ghi nhận doanh thu (TK 511, 512, ...) phải có hóa đơn tương ứng.");

            IsPosted = true;
            PostedDate = DateTime.UtcNow;
            Status = JournalEntryStatus.Posted;
            UpdatedBy = postedBy.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Liên kết bút toán với hóa đơn
        /// </summary>
        public void LinkToInvoice(InvoiceId invoiceId)
        {
            if (IsPosted)
                throw new InvalidOperationException("Không thể liên kết bút toán đã ghi sổ với hóa đơn");
            
            if (InvoiceId != null)
                throw new InvalidOperationException("Bút toán đã được liên kết với hóa đơn khác");
            
            InvoiceId = invoiceId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Kiểm tra có phải bút toán doanh thu không
        /// Bút toán doanh thu có TK 511, 512, 515, 517 (Doanh thu bán hàng/dịch vụ)
        /// </summary>
        public bool IsRevenueEntry => _lines.Any(l => 
            l.AccountCode.StartsWith("511") || // Doanh thu bán hàng
            l.AccountCode.StartsWith("512") || // Doanh thu cung cấp dịch vụ
            l.AccountCode.StartsWith("515") || // Doanh thu hoạt động tài chính
            l.AccountCode.StartsWith("517"));  // Doanh thu khác

        /// <summary>
        /// Hủy bút toán (tạo bút toán đảo ngược)
        /// </summary>
        public JournalEntry CreateReversal(string reversalNumber, string reason, string createdBy)
        {
            if (!IsPosted)
                throw new InvalidOperationException("Chỉ có thể hủy bút toán đã ghi sổ");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do hủy không được để trống", nameof(reason));

            var reversal = new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryNumber = reversalNumber,
                OriginalDocumentNumber = $"REV-{OriginalDocumentNumber}",
                EntryDate = DateTime.Now.Date,
                OriginalDocumentDate = OriginalDocumentDate,
                Description = $"Đảo bút toán {EntryNumber}: {reason}",
                Status = JournalEntryStatus.Draft,
                IsPosted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy.Trim()
            };

            // Đảo các dòng: Nợ thành Có, Có thành Nợ
            foreach (var line in _lines)
            {
                if (line.DebitAmount > 0)
                    reversal.AddLine(line.AccountCode, 0, line.DebitAmount, line.Description);
                else
                    reversal.AddLine(line.AccountCode, line.CreditAmount, 0, line.Description);
            }

            Status = JournalEntryStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = createdBy.Trim();

            return reversal;
        }

        /// <summary>
        /// Tổng tiền Nợ
        /// </summary>
        public decimal TotalDebit => _lines.Sum(l => l.DebitAmount);

        /// <summary>
        /// Tổng tiền Có
        /// </summary>
        public decimal TotalCredit => _lines.Sum(l => l.CreditAmount);

        /// <summary>
        /// Bút toán có cân bằng không
        /// </summary>
        public bool IsBalanced => TotalDebit == TotalCredit;

        /// <summary>
        /// Số dòng chi tiết
        /// </summary>
        public int LineCount => _lines.Count;

        /// <summary>
        /// Tổng số tiền của bút toán (Nợ = Có)
        /// </summary>
        public decimal TotalAmount => TotalDebit; // Khi bút toán cân bằng, TotalDebit = TotalCredit
    }
}
