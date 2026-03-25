using AMS.Domain.Entities.Tax;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for tax-related entities (TaxRates, PITBrackets, PITAllowances).
/// </summary>
public class TaxRepository : ITaxRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the TaxRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TaxRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<TaxRate?> GetTaxRateByKeyAsync(string taxRateKey, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.TaxRates
            .Where(r => r.TaxRateKey == taxRateKey 
                && r.IsActive 
                && r.EffectiveFrom <= date 
                && (r.EffectiveTo == null || r.EffectiveTo >= date))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TaxRate>> GetActiveTaxRatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaxRates
            .Where(r => r.IsActive)
            .OrderBy(r => r.TaxType)
            .ThenByDescending(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default)
    {
        await _context.TaxRates.AddAsync(taxRate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PITBracket>> GetActivePITBracketsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PITBrackets
            .Where(b => b.IsActive)
            .OrderBy(b => b.BracketNo)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPITBracketAsync(PITBracket bracket, CancellationToken cancellationToken = default)
    {
        await _context.PITBrackets.AddAsync(bracket, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PITAllowance>> GetEmployeeAllowancesAsync(Guid employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.PITAllowances
            .Where(a => a.EmployeeId == employeeId 
                && a.IsActive 
                && a.EffectiveFrom <= date 
                && (a.EffectiveTo == null || a.EffectiveTo >= date))
            .ToListAsync(cancellationToken);
    }
}
