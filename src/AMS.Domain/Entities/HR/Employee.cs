using AMS.Domain.Entities;
using AMS.Domain.Entities.Tax;

namespace AMS.Domain.Entities.HR;

/// <summary>
/// Represents an employee in the HR system.
/// Used for payroll processing and personal income tax calculations.
/// </summary>
public class Employee : BaseAuditEntity
{
    /// <summary>
    /// Unique employee code.
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the employee.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Identity card number (CMND/CCCD).
    /// </summary>
    public string? IdentityCardNo { get; set; }

    /// <summary>
    /// Date the identity card was issued.
    /// </summary>
    public DateTime? IdentityCardDate { get; set; }

    /// <summary>
    /// Place where the identity card was issued.
    /// </summary>
    public string? IdentityCardPlace { get; set; }

    /// <summary>
    /// Personal tax code (Mã số thuế cá nhân).
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Social insurance number (Mã số BHXH).
    /// </summary>
    public string? SocialInsuranceNo { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Home address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Department ID.
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Job position/title.
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Date the employee joined the company.
    /// </summary>
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// Date the employee left the company (if applicable).
    /// </summary>
    public DateTime? LeaveDate { get; set; }

    /// <summary>
    /// Active Directory username for Windows authentication.
    /// </summary>
    public string? ADUsername { get; set; }

    /// <summary>
    /// Indicates if the employee is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Salary bank account number.
    /// </summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>
    /// Salary bank name.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Navigation property to PIT allowances.
    /// </summary>
    public ICollection<PITAllowance> PITAllowances { get; set; } = new List<PITAllowance>();
}
