using AMS.Domain.Entities.DM;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for Bank entity.
/// </summary>
public class BankDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the bank code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bank name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bank SWIFT code.
    /// </summary>
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Gets or sets the bank logo path.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Gets or sets the bank branch name.
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the bank address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bank is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time the entity was created.
    /// </>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time the entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }
}