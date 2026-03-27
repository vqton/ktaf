namespace AMS.Web.Services;

/// <summary>
/// Export format types.
/// </summary>
public enum ExportFormat
{
    Excel,
    Pdf,
    Csv
}

/// <summary>
/// Result of an export operation.
/// </summary>
public class ExportResult
{
    /// <summary>
    /// Gets or sets the file content as bytes.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name with extension.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Column definition for export.
/// </summary>
public class ExportColumn
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display header.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the column width (for Excel).
    /// </summary>
    public int Width { get; set; } = 15;

    /// <summary>
    /// Gets or sets a value indicating whether to format as currency.
    /// </summary>
    public bool IsCurrency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to format as date.
    /// </summary>
    public bool IsDate { get; set; }
}

/// <summary>
/// Export request model.
/// </summary>
public class ExportRequest
{
    /// <summary>
    /// Gets or sets the export format.
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Excel;

    /// <summary>
    /// Gets or sets the title for the export.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the columns to export.
    /// </summary>
    public List<ExportColumn> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets the data to export.
    /// </summary>
    public IEnumerable<Dictionary<string, object?>> Data { get; set; } = Enumerable.Empty<Dictionary<string, object?>>();
}
