using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface IBankReconciliationService
{
    Task<ServiceResult<BankReconciliationDto>> CreateReconciliationAsync(CreateBankReconciliationDto dto);
    Task<ServiceResult<BankReconciliationDto>> GetReconciliationByIdAsync(Guid id);
    Task<ServiceResult<List<BankReconciliationDto>>> GetReconciliationsByAccountAsync(Guid bankAccountId);
    Task<ServiceResult<BankReconciliationDto>> ReconcileAsync(Guid reconciliationId, List<ReconciliationItemDto> items);
    Task<ServiceResult<BankReconciliationDto>> ApproveReconciliationAsync(Guid reconciliationId, string approvedBy);
    Task<ServiceResult<BankReconciliationDto>> CancelReconciliationAsync(Guid reconciliationId);
    Task<ServiceResult<ReconciliationReportDto>> GetReconciliationReportAsync(Guid bankAccountId, DateTime fromDate, DateTime toDate);
    Task<ServiceResult> UpdateStatementBalanceAsync(Guid reconciliationId, decimal statementBalance);
    Task<ServiceResult> CompleteReconciliationAsync(Guid reconciliationId, string approvedBy);
}

public class BankReconciliationDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public DateTime StatementDate { get; set; }
    public decimal BankBalance { get; set; }
    public decimal BookBalance { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<ReconciliationItemDto> Items { get; set; } = new();
}

public class CreateBankReconciliationDto
{
    public Guid BankAccountId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ReconciliationItemDto
{
    public Guid Id { get; set; }
    public Guid BankTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public bool IsReconciled { get; set; }
    public string? Notes { get; set; }
}

public class ReconciliationReportDto
{
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBankBalance { get; set; }
    public decimal ClosingBankBalance { get; set; }
    public decimal OpeningBookBalance { get; set; }
    public decimal ClosingBookBalance { get; set; }
    public decimal TotalReconciledAmount { get; set; }
    public decimal TotalUnreconciledAmount { get; set; }
    public List<ReconciliationItemDto> ReconciledItems { get; set; } = new();
    public List<ReconciliationItemDto> UnreconciledItems { get; set; } = new();
}