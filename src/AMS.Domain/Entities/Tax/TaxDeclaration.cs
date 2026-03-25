using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a tax declaration submitted to tax authorities.
/// Tracks VAT, CIT, PIT declarations and their statuses.
/// </summary>
public class TaxDeclaration : BaseEntity
{
    /// <summary>
    /// Type of tax being declared (GTGT, TNDN, TNCN, TTDB).
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Year of the tax period.
    /// </summary>
    public int PeriodYear { get; set; }

    /// <summary>
    /// Month of the tax period.
    /// </summary>
    public int PeriodMonth { get; set; }

    /// <summary>
    /// Type of period (MONTH, QUARTER, YEAR).
    /// </summary>
    public string PeriodType { get; set; } = "MONTH";

    /// <summary>
    /// Current status of the tax declaration.
    /// </summary>
    public TaxDeclarationStatus Status { get; set; } = TaxDeclarationStatus.Draft;

    /// <summary>
    /// Total tax amount due.
    /// </summary>
    public decimal? TotalTaxDue { get; set; }

    /// <summary>
    /// Total tax amount already paid.
    /// </summary>
    public decimal? TotalTaxPaid { get; set; }

    /// <summary>
    /// Difference between tax due and tax paid.
    /// </summary>
    public decimal? TaxDifference { get; set; }

    /// <summary>
    /// Date when the declaration was submitted to tax authority.
    /// </summary>
    public DateTime? SubmittedDate { get; set; }

    /// <summary>
    /// Date when the declaration was accepted by tax authority.
    /// </summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>
    /// XML content of the declaration (for iTaxViewer import).
    /// </summary>
    public string? XmlContent { get; set; }

    /// <summary>
    /// File path to the saved XML declaration.
    /// </summary>
    public string? XmlFilePath { get; set; }

    /// <summary>
    /// Additional notes/comments.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Username of the person who submitted the declaration.
    /// </summary>
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Username of the person who approved the declaration.
    /// </summary>
    public string? ApprovedBy { get; set; }
}
