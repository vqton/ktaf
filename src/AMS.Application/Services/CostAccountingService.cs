using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Entities.HR;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class CostAccountingService : ICostAccountingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly ICostAllocationRepository _costAllocationRepository;
    private readonly ICostAllocationDetailRepository _costAllocationDetailRepository;
    private readonly ICostReportRepository _costReportRepository;

    public CostAccountingService(
        IUnitOfWork unitOfWork,
        ICostCenterRepository costCenterRepository,
        ICostAllocationRepository costAllocationRepository,
        ICostAllocationDetailRepository costAllocationDetailRepository,
        ICostReportRepository costReportRepository)
    {
        _unitOfWork = unitOfWork;
        _costCenterRepository = costCenterRepository;
        _costAllocationRepository = costAllocationRepository;
        _costAllocationDetailRepository = costAllocationDetailRepository;
        _costReportRepository = costReportRepository;
    }

    public async Task<ServiceResult<CostCenterDto>> CreateCostCenterAsync(CreateCostCenterDto dto)
    {
        var existing = await _costCenterRepository.GetByCodeAsync(dto.CostCenterCode);
        if (existing != null)
            return ServiceResult<CostCenterDto>.Failure($"Mã trung tâm chi phí '{dto.CostCenterCode}' đã tồn tại.");

        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(),
            CostCenterCode = dto.CostCenterCode,
            CostCenterName = dto.CostCenterName,
            ParentId = dto.ParentId,
            CostCenterType = dto.CostCenterType,
            Description = dto.Description,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _costCenterRepository.AddAsync(costCenter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<CostCenterDto>.Success(MapToDto(costCenter));
    }

    public async Task<ServiceResult<CostCenterDto>> UpdateCostCenterAsync(Guid id, CreateCostCenterDto dto)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(id);
        if (costCenter == null)
            return ServiceResult<CostCenterDto>.Failure("Trung tâm chi phí không tồn tại.");

        costCenter.CostCenterCode = dto.CostCenterCode;
        costCenter.CostCenterName = dto.CostCenterName;
        costCenter.ParentId = dto.ParentId;
        costCenter.CostCenterType = dto.CostCenterType;
        costCenter.Description = dto.Description;
        costCenter.DepartmentId = dto.DepartmentId;
        costCenter.ModifiedAt = DateTime.UtcNow;

        await _costCenterRepository.UpdateAsync(costCenter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<CostCenterDto>.Success(MapToDto(costCenter));
    }

    public async Task<ServiceResult<CostCenterDto>> GetCostCenterByIdAsync(Guid id)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(id);
        if (costCenter == null)
            return ServiceResult<CostCenterDto>.Failure("Trung tâm chi phí không tồn tại.");

        return ServiceResult<CostCenterDto>.Success(MapToDto(costCenter));
    }

    public async Task<ServiceResult<List<CostCenterDto>>> GetAllCostCentersAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _costCenterRepository.GetAllPagedAsync(page, pageSize);
        var dtos = items.Select(MapToDto).ToList();
        return ServiceResult<List<CostCenterDto>>.Success(dtos);
    }

    public async Task<ServiceResult<List<CostCenterDto>>> GetCostCentersByDepartmentAsync(Guid departmentId)
    {
        var costCenters = await _costCenterRepository.GetByDepartmentAsync(departmentId);
        var dtos = costCenters.Select(MapToDto).ToList();
        return ServiceResult<List<CostCenterDto>>.Success(dtos);
    }

    public async Task<ServiceResult<CostAllocationDto>> CreateCostAllocationAsync(CreateCostAllocationDto dto)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(dto.CostCenterId);
        if (costCenter == null)
            return ServiceResult<CostAllocationDto>.Failure("Trung tâm chi phí không tồn tại.");

        var allocation = new CostAllocation
        {
            Id = Guid.NewGuid(),
            CostCenterId = dto.CostCenterId,
            Year = dto.Year,
            Month = dto.Month,
            TotalAmount = dto.TotalAmount,
            AllocatedAmount = 0,
            RemainingAmount = dto.TotalAmount,
            AllocationMethod = Enum.Parse<CostAllocationMethod>(dto.AllocationMethod),
            AllocationDate = DateTime.UtcNow,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        if (dto.Details.Any())
        {
            var details = dto.Details.Select(d => new CostAllocationDetail
            {
                Id = Guid.NewGuid(),
                CostAllocationId = allocation.Id,
                TargetCostCenterId = d.TargetCostCenterId,
                Amount = d.Amount,
                Percentage = d.Percentage,
                Description = d.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            }).ToList();

            allocation.AllocatedAmount = details.Sum(d => d.Amount);
            allocation.RemainingAmount = dto.TotalAmount - allocation.AllocatedAmount;

            await _costAllocationDetailRepository.AddRangeAsync(details);
        }

        await _costAllocationRepository.AddAsync(allocation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<CostAllocationDto>.Success(MapAllocationToDto(allocation, costCenter.CostCenterName));
    }

    public async Task<ServiceResult<CostAllocationDto>> GetCostAllocationByIdAsync(Guid id)
    {
        var allocation = await _costAllocationRepository.GetByIdAsync(id);
        if (allocation == null)
            return ServiceResult<CostAllocationDto>.Failure("Phân bổ chi phí không tồn tại.");

        var details = await _costAllocationDetailRepository.GetByAllocationIdAsync(id);
        var dto = MapAllocationToDto(allocation, allocation.CostCenter?.CostCenterName ?? "");
        dto.Details = details.Select(d => new CostAllocationDetailDto
        {
            TargetCostCenterId = d.TargetCostCenterId,
            TargetCostCenterName = d.TargetCostCenter?.CostCenterName ?? "",
            Amount = d.Amount,
            Percentage = d.Percentage,
            Description = d.Description
        }).ToList();

        return ServiceResult<CostAllocationDto>.Success(dto);
    }

    public async Task<ServiceResult<List<CostAllocationDto>>> GetCostAllocationsByPeriodAsync(int year, int month)
    {
        var allocations = await _costAllocationRepository.GetByPeriodAsync(year, month);
        var dtos = allocations.Select(a => MapAllocationToDto(a, a.CostCenter?.CostCenterName ?? "")).ToList();
        return ServiceResult<List<CostAllocationDto>>.Success(dtos);
    }

    public async Task<ServiceResult<CostAllocationDto>> ApproveCostAllocationAsync(Guid id)
    {
        var allocation = await _costAllocationRepository.GetByIdAsync(id);
        if (allocation == null)
            return ServiceResult<CostAllocationDto>.Failure("Phân bổ chi phí không tồn tại.");

        allocation.AllocatedAmount = allocation.TotalAmount - allocation.RemainingAmount;
        allocation.ModifiedAt = DateTime.UtcNow;

        await _costAllocationRepository.UpdateAsync(allocation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<CostAllocationDto>.Success(MapAllocationToDto(allocation, allocation.CostCenter?.CostCenterName ?? ""));
    }

    public async Task<ServiceResult<CostReportDto>> GetCostReportAsync(int year, int month, Guid? costCenterId = null)
    {
        return await GenerateCostReportAsync(year, month, costCenterId);
    }

    public async Task<ServiceResult<CostReportDto>> GetCostVarianceReportAsync(int year, int month, Guid? costCenterId = null)
    {
        var result = await GenerateCostReportAsync(year, month, costCenterId);
        if (!result.IsSuccess)
            return result;

        result.Data.ReportType = "Variance";
        return result;
    }

    public async Task<ServiceResult<CostReportDto>> GenerateCostReportAsync(int year, int month, Guid? costCenterId = null)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var costCenters = costCenterId.HasValue
            ? new[] { await _costCenterRepository.GetByIdAsync(costCenterId.Value) }.Where(c => c != null)
            : await _costCenterRepository.GetAllActiveAsync();

        var allocations = await _costAllocationRepository.GetByPeriodAsync(year, month);

        var result = new CostReportDto
        {
            Year = year,
            Month = month,
            CostCenterId = costCenterId,
            ReportType = "CostReport",
            Details = new List<CostReportDetailDto>()
        };

        foreach (var cc in costCenters.Where(c => c != null))
        {
            var c = cc!;
            var ccAllocations = allocations.Where(a => a.CostCenterId == c.Id).ToList();
            var directCost = ccAllocations.Sum(a => a.TotalAmount);
            var allocatedCost = 0m;

            result.Details.Add(new CostReportDetailDto
            {
                CostCenterId = c.Id,
                CostCenterCode = c.CostCenterCode,
                CostCenterName = c.CostCenterName,
                DirectCost = directCost,
                AllocatedCost = allocatedCost,
                TotalCost = directCost + allocatedCost,
                BudgetAmount = 0,
                Variance = 0
            });

            result.DirectCost += directCost;
            result.AllocatedCost += allocatedCost;
        }

        result.TotalCost = result.DirectCost + result.AllocatedCost;
        result.Variance = result.BudgetAmount - result.TotalCost;
        result.VariancePercentage = result.BudgetAmount > 0 ? (result.Variance / result.BudgetAmount) * 100 : 0;

        if (costCenterId.HasValue)
        {
            var cc = costCenters.FirstOrDefault();
            result.CostCenterName = cc?.CostCenterName;
        }

        return ServiceResult<CostReportDto>.Success(result);
    }

    private static CostCenterDto MapToDto(CostCenter entity)
    {
        return new CostCenterDto
        {
            Id = entity.Id,
            CostCenterCode = entity.CostCenterCode,
            CostCenterName = entity.CostCenterName,
            ParentId = entity.ParentId,
            CostCenterType = entity.CostCenterType,
            IsActive = entity.IsActive,
            Description = entity.Description,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.DepartmentName
        };
    }

    private static CostAllocationDto MapAllocationToDto(CostAllocation entity, string costCenterName)
    {
        return new CostAllocationDto
        {
            Id = entity.Id,
            CostCenterId = entity.CostCenterId,
            CostCenterName = costCenterName,
            Year = entity.Year,
            Month = entity.Month,
            TotalAmount = entity.TotalAmount,
            AllocatedAmount = entity.AllocatedAmount,
            RemainingAmount = entity.RemainingAmount,
            AllocationMethod = entity.AllocationMethod.ToString(),
            AllocationDate = entity.AllocationDate,
            Description = entity.Description,
            Status = entity.RemainingAmount == 0 ? "Completed" : "InProgress"
        };
    }
}