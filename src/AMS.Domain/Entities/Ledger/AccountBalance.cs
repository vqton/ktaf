namespace AMS.Domain.Entities;

/// <summary>
/// Tracks account balances per fiscal period for quick reporting.
/// Updated when vouchers are posted or during month-end closing.
/// </summary>
public class AccountBalance : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the fiscal period.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Foreign key to the chart of accounts.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Account code for quick reference.
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Opening debit balance brought forward from previous period.
    /// </summary>
    public decimal OpeningDebit { get; set; }

    /// <summary>
    /// Opening credit balance brought forward from previous period.
    /// </summary>
    public decimal OpeningCredit { get; set; }

    /// <summary>
    /// Total debit amount during this period.
    /// </summary>
    public decimal PeriodDebit { get; set; }

    /// <summary>
    /// Total credit amount during this period.
    /// </summary>
    public decimal PeriodCredit { get; set; }

    /// <summary>
    /// Closing debit balance (Opening + Period movement).
    /// </summary>
    public decimal ClosingDebit { get; set; }

    /// <summary>
    /// Closing credit balance (Opening + Period movement).
    /// </summary>
    public decimal ClosingCredit { get; set; }

    /// <summary>
    /// Navigation property to the fiscal period.
    /// </summary>
    public FiscalPeriod? FiscalPeriod { get; set; }

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public ChartOfAccounts? Account { get; set; }

    /// <summary>
    /// Recalculates closing balances based on opening and period movements.
    /// </summary>
    public void RecalculateClosing()
    {
        var netMovement = PeriodDebit - PeriodCredit;
        if (netMovement > 0)
        {
            ClosingDebit = OpeningDebit + netMovement;
            ClosingCredit = 0;
        }
        else if (netMovement < 0)
        {
            ClosingDebit = 0;
            ClosingCredit = OpeningCredit + Math.Abs(netMovement);
        }
        else
        {
            ClosingDebit = OpeningDebit;
            ClosingCredit = OpeningCredit;
        }
    }

    /// <summary>
    /// Applies a debit amount to this balance.
    /// </summary>
    public void ApplyDebit(decimal amount)
    {
        PeriodDebit += amount;
        RecalculateClosing();
    }

    /// <summary>
    /// Applies a credit amount to this balance.
    /// </summary>
    public void ApplyCredit(decimal amount)
    {
        PeriodCredit += amount;
        RecalculateClosing();
    }
}
