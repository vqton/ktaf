using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Assets;

/// <summary>
/// Represents a monthly depreciation schedule entry for a fixed asset.
/// </summary>
public class DepreciationSchedule : BaseEntity
{
    /// <summary>
    /// Foreign key to the fixed asset.
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Year of the depreciation period.
    /// </summary>
    public int PeriodYear { get; set; }

    /// <summary>
    /// Month of the depreciation period.
    /// </summary>
    public int PeriodMonth { get; set; }

    /// <summary>
    /// Depreciation amount for this period.
    /// </summary>
    public decimal DepreciationAmount { get; set; }

    /// <summary>
    /// Accumulated depreciation up to this period.
    /// </summary>
    public decimal AccumulatedAmount { get; set; }

    /// <summary>
    /// Book value at the beginning of the period.
    /// </summary>
    public decimal BookValueBeginning { get; set; }

    /// <summary>
    /// Book value at the end of the period.
    /// </summary>
    public decimal BookValueEnding { get; set; }

    /// <summary>
    /// Indicates if the depreciation has been posted to the ledger.
    /// </summary>
    public bool IsPosted { get; set; }

    /// <summary>
    /// Foreign key to the voucher (if posted).
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Username of the creator.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Navigation property to the fixed asset.
    /// </summary>
    public FixedAsset? Asset { get; set; }
}
