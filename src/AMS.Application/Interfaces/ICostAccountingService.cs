using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface ICostAccountingService
{
    Task<ServiceResult<CostCenterDto>> CreateCostCenterAsync(CreateCostCenterDto dto);
    Task<ServiceResult<CostCenterDto>> UpdateCostCenterAsync(Guid id, CreateCostCenterDto dto);
    Task<ServiceResult<CostCenterDto>> GetCostCenterByIdAsync(Guid id);
    Task<ServiceResult<List<CostCenterDto>>> GetAllCostCentersAsync(int page, int pageSize);
    Task<ServiceResult<List<CostCenterDto>>> GetCostCentersByDepartmentAsync(Guid departmentId);
    
    Task<ServiceResult<CostAllocationDto>> CreateCostAllocationAsync(CreateCostAllocationDto dto);
    Task<ServiceResult<CostAllocationDto>> GetCostAllocationByIdAsync(Guid id);
    Task<ServiceResult<List<CostAllocationDto>>> GetCostAllocationsByPeriodAsync(int year, int month);
    Task<ServiceResult<CostAllocationDto>> ApproveCostAllocationAsync(Guid id);
    
    Task<ServiceResult<CostReportDto>> GetCostReportAsync(int year, int month, Guid? costCenterId = null);
    Task<ServiceResult<CostReportDto>> GetCostVarianceReportAsync(int year, int month, Guid? costCenterId = null);
    Task<ServiceResult<CostReportDto>> GenerateCostReportAsync(int year, int month, Guid? costCenterId = null);
}

public class CostCenterDto
{
    public Guid Id { get; set; }
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string CostCenterType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}

public class CreateCostCenterDto
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string CostCenterType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class CostAllocationDto
{
    public Guid Id { get; set; }
    public Guid CostCenterId { get; set; }
    public string CostCenterName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string AllocationMethod { get; set; } = string.Empty;
    public DateTime AllocationDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<CostAllocationDetailDto> Details { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class CreateCostAllocationDto
{
    public Guid CostCenterId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public string AllocationMethod { get; set; } = "Direct";
    public string Description { get; set; } = string.Empty;
    public List<CostAllocationDetailDto> Details { get; set; } = new();
}

public class CostAllocationDetailDto
{
    public Guid TargetCostCenterId { get; set; }
    public string TargetCostCenterName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CostReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public decimal DirectCost { get; set; }
    public decimal AllocatedCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public List<CostReportDetailDto> Details { get; set; } = new();
}

public class CostReportDetailDto
{
    public Guid CostCenterId { get; set; }
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public decimal DirectCost { get; set; }
    public decimal AllocatedCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal Variance { get; set; }
}