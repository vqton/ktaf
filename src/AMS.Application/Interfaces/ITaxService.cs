using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for managing tax-related operations including tax rates, declarations, and VAT registers.
/// </summary>
public interface ITaxService
{
    /// <summary>
    /// Gets a tax rate by its key and effective date.
    /// </summary>
    /// <param name="taxRateKey">The unique key of the tax rate.</param>
    /// <param name="date">The date to check effectiveness.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tax rate DTO if found; otherwise, null.</returns>
    Task<TaxRateDto?> GetTaxRateByKeyAsync(string taxRateKey, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active tax rates.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tax rate DTOs.</returns>
    Task<IEnumerable<TaxRateDto>> GetActiveTaxRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tax rate.
    /// </summary>
    /// <param name="dto">The tax rate data to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result containing the created tax rate.</returns>
    Task<ServiceResult<TaxRateDto>> CreateTaxRateAsync(CreateTaxRateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active personal income tax (PIT) brackets.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active PIT bracket DTOs.</returns>
    Task<IEnumerable<PITBracketDto>> GetActivePITBracketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new PIT bracket.
    /// </summary>
    /// <param name="dto">The PIT bracket data to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result containing the created PIT bracket.</returns>
    Task<ServiceResult<PITBracketDto>> CreatePITBracketAsync(CreatePITBracketDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates personal income tax based on gross income and deductions.
    /// </summary>
    /// <param name="grossIncome">The gross income amount.</param>
    /// <param name="personalDeduction">The personal deduction amount.</param>
    /// <param name="allowances">The collection of allowances.</param>
    /// <returns>The calculated PIT result.</returns>
    PITResultDto CalculatePIT(decimal grossIncome, long personalDeduction, IEnumerable<PITAllowanceDto> allowances);

    /// <summary>
    /// Calculates personal income tax for an employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="grossIncome">The gross income amount.</param>
    /// <param name="calculationDate">The date of calculation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The calculated PIT result with employee allowances.</returns>
    Task<PITResultDto> CalculateEmployeePITAsync(Guid employeeId, decimal grossIncome, DateTime calculationDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tax declaration.
    /// </summary>
    /// <param name="dto">The tax declaration data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result containing the created tax declaration.</returns>
    Task<ServiceResult<TaxDeclarationDto>> CreateTaxDeclarationAsync(CreateTaxDeclarationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a tax declaration to the tax authority.
    /// </summary>
    /// <param name="declarationId">The unique identifier of the declaration.</param>
    /// <param name="submittedBy">The username of the submitter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result containing the updated tax declaration.</returns>
    Task<ServiceResult<TaxDeclarationDto>> SubmitTaxDeclarationAsync(Guid declarationId, string submittedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tax declarations filtered by type and year.
    /// </summary>
    /// <param name="taxType">The type of tax (GTGT, TNDN, TNCN).</param>
    /// <param name="year">The year to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of matching tax declaration DTOs.</returns>
    Task<IEnumerable<TaxDeclarationDto>> GetTaxDeclarationsAsync(string? taxType, int? year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tax declaration by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tax declaration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tax declaration DTO if found; otherwise, null.</returns>
    Task<TaxDeclarationDto?> GetTaxDeclarationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the VAT input register for a fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of VAT input register entries.</returns>
    Task<IEnumerable<VATRegisterDto>> GetVATInputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the VAT output register for a fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of VAT output register entries.</returns>
    Task<IEnumerable<VATRegisterDto>> GetVATOutputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);
}
