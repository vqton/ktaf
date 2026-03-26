using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

public class ProductsController : BaseController
{
    private readonly IInventoryService _inventoryService;

    public ProductsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _inventoryService.GetAllProductsAsync(page, pageSize);
        return View(result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _inventoryService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _inventoryService.CreateProductAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo sản phẩm.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _inventoryService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        var updateDto = new CreateProductDto
        {
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            ProductNameEn = product.ProductNameEn,
            ProductGroupId = product.ProductGroupId,
            Type = product.Type,
            UnitOfMeasure = product.UnitOfMeasure,
            UnitPrice = product.UnitPrice,
            CurrencyCode = product.CurrencyCode,
            VatRate = product.VatRate,
            IsExciseTaxItem = product.IsExciseTaxItem,
            TaxCode = product.TaxCode,
            WarehouseId = product.WarehouseId,
            Specification = product.Specification,
            Origin = product.Origin
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _inventoryService.UpdateProductAsync(id, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật sản phẩm.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var products = await _inventoryService.GetActiveProductsAsync();
        return Json(products);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var products = await _inventoryService.GetActiveProductsAsync();
        var filtered = products.Where(p => 
            p.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            p.ProductCode.Contains(term, StringComparison.OrdinalIgnoreCase));
        return Json(filtered);
    }

    public async Task<IActionResult> Balances(Guid? warehouseId = null)
    {
        var balances = await _inventoryService.GetInventoryBalancesAsync(warehouseId);
        return View(balances);
    }

    public async Task<IActionResult> Transactions(Guid productId, int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _inventoryService.GetTransactionsByProductAsync(productId, page, pageSize);
        return View(result);
    }
}
