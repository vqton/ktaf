using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for creating a Warehouse.
/// </summary>
public class CreateWarehouseDto
{
    /// <summary>
    /// Gets or sets the warehouse code.
    /// </summary>
    [Required(ErrorMessage = "Warehouse code is required.")]
    [StringLength(20, ErrorMessage = "Warehouse code cannot exceed 20 characters.")]
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warehouse name.
    /// </summary>
    [Required(ErrorMessage = "Warehouse name is required.")]
    [StringLength(100, ErrorMessage = "Warehouse name cannot exceed 100 characters.")]
    public string WarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warehouse address.
    /// </summary>
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the warehouse manager.
    /// </summary>
    [StringLength(100, ErrorMessage = "Manager name cannot exceed 100 characters.")]
    public string? Manager { get; set; }

    /// </summary>
    /// Gets or sets the inventory costing method (FIFO or AVCO).
    /// </summary>
    [Required(ErrorMessage = "Pricing method is required.")]
    [StringLength(10, ErrorMessage = "Pricing method cannot exceed 10 characters.")]
    public string PricingMethod { get; set; } = "AVCO";

    /// <summary>
    /// Gets or sets a value indicating whether the warehouse is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}