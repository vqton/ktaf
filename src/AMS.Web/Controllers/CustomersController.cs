using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _customerService.GetAllAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        return View(customer);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _customerService.CreateAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo khách hàng.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        var updateDto = new UpdateCustomerDto
        {
            Code = customer.Code,
            Name = customer.Name,
            TaxCode = customer.TaxCode,
            Address = customer.Address,
            Phone = customer.Phone,
            Email = customer.Email,
            ContactPerson = customer.ContactPerson,
            BankAccount = customer.BankAccount,
            BankName = customer.BankName,
            CreditLimit = customer.CreditLimit,
            PaymentTermDays = customer.PaymentTermDays,
            IsVatPayer = customer.IsVatPayer,
            InvoiceType = customer.InvoiceType,
            IsActive = customer.IsActive,
            Province = customer.Province,
            District = customer.District,
            Ward = customer.Ward
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _customerService.UpdateAsync(id, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật khách hàng.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var customers = await _customerService.GetAllActiveAsync();
        return Json(customers);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var customers = await _customerService.GetAllActiveAsync();
        var filtered = customers.Where(c => 
            c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            c.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            c.TaxCode.Contains(term, StringComparison.OrdinalIgnoreCase));
        return Json(filtered);
    }
}
