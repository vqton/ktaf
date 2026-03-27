using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Accounting.Vouchers.Interfaces;

namespace AMS.Web.Controllers;

/// <summary>
/// API controller providing DataTables-compatible endpoints for accounting data.
/// </summary>
[ApiController]
[Route("api/datatables")]
public class DataTablesController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly ICustomerService _customerService;
    private readonly IVendorService _vendorService;

    /// <summary>
    /// Initializes a new instance of the DataTablesController class.
    /// </summary>
    /// <param name="voucherService">The voucher service.</param>
    /// <param name="chartOfAccountsService">The chart of accounts service.</param>
    /// <param name="customerService">The customer service.</param>
    /// <param name="vendorService">The vendor service.</param>
    public DataTablesController(
        IVoucherService voucherService,
        IChartOfAccountsService chartOfAccountsService,
        ICustomerService customerService,
        IVendorService vendorService)
    {
        _voucherService = voucherService;
        _chartOfAccountsService = chartOfAccountsService;
        _customerService = customerService;
        _vendorService = vendorService;
    }

    /// <summary>
    /// Gets vouchers for DataTables server-side processing.
    /// </summary>
    /// <param name="request">The DataTables request.</param>
    /// <returns>DataTable-compatible response.</returns>
    [HttpPost("vouchers")]
    public async Task<IActionResult> GetVouchers([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _voucherService.GetAllAsync(request.Start / request.Length + 1, request.Length);

            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = Enumerable.Empty<object>(),
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets chart of accounts for DataTables server-side processing.
    /// </summary>
    /// <param name="request">The DataTables request.</param>
    /// <returns>DataTable-compatible response.</returns>
    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> GetAccounts([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _chartOfAccountsService.GetAllAsync(request.Start / request.Length + 1, request.Length);

            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = Enumerable.Empty<object>(),
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets customers for DataTables server-side processing.
    /// </summary>
    /// <param name="request">The DataTables request.</param>
    /// <returns>DataTable-compatible response.</returns>
    [HttpPost("customers")]
    public async Task<IActionResult> GetCustomers([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _customerService.GetAllAsync(request.Start / request.Length + 1, request.Length);

            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = Enumerable.Empty<object>(),
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets vendors for DataTables server-side processing.
    /// </summary>
    /// <param name="request">The DataTables request.</param>
    /// <returns>DataTable-compatible response.</returns>
    [HttpPost("vendors")]
    public async Task<IActionResult> GetVendors([FromBody] DataTablesRequest request)
    {
        try
        {
            var result = await _vendorService.GetAllAsync(request.Start / request.Length + 1, request.Length);

            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            return Ok(new DataTablesResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = Enumerable.Empty<object>(),
                Error = ex.Message
            });
        }
    }
}
