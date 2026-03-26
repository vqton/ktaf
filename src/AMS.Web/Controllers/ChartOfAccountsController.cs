using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

public class ChartOfAccountsController : BaseController
{
    private readonly IChartOfAccountsService _chartOfAccountsService;

    public ChartOfAccountsController(IChartOfAccountsService chartOfAccountsService)
    {
        _chartOfAccountsService = chartOfAccountsService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _chartOfAccountsService.GetAllAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var account = await _chartOfAccountsService.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        return View(account);
    }

    public async Task<IActionResult> Hierarchy()
    {
        var accounts = await _chartOfAccountsService.GetHierarchyAsync();
        return View(accounts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateChartOfAccountsDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _chartOfAccountsService.CreateAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo tài khoản.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var account = await _chartOfAccountsService.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        var updateDto = new UpdateChartOfAccountsDto
        {
            Code = account.Code,
            Name = account.Name,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            ParentId = account.ParentId,
            IsDetail = account.IsDetail,
            IsActive = account.IsActive,
            AllowEntry = account.AllowEntry,
            TaxCategory = account.TaxCategory,
            BankAccount = account.BankAccount,
            BankName = account.BankName,
            OpeningBalance = account.OpeningBalance,
            CurrencyCode = account.CurrencyCode,
            IsBankAccount = account.IsBankAccount,
            IsTaxAccount = account.IsTaxAccount,
            IsVatAccount = account.IsVatAccount,
            VatRateCode = account.VatRateCode,
            IsRevenueSharing = account.IsRevenueSharing,
            RevenueSharingPercentage = account.RevenueSharingPercentage,
            ReconciliationAccount = account.ReconciliationAccount
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] UpdateChartOfAccountsDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _chartOfAccountsService.UpdateAsync(id, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật tài khoản.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _chartOfAccountsService.DeleteAsync(id);
        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetChildren(Guid? parentId)
    {
        var children = await _chartOfAccountsService.GetChildrenAsync(parentId);
        return Json(children);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var accounts = await _chartOfAccountsService.GetAllActiveAsync();
        return Json(accounts);
    }
}
