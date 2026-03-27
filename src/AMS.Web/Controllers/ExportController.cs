using Microsoft.AspNetCore.Mvc;
using AMS.Web.Services;
using AMS.Application.Common.Results;

namespace AMS.Web.Controllers;

/// <summary>
/// API controller for exporting data to various formats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    /// <summary>
    /// Initializes a new instance of the ExportController class.
    /// </summary>
    /// <param name="exportService">The export service.</param>
    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Exports data to the specified format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The exported file.</returns>
    [HttpPost]
    public async Task<IActionResult> Export([FromBody] ExportRequest request)
    {
        if (request.Columns == null || !request.Columns.Any())
            return BadRequest(new { success = false, message = "Columns are required." });

        var result = await _exportService.ExportAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Exports data to Excel format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The exported Excel file.</returns>
    [HttpPost("excel")]
    public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
    {
        request.Format = ExportFormat.Excel;
        var result = await _exportService.ExportToExcelAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Exports data to PDF format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The exported PDF file.</returns>
    [HttpPost("pdf")]
    public async Task<IActionResult> ExportToPdf([FromBody] ExportRequest request)
    {
        request.Format = ExportFormat.Pdf;
        var result = await _exportService.ExportToPdfAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Exports data to CSV format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The exported CSV file.</returns>
    [HttpPost("csv")]
    public async Task<IActionResult> ExportToCsv([FromBody] ExportRequest request)
    {
        request.Format = ExportFormat.Csv;
        var result = await _exportService.ExportToCsvAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
