using System;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Entities
{
    /// <summary>
    /// Chi tiết bút toán (Journal Entry Line)
    /// TT99: Mỗi dòng phản ánh một bên Nợ hoặc Có của tài khoản
    /// </summary>
    public class JournalEntryLine : BaseEntity
    {
        public Guid JournalEntryId { get; private set; }
        public string AccountCode { get; private set; } = string.Empty;
        public decimal DebitAmount { get; private set; }
        public decimal CreditAmount { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string? CostCenterCode { get; private set; }
        public string? ProjectCode { get; private set; }
        public string? CustomerCode { get; private set; }
        public string? SupplierCode { get; private set; }
        
        public JournalEntry JournalEntry { get; private set; } = null!;

        private JournalEntryLine() : base() { }

        /// <summary>
        /// Tạo dòng chi tiết Nợ
        /// </summary>
        public static JournalEntryLine CreateDebit(
            Guid journalEntryId,
            string accountCode,
            decimal amount,
            string description,
            string? costCenterCode = null,
            string? projectCode = null)
        {
            ValidateInputs(journalEntryId, accountCode, amount, description);

            return new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntryId,
                AccountCode = accountCode.Trim(),
                DebitAmount = amount,
                CreditAmount = 0,
                Description = description.Trim(),
                CostCenterCode = costCenterCode?.Trim(),
                ProjectCode = projectCode?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo dòng chi tiết Có
        /// </summary>
        public static JournalEntryLine CreateCredit(
            Guid journalEntryId,
            string accountCode,
            decimal amount,
            string description,
            string? costCenterCode = null,
            string? projectCode = null)
        {
            ValidateInputs(journalEntryId, accountCode, amount, description);

            return new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntryId,
                AccountCode = accountCode.Trim(),
                DebitAmount = 0,
                CreditAmount = amount,
                Description = description.Trim(),
                CostCenterCode = costCenterCode?.Trim(),
                ProjectCode = projectCode?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Factory method chung
        /// </summary>
        public static JournalEntryLine Create(
            Guid journalEntryId,
            string accountCode,
            decimal debitAmount,
            decimal creditAmount,
            string description)
        {
            if (debitAmount > 0 && creditAmount > 0)
                throw new ArgumentException(
                    "TT99: Một dòng chi tiết chỉ được phép có Nợ HOẶC Có, không cả hai.");

            if (debitAmount > 0)
                return CreateDebit(journalEntryId, accountCode, debitAmount, description);
            
            if (creditAmount > 0)
                return CreateCredit(journalEntryId, accountCode, creditAmount, description);

            throw new ArgumentException(
                "TT99: Dòng chi tiết phải có số tiền Nợ hoặc Có lớn hơn 0.");
        }

        private static void ValidateInputs(Guid journalEntryId, string accountCode, decimal amount, string description)
        {
            if (journalEntryId == Guid.Empty)
                throw new ArgumentException("ID bút toán không hợp lệ", nameof(journalEntryId));

            if (string.IsNullOrWhiteSpace(accountCode))
                throw new ArgumentException(
                    "TT99: Mã tài khoản không được để trống", 
                    nameof(accountCode));

            // Validate account code format
            var trimmedCode = accountCode.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedCode, @"^[1-9]\d{2,4}$"))
                throw new ArgumentException(
                    $"TT99: Mã tài khoản '{trimmedCode}' không hợp lệ. " +
                    "Mã TK phải có 3-5 chữ số, bắt đầu bằng 1-9.",
                    nameof(accountCode));

            // TK 911 restriction
            if (trimmedCode == "911")
                throw new InvalidOperationException(
                    "TK 911 (Xác định kết quả kinh doanh) không được sử dụng trong bút toán thông thường. " +
                    "TK này chỉ dùng để kết chuyển cuối kỳ.");

            if (amount <= 0)
                throw new ArgumentException(
                    "TT99: Số tiền phải lớn hơn 0", 
                    nameof(amount));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(
                    "TT99: Diễn giải không được để trống", 
                    nameof(description));
        }

        /// <summary>
        /// Kiểm tra dòng này là bên Nợ
        /// </summary>
        public bool IsDebit => DebitAmount > 0;

        /// <summary>
        /// Kiểm tra dòng này là bên Có
        /// </summary>
        public bool IsCredit => CreditAmount > 0;

        /// <summary>
        /// Lấy số tiền (bất kể Nợ hay Có)
        /// </summary>
        public decimal Amount => DebitAmount > 0 ? DebitAmount : CreditAmount;

        /// <summary>
        /// Lấy số tiền dưới dạng Money
        /// </summary>
        public Money GetMoney() => Money.VND(Amount);
    }
}
