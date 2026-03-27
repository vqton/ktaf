using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for creating a Bank.
/// </summary>
public class CreateBankDto
{
    /// <summary>
    /// Gets or sets the bank code.
    /// </summary>
    [Required(ErrorMessage = "Bank code is required.")]
    [StringLength(20, ErrorMessage = "Bank code cannot exceed 20 characters.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bank name.
    /// </summary>
    [Required(ErrorMessage = "Bank name is required.")]
    [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bank SWIFT code.
    /// </summary>
    [StringLength(11, ErrorMessage = "SWIFT code cannot exceed 11 characters.")]
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Gets or sets the bank logo path.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Gets or sets the bank branch name.
    /// </summary>
    [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters.")]
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the bank address.
    /// </summary>
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bank is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}