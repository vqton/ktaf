using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for updating a Bank.
/// </summary>
public class UpdateBankDto : CreateBankDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    public Guid Id { get; set; }
}