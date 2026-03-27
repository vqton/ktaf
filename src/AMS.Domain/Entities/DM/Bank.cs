using AMS.Domain.Entities;

namespace AMS.Domain.Entities.DM;

/// <summary>
/// Represents a bank (Ngân hàng).
/// </summary>
public class Bank : BaseLookupEntity
{
    /// <summary>
    /// Bank SWIFT code (Mã SWIFT).
    /// </summary>
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Bank logo path (Đường dẫn logo).
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Bank branch name (Chi nhánh).
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Bank address (Địa chỉ).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Phone number (Điện thoại).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Collection of bank accounts for this bank.
    /// </summary>
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}

/// <summary>
/// Represents a bank account (Tài khoản ngân hàng).
/// </summary>
public class BankAccount : BaseAuditEntity
{
    /// <summary>
    /// Bank ID.
    /// </summary>
    public Guid BankId { get; set; }

    /// <summary>
    /// Account number (Số tài khoản).
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Account name (Tên tài khoản).
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Account type: CHECKING, SAVINGS, TERM (Loại TK: TKTT, TKTG, Tiết kiệm).
    /// </summary>
    public string AccountType { get; set; } = "CHECKING";

    /// <summary>
    /// Currency code (Mã tiền tệ).
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Opening balance (Số dư đầu kỳ).
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Is primary account (Tài khoản chính).
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Is active for transactions.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bank branch (Chi nhánh).
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Account holder name (Chủ tài khoản).
    /// </summary>
    public string? AccountHolder { get; set; }

    /// <summary>
    /// Navigation property to Bank.
    /// </summary>
    public Bank? Bank { get; set; }

    /// <summary>
    /// Collection of bank transactions.
    /// </summary>
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}

/// <summary>
/// Transaction type: DEPOSIT, WITHDRAWAL, TRANSFER_IN, TRANSFER_OUT, FEE, INTEREST
/// </summary>
public enum BankTransactionType
{
    Deposit,
    Withdrawal,
    TransferIn,
    TransferOut,
    Fee,
    Interest
}

/// <summary>
/// Bank transaction status: PENDING, COMPLETED, CANCELLED
/// </summary>
public enum BankTransactionStatus
{
    Pending,
    Completed,
    Cancelled
}

/// <summary>
/// Represents a bank transaction (Giao dịch ngân hàng).
/// </summary>
public class BankTransaction : BaseAuditEntity
{
    /// <summary>
    /// Bank account ID.
    /// </summary>
    public Guid BankAccountId { get; set; }

    /// <summary>
    /// Transaction date (Ngày giao dịch).
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Transaction type (Loại giao dịch).
    /// </summary>
    public BankTransactionType TransactionType { get; set; }

    /// <summary>
    /// Transaction amount (Số tiền).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Fee amount (Phí giao dịch).
    /// </summary>
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Description/Nội dung giao dịch.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reference number (Số tham chiếu).
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Transaction status (Trạng thái).
    /// </summary>
    public BankTransactionStatus Status { get; set; } = BankTransactionStatus.Pending;

    /// <summary>
    /// Is reconciled (Đã đối chiếu).
    /// </summary>
    public bool IsReconciled { get; set; }

    /// <summary>
    /// Reconciliation date (Ngày đối chiếu).
    /// </summary>
    public DateTime? ReconciledDate { get; set; }

    /// <summary>
    /// Voucher ID if linked to accounting voucher.
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Partner name (Đối tác).
    /// </summary>
    public string? PartnerName { get; set; }

    /// <summary>
    /// Partner bank account (TK đối tác).
    /// </summary>
    public string? PartnerAccountNumber { get; set; }

    /// <summary>
    /// Navigation property to BankAccount.
    /// </summary>
    public BankAccount? BankAccount { get; set; }

    /// <summary>
    /// Navigation property to Voucher.
    /// </summary>
    public Voucher? Voucher { get; set; }
}
