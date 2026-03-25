using AMS.Domain.Entities.DM;

namespace AMS.Domain.Entities;

/// <summary>
/// Bank reconciliation status: DRAFT, IN_PROGRESS, COMPLETED
/// </summary>
public enum BankReconciliationStatus
{
    Draft,
    InProgress,
    Completed,
    Cancelled
}

/// <summary>
/// Represents a bank reconciliation (Đối chiếu sao kê ngân hàng).
/// </summary>
public class BankReconciliation : BaseAuditEntity
{
    /// <summary>
    /// Bank account ID.
    /// </summary>
    public Guid BankAccountId { get; set; }

    /// <summary>
    /// Reconciliation period (Kỳ đối chiếu).
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Reconciliation month (Tháng đối chiếu).
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Statement closing balance from bank (Số dư cuối kỳ ngân hàng).
    /// </summary>
    public decimal StatementClosingBalance { get; set; }

    /// <summary>
    /// Book closing balance (Số dư cuối kỳ sổ sách).
    /// </summary>
    public decimal BookClosingBalance { get; set; }

    /// <summary>
    /// Difference (Chênh lệch).
    /// </summary>
    public decimal Difference => StatementClosingBalance - BookClosingBalance;

    /// <summary>
    /// In transit deposits (Các khoản chuyển chưa nhận).
    /// </summary>
    public decimal InTransitDeposits { get; set; }

    /// <summary>
    /// Outstanding checks (Các khoản chưa chi).
    /// public decimal OutstandingChecks { get; set; }

    /// <summary>
    /// Bank fees not recorded (Phí ngân hàng chưa hạch toán).
    /// </summary>
    public decimal UnrecordedBankFees { get; set; }

    /// <summary>
    /// Interest not recorded (Lãi chưa hạch toán).
    /// </summary>
    public decimal UnrecordedInterest { get; set; }

    /// <summary>
    /// Reconciliation date (Ngày đối chiếu).
    /// </summary>
    public DateTime ReconciliationDate { get; set; }

    /// <summary>
    /// Status (Trạng thái).
    /// </summary>
    public BankReconciliationStatus Status { get; set; } = BankReconciliationStatus.Draft;

    /// <summary>
    /// Notes/Ghi chú.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Prepared by (Người lập).
    /// </summary>
    public string? PreparedBy { get; set; }

    /// <summary>
    /// Approved by (Người duyệt).
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Navigation property to BankAccount.
    /// </summary>
    public BankAccount? BankAccount { get; set; }
}
