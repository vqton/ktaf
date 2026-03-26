using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Constants;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller xử lý các request liên quan đến tài sản cố định.
/// </summary>
public class FixedAssetsController : BaseController
{
    private readonly IAssetService _assetService;

    public FixedAssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    /// <summary>
    /// Hiển thị trang danh sách tài sản.
    /// </summary>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Số phần tử mỗi trang.</param>
    /// <returns>View danh sách tài sản.</returns>
    public async Task<IActionResult> Index(int page = 1, int pageSize = AppConstants.DefaultPageSize)
    {
        var result = await _assetService.GetAllAssetsAsync(page, pageSize);
        return View(result);
    }

    /// <summary>
    /// Hiển thị trang chi tiết tài sản.
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <returns>View chi tiết tài sản.</returns>
    public async Task<IActionResult> Details(Guid id)
    {
        var asset = await _assetService.GetAssetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }

    /// <summary>
    /// Lấy tài sản theo mã (API).
    /// </param>
    /// <param name="code">Mã tài sản.</param>
    /// <returns>Tài sản tìm thấy hoặc 404.</returns>
    [HttpGet]
    public async Task<IActionResult> GetByCode(string code)
    {
        var asset = await _assetService.GetAssetByCodeAsync(code);
        if (asset == null)
            return NotFound();

        return Ok(asset);
    }

    /// <summary>
    /// Lấy danh sách tài sản đang hoạt động (API).
    /// </summary>
    /// <returns>Danh sách tài sản đang hoạt động.</returns>
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var assets = await _assetService.GetActiveAssetsAsync();
        return Ok(assets);
    }

    /// <summary>
    /// Hiển thị trang tạo mới tài sản.
    /// </summary>
    /// <returns>View tạo mới tài sản.</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Tạo mới tài sản.
    /// </summary>
    /// <param name="dto">Thông tin tài sản cần tạo.</param>
    /// <returns>Chuyển đến trang chi tiết nếu thành công.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateFixedAssetDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _assetService.CreateAssetAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi tạo tài sản.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    /// <summary>
    /// Hiển thị trang chỉnh sửa tài sản.
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <returns>View chỉnh sửa tài sản.</returns>
    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await _assetService.GetAssetByIdAsync(id);
        if (asset == null)
            return NotFound();

        var dto = new CreateFixedAssetDto
        {
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            AssetGroupId = asset.AssetGroupId,
            SerialNumber = asset.SerialNumber,
            Model = asset.Model,
            AccountId = asset.AccountId,
            OriginalCost = asset.OriginalCost,
            ResidualValue = asset.ResidualValue,
            UsefulLifeMonths = asset.UsefulLifeMonths,
            AcquisitionDate = asset.AcquisitionDate,
            DepreciationStartDate = asset.DepreciationStartDate,
            DepreciationMethod = asset.DepreciationMethod,
            DepartmentCode = asset.DepartmentCode,
            Description = asset.Description
        };

        return View(dto);
    }

    /// <summary>
    /// Cập nhật tài sản.
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <param name="dto">Thông tin tài sản cần cập nhật.</param>
    /// <returns>Chuyển đến trang chi tiết nếu thành công.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] CreateFixedAssetDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _assetService.UpdateAssetAsync(id, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi khi cập nhật tài sản.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Tính khấu hao cho tài sản.
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <returns>Kết quả tính khấu hao.</returns>
    [HttpPost]
    public async Task<IActionResult> CalculateDepreciation(Guid id)
    {
        var result = await _assetService.CalculateDepreciationAsync(id);
        return FromResult(result);
    }

    /// <summary>
    /// Ghi sổ khấu hao tài sản.
    /// </summary>
    /// <param name="dto">Thông tin khấu hao cần ghi sổ.</param>
    /// <returns>Kết quả ghi sổ.</returns>
    [HttpPost]
    public async Task<IActionResult> PostDepreciation([FromForm] DepreciationPostDto dto)
    {
        var result = await _assetService.PostDepreciationAsync(dto.FixedAssetId, dto.Year, dto.Month, dto.VoucherNo);
        return FromResult(result);
    }

    /// <summary>
    /// Lấy lịch trình khấu hao của tài sản (API).
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <returns>Danh sách lịch trình khấu hao.</returns>
    [HttpGet]
    public async Task<IActionResult> GetDepreciationSchedule(Guid id)
    {
        var schedule = await _assetService.GenerateDepreciationScheduleAsync(id);
        return Ok(schedule);
    }

    /// <summary>
    /// Lấy thông tin khấu hao hàng tháng của tài sản (API).
    /// </summary>
    /// <param name="id">ID tài sản.</param>
    /// <returns>Thông tin khấu hao hàng tháng.</returns>
    [HttpGet]
    public async Task<IActionResult> GetMonthlyDepreciation(Guid id)
    {
        var result = await _assetService.CalculateMonthlyDepreciationAsync(id);
        return Ok(result);
    }
}