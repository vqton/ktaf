using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a Personal Income Tax allowance/deduction for an employee.
/// Includes personal deduction (giảm trừ gia cảnh) and dependent deductions.
/// </summary>
public class PITAllowance : BaseEntity
{
    /// <summary>
    /// Foreign key to the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Type of allowance (e.g., "PERSONAL", "DEPENDENT", "HOUSING").
    /// </summary>
    public string AllowanceType { get; set; } = string.Empty;

    /// <summary>
    /// Allowance amount in VND.
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Date from which this allowance is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Date until which this allowance is effective (null = currently active).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Legal basis for this allowance.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Indicates if the allowance is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
