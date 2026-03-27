using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for updating a Warehouse.
/// </summary>
public class UpdateWarehouseDto : CreateWarehouseDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    public Guid Id { get; set; }
}