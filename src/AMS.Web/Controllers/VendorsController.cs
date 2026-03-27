using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

public class VendorsController : BaseController
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _vendorService.GetAllAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var vendor = await _vendorService.GetByIdAsync(id);
        if (vendor == null)
            return NotFound();

        return View(vendor);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateVendorDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _vendorService.CreateAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo nhà cung cấp.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var vendor = await _vendorService.GetByIdAsync(id);
        if (vendor == null)
            return NotFound();

        var updateDto = new UpdateVendorDto
        {
            Code = vendor.Code,
            Name = vendor.Name,
            TaxCode = vendor.TaxCode,
            Address = vendor.Address,
            Phone = vendor.Phone,
            Email = vendor.Email,
            ContactPerson = vendor.ContactPerson,
            BankAccount = vendor.BankAccount,
            BankName = vendor.BankName,
            CreditLimit = vendor.CreditLimit,
            PaymentTermDays = vendor.PaymentTermDays,
            IsVatPayer = vendor.IsVatPayer,
            InvoiceType = vendor.InvoiceType,
            IsActive = vendor.IsActive,
            Province = vendor.Province,
            District = vendor.District,
            Ward = vendor.Ward
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] UpdateVendorDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _vendorService.UpdateAsync(id, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật nhà cung cấp.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _vendorService.DeleteAsync(id);
        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var vendors = await _vendorService.GetAllActiveAsync();
        return Json(vendors);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var vendors = await _vendorService.GetAllActiveAsync();
        var filtered = vendors.Where(v =>
            v.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            v.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            v.TaxCode.Contains(term, StringComparison.OrdinalIgnoreCase));
        return Json(filtered);
    }
}
