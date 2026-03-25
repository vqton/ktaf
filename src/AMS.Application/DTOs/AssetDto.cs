using AMS.Domain.Enums;

namespace AMS.Application.DTOs;

public class FixedAssetDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public Guid? AssetGroupId { get; set; }
    public string? AssetGroupName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public Guid? AccountId { get; set; }
    public decimal OriginalCost { get; set; }
    public decimal? ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public AssetStatus Status { get; set; }
    public string? DepreciationMethod { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }
    public decimal? BookValue { get; set; }
    public string? DepartmentCode { get; set; }
    public string? Description { get; set; }
}

public class CreateFixedAssetDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public Guid? AssetGroupId { get; set; }
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public Guid? AccountId { get; set; }
    public decimal OriginalCost { get; set; }
    public decimal? ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? DepreciationMethod { get; set; }
    public string? DepartmentCode { get; set; }
    public string? Description { get; set; }
}

public class DepreciationScheduleDto
{
    public Guid Id { get; set; }
    public Guid FixedAssetId { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
}

public class DepreciationResultDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public decimal OriginalCost { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal DepreciableAmount { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal MonthlyDepreciation { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal CurrentBookValue { get; set; }
    public int RemainingMonths { get; set; }
}

public class DepreciationPostDto
{
    public Guid FixedAssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
}
