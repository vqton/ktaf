using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities.Assets;

/// <summary>
/// Represents a fixed asset (Tài sản cố định) in the accounting system.
/// Tracks asset information, depreciation, and book value.
/// </summary>
public class FixedAsset : BaseAuditEntity
{
    /// <summary>
    /// Unique asset code.
    /// </summary>
    public string AssetCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the fixed asset.
    /// </summary>
    public string AssetName { get; set; } = string.Empty;

    /// <summary>
    /// Asset group/category ID.
    /// </summary>
    public Guid? AssetGroupId { get; set; }

    /// <summary>
    /// Serial number (for tracking physical asset).
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Model/version of the asset.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Foreign key to the asset account in chart of accounts.
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Original acquisition cost (Nguyên giá).
    /// </summary>
    public decimal OriginalCost { get; set; }

    /// <summary>
    /// Residual value at end of useful life (Giá trị thanh lý).
    /// </summary>
    public decimal? ResidualValue { get; set; }

    /// <summary>
    /// Useful life in months (Thời gian sử dụng hữu ích).
    /// </summary>
    public int UsefulLifeMonths { get; set; }

    /// <summary>
    /// Date of asset acquisition.
    /// </summary>
    public DateTime AcquisitionDate { get; set; }

    /// <summary>
    /// Date when depreciation starts.
    /// </summary>
    public DateTime? DepreciationStartDate { get; set; }

    /// <summary>
    /// Current status of the asset.
    /// </summary>
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    /// <summary>
    /// Depreciation method (STRAIGHT_LINE, DECLINING_BALANCE, UNITS_OF_PRODUCTION).
    /// </summary>
    public string? DepreciationMethod { get; set; } = "STRAIGHT_LINE";

    /// <summary>
    /// Accumulated depreciation to date (Hao mòn lũy kế).
    /// </summary>
    public decimal? AccumulatedDepreciation { get; set; }

    /// <summary>
    /// Current book value (Giá trị còn lại).
    /// </summary>
    public decimal? BookValue { get; set; }

    /// <summary>
    /// Department code using the asset.
    /// </summary>
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// Additional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to depreciation schedules.
    /// </summary>
    public ICollection<DepreciationSchedule> DepreciationSchedules { get; set; } = new List<DepreciationSchedule>();

    /// <summary>
    /// Navigation property to asset group.
    /// </summary>
    public AssetGroup? AssetGroup { get; set; }
}
