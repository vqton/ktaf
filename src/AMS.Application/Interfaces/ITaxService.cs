using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface ITaxService
{
    Task<TaxRateDto?> GetTaxRateByKeyAsync(string taxRateKey, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaxRateDto>> GetActiveTaxRatesAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<TaxRateDto>> CreateTaxRateAsync(CreateTaxRateDto dto, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PITBracketDto>> GetActivePITBracketsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<PITBracketDto>> CreatePITBracketAsync(CreatePITBracketDto dto, CancellationToken cancellationToken = default);
    
    PITResultDto CalculatePIT(decimal grossIncome, long personalDeduction, IEnumerable<PITAllowanceDto> allowances);
    Task<PITResultDto> CalculateEmployeePITAsync(Guid employeeId, decimal grossIncome, DateTime calculationDate, CancellationToken cancellationToken = default);
}
