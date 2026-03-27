using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller for managing bank accounts.
/// </summary>
public class BankAccountsController : BaseController
{
    private readonly IBankAccountService _bankAccountService;
    private readonly IBankService _bankService;

    /// <summary>
    /// Initializes a new instance of the BankAccountsController class.
    /// </summary>
    /// <param name="bankAccountService">The bank account service.</param>
    /// <param name="bankService">The bank service.</param>
    public BankAccountsController(IBankAccountService bankAccountService, IBankService bankService)
    {
        _bankAccountService = bankAccountService;
        _bankService = bankService;
    }

    /// <summary>
    /// Displays the bank account list page.
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _bankAccountService.GetAllPagedAsync(page, pageSize);
        return View(result);
    }

    /// <summary>
    /// Displays the bank account details page.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var account = await _bankAccountService.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        return View(account);
    }

    /// <summary>
    /// Displays the create bank account form.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        var bankList = await _bankService.GetAllActiveAsync();
        ViewBag.Banks = bankList;
        return View();
    }

    /// <summary>
    /// Handles the create bank account form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBankAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            var availableBanks = await _bankService.GetAllActiveAsync();
            ViewBag.Banks = availableBanks;
            return View(dto);
        }

        var result = await _bankAccountService.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        var availableBanksList = await _bankService.GetAllActiveAsync();
        ViewBag.Banks = availableBanksList;
        return View(dto);
    }

    /// <summary>
    /// Displays the edit bank account form.
    /// </summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        var account = await _bankAccountService.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        var editBankList = await _bankService.GetAllActiveAsync();
        ViewBag.Banks = editBankList;
        return View(account);
    }

    /// <summary>
    /// Handles the edit bank account form submission.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateBankAccountDto dto)
    {
        if (id != dto.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var editAvailableBanks = await _bankService.GetAllActiveAsync();
            ViewBag.Banks = editAvailableBanks;
            return View(dto);
        }

        var result = await _bankAccountService.UpdateAsync(dto);
        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        var editBanksList = await _bankService.GetAllActiveAsync();
        ViewBag.Banks = editBanksList;
        return View(dto);
    }

    /// <summary>
    /// Deletes a bank account.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bankAccountService.DeleteAsync(id);
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

    /// <summary>
    /// Sets a bank account as primary.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimary(Guid id)
    {
        var result = await _bankAccountService.SetPrimaryAsync(id);
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
}