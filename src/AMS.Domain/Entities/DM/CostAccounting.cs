using AMS.Domain.Entities;
using AMS.Domain.Entities.HR;

namespace AMS.Domain.Entities.DM;

public enum CostAllocationMethod
{
    Direct,
    Percentage,
    SquareMeter,
    NumberOfEmployees,
    MachineHours,
    LaborHours
}

public class CostCenter : BaseAuditEntity
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string CostCenterType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
}

public class CostAllocation : BaseAuditEntity
{
    public Guid CostCenterId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public CostAllocationMethod AllocationMethod { get; set; }
    public DateTime AllocationDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public CostCenter? CostCenter { get; set; }
}

public class CostAllocationDetail : BaseAuditEntity
{
    public Guid CostAllocationId { get; set; }
    public Guid TargetCostCenterId { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
    public CostAllocation? CostAllocation { get; set; }
    public CostCenter? TargetCostCenter { get; set; }
}

public class CostReport : BaseAuditEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid CostCenterId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public decimal DirectCost { get; set; }
    public decimal AllocatedCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal Variance { get; set; }
    public string Description { get; set; } = string.Empty;
    public CostCenter? CostCenter { get; set; }
}