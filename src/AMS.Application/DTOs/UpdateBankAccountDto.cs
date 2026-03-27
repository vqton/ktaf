using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for updating a BankAccount.
/// </summary>
public class UpdateBankAccountDto : CreateBankAccountDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    public Guid Id { get; set; }
}