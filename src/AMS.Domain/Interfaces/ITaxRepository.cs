using AMS.Domain.Entities.Tax;

namespace AMS.Domain.Interfaces;

public interface ITaxRepository
{
    Task<TaxRate?> GetTaxRateByKeyAsync(string taxRateKey, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaxRate>> GetActiveTaxRatesAsync(CancellationToken cancellationToken = default);
    Task AddTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default);

    Task<IEnumerable<PITBracket>> GetActivePITBracketsAsync(CancellationToken cancellationToken = default);
    Task AddPITBracketAsync(PITBracket bracket, CancellationToken cancellationToken = default);

    Task<IEnumerable<PITAllowance>> GetEmployeeAllowancesAsync(Guid employeeId, DateTime date, CancellationToken cancellationToken = default);

    Task<TaxDeclaration?> GetTaxDeclarationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaxDeclaration>> GetTaxDeclarationsAsync(string? taxType, int? year, CancellationToken cancellationToken = default);
    Task AddTaxDeclarationAsync(TaxDeclaration declaration, CancellationToken cancellationToken = default);
    Task UpdateTaxDeclarationAsync(TaxDeclaration declaration, CancellationToken cancellationToken = default);

    Task<IEnumerable<VATInputRegister>> GetVATInputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VATOutputRegister>> GetVATOutputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);
}
