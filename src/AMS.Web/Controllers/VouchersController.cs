using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Accounting.Vouchers.Interfaces;
using AMS.Application.Accounting.Vouchers.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller for managing accounting vouchers.
/// </summary>
public class VouchersController : BaseController
{
    private readonly IVoucherService _voucherService;

    /// <summary>
    /// Initializes a new instance of the VouchersController class.
    /// </summary>
    /// <param name="voucherService">The voucher service.</param>
    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>
    /// Displays the voucher list page.
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _voucherService.GetAllAsync(page, pageSize);
        return View(result);
    }

    /// <summary>
    /// Displays the voucher details page.
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        var voucher = await _voucherService.GetByIdAsync(id);
        if (voucher == null)
            return NotFound();

        return View(voucher);
    }

    /// <summary>
    /// Displays the create voucher form.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Creates a new voucher.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateVoucherDto voucher)
    {
        if (!ModelState.IsValid)
            return View(voucher);

        var result = await _voucherService.CreateAsync(voucher);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo chứng từ.");
            return View(voucher);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    /// <summary>
    /// Submits a voucher for approval.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _voucherService.SubmitAsync(id);
        return FromResult(result);
    }

    /// <summary>
    /// Approves a pending voucher.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var approverId = User.Identity?.Name ?? "system";
        var result = await _voucherService.ApproveAsync(id, approverId);
        return FromResult(result);
    }

    /// <summary>
    /// Rejects a pending voucher.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, [FromForm] string reason)
    {
        var result = await _voucherService.RejectAsync(id, reason);
        return FromResult(result);
    }

    /// <summary>
    /// Posts an approved voucher to the general ledger.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _voucherService.PostAsync(id);
        return FromResult(result);
    }

    /// <summary>
    /// Displays the edit voucher form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var voucher = await _voucherService.GetByIdAsync(id);
        if (voucher == null)
            return NotFound();

        return View(voucher);
    }

    /// <summary>
    /// Updates an existing voucher.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] VoucherDto voucherDto)
    {
        if (!ModelState.IsValid)
            return View(voucherDto);

        var result = await _voucherService.UpdateAsync(voucherDto.Id, voucherDto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật chứng từ.");
            return View(voucherDto);
        }

        return RedirectToAction(nameof(Details), new { id = voucherDto.Id });
    }

    /// <summary>
    /// Deletes a voucher.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _voucherService.DeleteAsync(id);
        if (!result.IsSuccess)
            return Failure(result.ErrorMessage ?? "Lỗi khi xóa chứng từ.");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Reverses a posted voucher.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reverse(Guid id)
    {
        var userId = User.Identity?.Name ?? "system";
        var result = await _voucherService.ReverseAsync(id, userId);
        return FromResult(result);
    }
}