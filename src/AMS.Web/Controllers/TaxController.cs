using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;

namespace AMS.Web.Controllers;

/// <summary>
/// Controller for managing tax-related operations including VAT declarations, corporate income tax (TNDN), and personal income tax (TNCN).
/// </summary>
public class TaxController : BaseController
{
    private readonly ITaxService _taxService;
    private readonly IFiscalPeriodService _fiscalPeriodService;

    /// <summary>
    /// Initializes a new instance of the TaxController class.
    /// </summary>
    /// <param name="taxService">The tax service for business logic.</param>
    /// <param name="fiscalPeriodService">The fiscal period service.</param>
    public TaxController(ITaxService taxService, IFiscalPeriodService fiscalPeriodService)
    {
        _taxService = taxService;
        _fiscalPeriodService = fiscalPeriodService;
    }

    /// <summary>
    /// Displays the tax management index page.
    /// </summary>
    /// <returns>The index view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the VAT (GTGT) declaration page for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <returns>The GTGT view with VAT input and output registers.</returns>
    [HttpGet]
    public async Task<IActionResult> GTGT(Guid fiscalPeriodId)
    {
        var inputVat = await _taxService.GetVATInputRegisterAsync(fiscalPeriodId);
        var outputVat = await _taxService.GetVATOutputRegisterAsync(fiscalPeriodId);

        var model = new GTGTViewModel
        {
            FiscalPeriodId = fiscalPeriodId,
            VATInputs = inputVat.ToList(),
            VATOutputs = outputVat.ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Displays the corporate income tax (TNDN) declaration page.
    /// </summary>
    /// <param name="year">The tax year.</param>
    /// <param name="month">The tax month.</param>
    /// <returns>The TNDN view with tax declarations.</returns>
    [HttpGet]
    public async Task<IActionResult> TNDN(int year, int month)
    {
        var declarations = await _taxService.GetTaxDeclarationsAsync("TNDN", year);
        return View(declarations);
    }

    /// <summary>
    /// Displays the personal income tax (TNCN) brackets page.
    /// </summary>
    /// <returns>The TNCN view with progressive tax brackets.</returns>
    [HttpGet]
    public async Task<IActionResult> TNCN()
    {
        var brackets = await _taxService.GetActivePITBracketsAsync();
        return View(brackets);
    }

    /// <summary>
    /// Creates a new tax declaration.
    /// </summary>
    /// <param name="dto">The tax declaration data.</param>
    /// <returns>A JSON result indicating success or failure.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateDeclaration(CreateTaxDeclarationDto dto)
    {
        var result = await _taxService.CreateTaxDeclarationAsync(dto);
        return FromResult(result);
    }

    /// <summary>
    /// Submits an existing tax declaration to the tax authority.
    /// </summary>
    /// <param name="id">The unique identifier of the tax declaration.</param>
    /// <param name="submittedBy">The username of the person submitting.</param>
    /// <returns>A JSON result indicating success or failure.</returns>
    [HttpPost]
    public async Task<IActionResult> SubmitDeclaration(Guid id, string submittedBy)
    {
        var result = await _taxService.SubmitTaxDeclarationAsync(id, submittedBy);
        return FromResult(result);
    }

    /// <summary>
    /// Gets tax declarations filtered by type and year.
    /// </summary>
    /// <param name="taxType">The type of tax (e.g., GTGT, TNDN, TNCN).</param>
    /// <param name="year">The year to filter by.</param>
    /// <returns>A JSON result containing the matching declarations.</returns>
    [HttpGet]
    public async Task<IActionResult> GetDeclarations(string? taxType, int? year)
    {
        var declarations = await _taxService.GetTaxDeclarationsAsync(taxType, year);
        return Success(declarations);
    }

    /// <summary>
    /// Calculates personal income tax based on gross income and deductions.
    /// </summary>
    /// <param name="grossIncome">The gross income amount.</param>
    /// <param name="personalDeduction">The personal deduction amount.</param>
    /// <param name="allowances">The list of allowances.</param>
    /// <returns>A JSON result containing the calculated PIT result.</returns>
    [HttpPost]
    public IActionResult CalculatePIT(decimal grossIncome, long personalDeduction, List<PITAllowanceDto> allowances)
    {
        var result = _taxService.CalculatePIT(grossIncome, personalDeduction, allowances);
        return Success(result);
    }
}

/// <summary>
/// View model for displaying VAT (GTGT) declaration data.
/// </summary>
public class GTGTViewModel
{
    /// <summary>
    /// Gets or sets the fiscal period identifier.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Gets or sets the VAT input register entries.
    /// </summary>
    public List<VATRegisterDto> VATInputs { get; set; } = new();

    /// <summary>
    /// Gets or sets the VAT output register entries.
    /// </summary>
    public List<VATRegisterDto> VATOutputs { get; set; } = new();
}
