using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Tax;
using AMS.Domain.Enums;
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

    public async Task<ServiceResult<TaxDeclarationDto>> CreateTaxDeclarationAsync(CreateTaxDeclarationDto dto, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.TaxType))
            errors.Add("Loại thuế không được để trống.");

        if (dto.PeriodYear < 2000 || dto.PeriodYear > 2100)
            errors.Add("Năm không hợp lệ.");

        if (dto.PeriodMonth < 1 || dto.PeriodMonth > 12)
            errors.Add("Tháng không hợp lệ.");

        if (errors.Any())
            return ServiceResult<TaxDeclarationDto>.Failure(errors);

        var declaration = new TaxDeclaration
        {
            Id = Guid.NewGuid(),
            TaxType = dto.TaxType,
            PeriodYear = dto.PeriodYear,
            PeriodMonth = dto.PeriodMonth,
            PeriodType = dto.PeriodType,
            Status = TaxDeclarationStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _taxRepository.AddTaxDeclarationAsync(declaration, cancellationToken);

        return ServiceResult<TaxDeclarationDto>.Success(MapToTaxDeclarationDto(declaration));
    }

    public async Task<ServiceResult<TaxDeclarationDto>> SubmitTaxDeclarationAsync(Guid declarationId, string submittedBy, CancellationToken cancellationToken = default)
    {
        var declaration = await _taxRepository.GetTaxDeclarationByIdAsync(declarationId, cancellationToken);
        if (declaration == null)
            return ServiceResult<TaxDeclarationDto>.Failure("Khai báo thuế không tìm thấy.");

        if (declaration.Status != TaxDeclarationStatus.Draft)
            return ServiceResult<TaxDeclarationDto>.Failure("Chỉ được nộp khai báo thuế ở trạng thái nháp.");

        declaration.Status = TaxDeclarationStatus.Submitted;
        declaration.SubmittedDate = DateTime.UtcNow;
        declaration.SubmittedBy = submittedBy;

        await _taxRepository.UpdateTaxDeclarationAsync(declaration, cancellationToken);

        return ServiceResult<TaxDeclarationDto>.Success(MapToTaxDeclarationDto(declaration));
    }

    public async Task<IEnumerable<TaxDeclarationDto>> GetTaxDeclarationsAsync(string? taxType, int? year, CancellationToken cancellationToken = default)
    {
        var declarations = await _taxRepository.GetTaxDeclarationsAsync(taxType, year, cancellationToken);
        return declarations.Select(MapToTaxDeclarationDto);
    }

    public async Task<TaxDeclarationDto?> GetTaxDeclarationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var declaration = await _taxRepository.GetTaxDeclarationByIdAsync(id, cancellationToken);
        return declaration == null ? null : MapToTaxDeclarationDto(declaration);
    }

    public async Task<IEnumerable<VATRegisterDto>> GetVATInputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var registers = await _taxRepository.GetVATInputRegisterAsync(fiscalPeriodId, cancellationToken);
        return registers.Select(MapToVATInputRegisterDto);
    }

    public async Task<IEnumerable<VATRegisterDto>> GetVATOutputRegisterAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var registers = await _taxRepository.GetVATOutputRegisterAsync(fiscalPeriodId, cancellationToken);
        return registers.Select(MapToVATOutputRegisterDto);
    }

    private static TaxDeclarationDto MapToTaxDeclarationDto(TaxDeclaration entity) => new()
    {
        Id = entity.Id,
        TaxType = entity.TaxType,
        PeriodYear = entity.PeriodYear,
        PeriodMonth = entity.PeriodMonth,
        PeriodType = entity.PeriodType,
        Status = entity.Status.ToString(),
        TotalTaxDue = entity.TotalTaxDue,
        TotalTaxPaid = entity.TotalTaxPaid,
        TaxDifference = entity.TaxDifference,
        SubmittedDate = entity.SubmittedDate,
        AcceptedDate = entity.AcceptedDate,
        Note = entity.Note,
        SubmittedBy = entity.SubmittedBy,
        ApprovedBy = entity.ApprovedBy
    };

    private static VATRegisterDto MapToVATInputRegisterDto(VATInputRegister entity) => new()
    {
        Id = entity.Id,
        VoucherId = entity.VoucherId,
        VendorId = entity.VendorId,
        InvoiceNo = entity.InvoiceNo,
        InvoiceDate = entity.InvoiceDate,
        TotalAmount = entity.TotalAmount,
        VatAmount = entity.VatAmount,
        VatRate = entity.VatRate,
        GoodsAmount = entity.GoodsAmount,
        IsClaimed = entity.IsClaimed,
        ClaimedDate = entity.ClaimedDate
    };

    private static VATRegisterDto MapToVATOutputRegisterDto(VATOutputRegister entity) => new()
    {
        Id = entity.Id,
        VoucherId = entity.VoucherId,
        VendorId = entity.CustomerId,
        InvoiceNo = entity.InvoiceNo,
        InvoiceDate = entity.InvoiceDate,
        TotalAmount = entity.TotalAmount,
        VatAmount = entity.VatAmount,
        VatRate = entity.VatRate,
        GoodsAmount = entity.GoodsAmount,
        IsClaimed = entity.IsCreditNote,
        ClaimedDate = null
    };
}
