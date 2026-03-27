using System.Data;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AMS.Web.Services;

/// <summary>
/// Service for exporting data to various formats (Excel, PDF, CSV).
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports data to Excel format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result with file content.</returns>
    Task<ExportResult> ExportToExcelAsync(ExportRequest request);

    /// <summary>
    /// Exports data to PDF format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result with file content.</returns>
    Task<ExportResult> ExportToPdfAsync(ExportRequest request);

    /// <summary>
    /// Exports data to CSV format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result with file content.</returns>
    Task<ExportResult> ExportToCsvAsync(ExportRequest request);

    /// <summary>
    /// Exports data based on the specified format.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result with file content.</returns>
    Task<ExportResult> ExportAsync(ExportRequest request);
}

/// <summary>
/// Implementation of the export service.
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    /// <summary>
    /// Initializes a new instance of the ExportService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportToExcelAsync(ExportRequest request)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Export");

        // Add title
        if (!string.IsNullOrEmpty(request.Title))
        {
            var titleCell = worksheet.Cell(1, 1);
            titleCell.Value = request.Title;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            worksheet.Range(1, 1, 1, request.Columns.Count).Merge();
        }

        var startRow = string.IsNullOrEmpty(request.Title) ? 1 : 3;

        // Add headers
        for (int i = 0; i < request.Columns.Count; i++)
        {
            var cell = worksheet.Cell(startRow, i + 1);
            cell.Value = request.Columns[i].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Column(i + 1).Width = request.Columns[i].Width;
        }

        // Add data
        var dataRows = request.Data.ToList();
        for (int row = 0; row < dataRows.Count; row++)
        {
            for (int col = 0; col < request.Columns.Count; col++)
            {
                var column = request.Columns[col];
                var cell = worksheet.Cell(startRow + row + 1, col + 1);
                var value = dataRows[row].GetValueOrDefault(column.PropertyName);

                if (value == null)
                {
                    cell.Value = Blank.Value;
                    continue;
                }

                if (column.IsDate && value is DateTime dateValue)
                {
                    cell.Value = dateValue;
                    cell.Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else if (column.IsCurrency && decimal.TryParse(value.ToString(), out decimal currencyValue))
                {
                    cell.Value = currencyValue;
                    cell.Style.NumberFormat.Format = "#,##0.00";
                }
                else
                {
                    cell.Value = value.ToString();
                }

                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = SanitizeFileName($"{request.Title}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

        return new ExportResult
        {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = fileName
        };
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportToPdfAsync(ExportRequest request)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text(request.Title)
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken4);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var column in request.Columns)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var column in request.Columns)
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(5)
                                    .Text(column.Header)
                                    .Bold();
                            }
                        });

                        // Data rows
                        var dataRows = request.Data.ToList();
                        foreach (var row in dataRows)
                        {
                            foreach (var column in request.Columns)
                            {
                                var value = row.GetValueOrDefault(column.PropertyName);
                                var displayValue = FormatValue(value, column);

                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text(displayValue);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);

        var fileName = SanitizeFileName($"{request.Title}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        return new ExportResult
        {
            Content = stream.ToArray(),
            ContentType = "application/pdf",
            FileName = fileName
        };
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportToCsvAsync(ExportRequest request)
    {
        var lines = new List<string>();

        // Header
        var header = string.Join(",", request.Columns.Select(c => EscapeCsv(c.Header)));
        lines.Add(header);

        // Data
        foreach (var row in request.Data)
        {
            var values = request.Columns.Select(column =>
            {
                var value = row.GetValueOrDefault(column.PropertyName);
                return EscapeCsv(FormatValue(value, column));
            });
            lines.Add(string.Join(",", values));
        }

        var content = string.Join(Environment.NewLine, lines);
        var bytes = Encoding.UTF8.GetBytes(content);

        var fileName = SanitizeFileName($"{request.Title}_{DateTime.Now:yyyyMMddHHmmss}.csv");

        return new ExportResult
        {
            Content = bytes,
            ContentType = "text/csv",
            FileName = fileName
        };
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportAsync(ExportRequest request)
    {
        return request.Format switch
        {
            ExportFormat.Excel => await ExportToExcelAsync(request),
            ExportFormat.Pdf => await ExportToPdfAsync(request),
            ExportFormat.Csv => await ExportToCsvAsync(request),
            _ => throw new ArgumentException($"Unsupported export format: {request.Format}")
        };
    }

    private static string FormatValue(object? value, ExportColumn column)
    {
        if (value == null) return string.Empty;

        if (column.IsDate && value is DateTime dateValue)
        {
            return dateValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        if (column.IsCurrency && decimal.TryParse(value.ToString(), out decimal currencyValue))
        {
            return currencyValue.ToString("N2", CultureInfo.InvariantCulture);
        }

        return value.ToString() ?? string.Empty;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
