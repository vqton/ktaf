using AMS.Domain.Entities;

namespace AMS.Domain.Entities.HR;

/// <summary>
/// Represents a department in the organization structure.
/// </summary>
public class Department : BaseAuditEntity
{
    /// <summary>
    /// Department code.
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Department name.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Parent department ID for hierarchy.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Manager employee ID.
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Department cost center code.
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Indicates if the department is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to parent department.
    /// </summary>
    public Department? Parent { get; set; }

    /// <summary>
    /// Navigation property to child departments.
    /// </summary>
    public ICollection<Department> Children { get; set; } = new List<Department>();
}
