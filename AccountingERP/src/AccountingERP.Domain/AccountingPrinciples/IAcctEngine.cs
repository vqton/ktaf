using System;

namespace AccountingERP.Domain.AccountingPrinciples;
public interface IAcctEngine
{
    void RecordAccrual(AccrualEvent accrual);
    void ReverseAccrual(string accrualId);
    void AllocatePrepaid(PrepaidAllocationRequest request);
    void CreateProvision(ProvisionRequest request);
}

public class AccrualEvent
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string RevenueAccount { get; set; } = string.Empty;
    public string ExpenseAccount { get; set; } = string.Empty;
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

public class PrepaidAllocationRequest
{
    public decimal Amount { get; set; }
    public int Months { get; set; }
    public string AssetAccount { get; set; } = string.Empty;
}

public class ProvisionRequest
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime AsOf { get; set; }
}
