using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Accounting.Vouchers.Interfaces;
using AMS.Application.Accounting.Vouchers.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

public class VouchersController : BaseController
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _voucherService.GetAllAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var voucher = await _voucherService.GetByIdAsync(id);
        if (voucher == null)
            return NotFound();

        return View(voucher);
    }

    public IActionResult Create()
    {
        return View();
    }

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

    [HttpPost]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _voucherService.SubmitAsync(id);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var approverId = User.Identity?.Name ?? "system";
        var result = await _voucherService.ApproveAsync(id, approverId);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, [FromForm] string reason)
    {
        var result = await _voucherService.RejectAsync(id, reason);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _voucherService.PostAsync(id);
        return FromResult(result);
    }
}