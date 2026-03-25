using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Tax;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class TaxService : ITaxService
{
    private readonly ITaxRepository _taxRepository;

    public TaxService(ITaxRepository taxRepository)
    {
        _taxRepository = taxRepository;
    }

    public async Task<TaxRateDto?> GetTaxRateByKeyAsync(string taxRateKey, DateTime date, CancellationToken cancellationToken = default)
    {
        var rate = await _taxRepository.GetTaxRateByKeyAsync(taxRateKey, date, cancellationToken);
        return rate == null ? null : MapToTaxRateDto(rate);
    }

    public async Task<IEnumerable<TaxRateDto>> GetActiveTaxRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = await _taxRepository.GetActiveTaxRatesAsync(cancellationToken);
        return rates.Select(MapToTaxRateDto);
    }

    public async Task<ServiceResult<TaxRateDto>> CreateTaxRateAsync(CreateTaxRateDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateTaxRate(dto);
        if (errors.Any())
            return ServiceResult<TaxRateDto>.Failure(errors);

        var entity = new TaxRate
        {
            Id = Guid.NewGuid(),
            TaxRateKey = dto.TaxRateKey,
            TaxType = dto.TaxType,
            Rate = dto.Rate,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            LegalBasis = dto.LegalBasis,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _taxRepository.AddTaxRateAsync(entity, cancellationToken);

        return ServiceResult<TaxRateDto>.Success(MapToTaxRateDto(entity));
    }

    public async Task<IEnumerable<PITBracketDto>> GetActivePITBracketsAsync(CancellationToken cancellationToken = default)
    {
        var brackets = await _taxRepository.GetActivePITBracketsAsync(cancellationToken);
        return brackets.Select(MapToPITBracketDto);
    }

    public async Task<ServiceResult<PITBracketDto>> CreatePITBracketAsync(CreatePITBracketDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.BracketNo < 1 || dto.BracketNo > 7)
            return ServiceResult<PITBracketDto>.Failure("Bậc thuế TNCN phải từ 1-7.");

        if (dto.FromAmount < 0)
            return ServiceResult<PITBracketDto>.Failure("Thu nhập tối thiểu không được âm.");

        var entity = new PITBracket
        {
            Id = Guid.NewGuid(),
            BracketNo = dto.BracketNo,
            FromAmount = dto.FromAmount,
            ToAmount = dto.ToAmount,
            TaxRate = dto.TaxRate,
            QuickDeduction = dto.QuickDeduction,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            LegalBasis = dto.LegalBasis,
            IsActive = true
        };

        await _taxRepository.AddPITBracketAsync(entity, cancellationToken);

        return ServiceResult<PITBracketDto>.Success(MapToPITBracketDto(entity));
    }

    public PITResultDto CalculatePIT(decimal grossIncome, long personalDeduction, IEnumerable<PITAllowanceDto> allowances)
    {
        var totalAllowances = allowances.Sum(a => (long)a.Amount);
        var taxableIncome = (long)grossIncome - personalDeduction - totalAllowances;

        if (taxableIncome <= 0)
        {
            return new PITResultDto
            {
                GrossIncome = grossIncome,
                TotalDeductions = personalDeduction + totalAllowances,
                TaxableIncome = 0,
                PITAmount = 0,
                NetIncome = grossIncome,
                BracketNo = 0,
                BracketRate = 0
            };
        }

        var brackets = GetVietnamesePITBrackets();
        var (bracketNo, bracketRate, pitAmount) = CalculateProgressivePIT(taxableIncome, brackets);

        return new PITResultDto
        {
            GrossIncome = grossIncome,
            TotalDeductions = personalDeduction + totalAllowances,
            TaxableIncome = taxableIncome,
            PITAmount = pitAmount,
            NetIncome = grossIncome - pitAmount,
            BracketNo = bracketNo,
            BracketRate = bracketRate
        };
    }

    public async Task<PITResultDto> CalculateEmployeePITAsync(Guid employeeId, decimal grossIncome, DateTime calculationDate, CancellationToken cancellationToken = default)
    {
        var personalDeduction = AppConstants.DefaultPersonalDeduction;
        var allowances = await GetEmployeeAllowancesAsync(employeeId, calculationDate, cancellationToken);

        return CalculatePIT(grossIncome, personalDeduction, allowances);
    }

    private async Task<List<PITAllowanceDto>> GetEmployeeAllowancesAsync(Guid employeeId, DateTime date, CancellationToken cancellationToken)
    {
        var allowances = await _taxRepository.GetEmployeeAllowancesAsync(employeeId, date, cancellationToken);

        return allowances.Select(a => new PITAllowanceDto
        {
            AllowanceType = a.AllowanceType,
            Amount = a.Amount
        }).ToList();
    }

    private static List<(int BracketNo, long FromAmount, long? ToAmount, decimal TaxRate, long QuickDeduction)> GetVietnamesePITBrackets()
    {
        return new List<(int, long, long?, decimal, long)>
        {
            (1, 0, 5000000, 0.05m, 0),
            (2, 5000000, 10000000, 0.10m, 250000),
            (3, 10000000, 18000000, 0.15m, 750000),
            (4, 18000000, 32000000, 0.20m, 1650000),
            (5, 32000000, 52000000, 0.25m, 3250000),
            (6, 52000000, 78000000, 0.30m, 5850000),
            (7, 78000000, null, 0.35m, 9850000)
        };
    }

    private static (int BracketNo, decimal BracketRate, decimal PITAmount) CalculateProgressivePIT(long taxableIncome, List<(int BracketNo, long FromAmount, long? ToAmount, decimal TaxRate, long QuickDeduction)> brackets)
    {
        foreach (var bracket in brackets)
        {
            var toAmount = bracket.ToAmount ?? long.MaxValue;
            if (taxableIncome <= toAmount)
            {
                var pitAmount = taxableIncome * bracket.TaxRate - bracket.QuickDeduction;
                return (bracket.BracketNo, bracket.TaxRate, pitAmount > 0 ? pitAmount : 0);
            }
        }

        var lastBracket = brackets.Last();
        var pitAmount2 = taxableIncome * lastBracket.TaxRate - lastBracket.QuickDeduction;
        return (lastBracket.BracketNo, lastBracket.TaxRate, pitAmount2 > 0 ? pitAmount2 : 0);
    }

    private static List<string> ValidateTaxRate(CreateTaxRateDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.TaxRateKey))
            errors.Add("Mã thuế không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.TaxType))
            errors.Add("Loại thuế không được để trống.");
        if (dto.Rate < 0 || dto.Rate > 1)
            errors.Add("Tỷ lệ thuế phải từ 0-100%.");
        return errors;
    }

    private static TaxRateDto MapToTaxRateDto(TaxRate entity) => new()
    {
        Id = entity.Id,
        TaxRateKey = entity.TaxRateKey,
        TaxType = entity.TaxType,
        Rate = entity.Rate,
        EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo,
        LegalBasis = entity.LegalBasis,
        Description = entity.Description,
        IsActive = entity.IsActive
    };

    private static PITBracketDto MapToPITBracketDto(PITBracket entity) => new()
    {
        Id = entity.Id,
        BracketNo = entity.BracketNo,
        FromAmount = entity.FromAmount,
        ToAmount = entity.ToAmount,
        TaxRate = entity.TaxRate,
        QuickDeduction = entity.QuickDeduction,
        EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo,
        LegalBasis = entity.LegalBasis,
        IsActive = entity.IsActive
    };
}
