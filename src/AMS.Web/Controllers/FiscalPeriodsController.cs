using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller for managing fiscal periods.
/// </summary>
public class FiscalPeriodsController : BaseController
{
    private readonly IFiscalPeriodService _fiscalPeriodService;

    /// <summary>
    /// Initializes a new instance of the FiscalPeriodsController class.
    /// </summary>
    /// <param name="fiscalPeriodService">The fiscal period service.</param>
    public FiscalPeriodsController(IFiscalPeriodService fiscalPeriodService)
    {
        _fiscalPeriodService = fiscalPeriodService;
    }

    /// <summary>
    /// Displays the fiscal period list page.
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _fiscalPeriodService.GetAllAsync(page, pageSize);
        return View(result);
    }

    /// <summary>
    /// Displays the fiscal period details page.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var period = await _fiscalPeriodService.GetByIdAsync(id);
        if (period == null)
            return NotFound();

        return View(period);
    }

    /// <summary>
    /// Displays the create fiscal period form.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Handles the create fiscal period form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFiscalPeriodDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _fiscalPeriodService.CreateAsync(dto);
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
    /// Displays the edit fiscal period form.
    /// </summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        var period = await _fiscalPeriodService.GetByIdAsync(id);
        if (period == null)
            return NotFound();

        return View(period);
    }

    /// <summary>
    /// Handles the edit fiscal period form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FiscalPeriodDto dto)
    {
        if (id != dto.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(dto);

        // Note: In a real implementation, we would have an UpdateAsync method
        // For now, we'll redirect back to index as edit functionality needs to be implemented in service
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Opens a fiscal period.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(Guid id)
    {
        var result = await _fiscalPeriodService.OpenPeriodAsync(id, CancellationToken.None);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Closes a fiscal period.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id)
    {
        var result = await _fiscalPeriodService.ClosePeriodAsync(id, User.Identity?.Name ?? "system", CancellationToken.None);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Locks a fiscal period.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(Guid id)
    {
        var result = await _fiscalPeriodService.LockPeriodAsync(id, CancellationToken.None);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Generates fiscal periods for a year.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateYear(int year)
    {
        var result = await _fiscalPeriodService.GenerateYearPeriodsAsync(year);
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