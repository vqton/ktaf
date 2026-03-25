using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Inventory;

/// <summary>
/// Represents a product group/category for classification.
/// </summary>
public class ProductGroup : BaseLookupEntity
{
    /// <summary>
    /// Parent group ID for hierarchical structure.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property to parent group.
    /// </summary>
    public ProductGroup? Parent { get; set; }

    /// <summary>
    /// Navigation property to child groups.
    /// </summary>
    public ICollection<ProductGroup> Children { get; set; } = new List<ProductGroup>();
}
