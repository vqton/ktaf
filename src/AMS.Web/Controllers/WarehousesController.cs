using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller for managing warehouses.
/// </summary>
public class WarehousesController : BaseController
{
    private readonly IWarehouseService _warehouseService;

    /// <summary>
    /// Initializes a new instance of the WarehousesController class.
    /// </summary>
    /// <param name="warehouseService">The warehouse service.</param>
    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// Displays the warehouse list page.
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _warehouseService.GetAllPagedAsync(page, pageSize);
        return View(result);
    }

    /// <summary>
    /// Displays the warehouse details page.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound();

        return View(warehouse);
    }

    /// <summary>
    /// Displays the create warehouse form.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Handles the create warehouse form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWarehouseDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _warehouseService.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(dto);
    }

    /// <summary>
    /// Displays the edit warehouse form.
    /// </summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound();

        return View(warehouse);
    }

    /// <summary>
    /// Handles the edit warehouse form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateWarehouseDto dto)
    {
        if (id != dto.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(dto);

        var result = await _warehouseService.UpdateAsync(dto);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(dto);
    }

    /// <summary>
    /// Deletes a warehouse.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return RedirectToAction(nameof(Index));
    }
}