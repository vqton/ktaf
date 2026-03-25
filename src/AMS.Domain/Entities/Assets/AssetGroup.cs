using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Assets;

/// <summary>
/// Represents an asset group/category for fixed asset classification.
/// </summary>
public class AssetGroup : BaseLookupEntity
{
    /// <summary>
    /// Depreciation method for this group (STRAIGHT_LINE, DECLINING_BALANCE, UNITS_OF_PRODUCTION).
    /// </summary>
    public string? DepreciationMethod { get; set; }

    /// <summary>
    /// Default useful life in months for assets in this group.
    /// </summary>
    public int? DefaultUsefulLifeMonths { get; set; }

    /// <summary>
    /// Default depreciation rate for declining balance method.
    /// </summary>
    public decimal? DefaultDepreciationRate { get; set; }

    /// <summary>
    /// Parent group ID for hierarchical structure.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Navigation property to parent group.
    /// </summary>
    public AssetGroup? Parent { get; set; }

    /// <summary>
    /// Navigation property to child groups.
    /// </summary>
    public ICollection<AssetGroup> Children { get; set; } = new List<AssetGroup>();
}
