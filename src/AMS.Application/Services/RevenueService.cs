using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class RevenueService : IRevenueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRevenueRepository _revenueRepository;
    private readonly IRevenueRecognitionRepository _recognitionRepository;
    private readonly ICustomerRepository _customerRepository;

    public RevenueService(
        IUnitOfWork unitOfWork,
        IRevenueRepository revenueRepository,
        IRevenueRecognitionRepository recognitionRepository,
        ICustomerRepository customerRepository)
    {
        _unitOfWork = unitOfWork;
        _revenueRepository = revenueRepository;
        _recognitionRepository = recognitionRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ServiceResult<RevenueDto>> CreateRevenueAsync(CreateRevenueDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            return ServiceResult<RevenueDto>.Failure("Khách hàng không tồn tại.");

        var revenue = new Revenue
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            VoucherNo = dto.VoucherNo,
            VoucherDate = dto.VoucherDate,
            RevenueType = Enum.Parse<RevenueType>(dto.RevenueType),
            Amount = dto.Amount,
            TaxAmount = dto.TaxAmount,
            RecognitionDate = dto.RecognitionDate,
            Description = dto.Description,
            VoucherId = dto.VoucherId,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            IsRecognized = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _revenueRepository.AddAsync(revenue);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<RevenueDto>.Success(MapToDto(revenue, customer.Name));
    }

    public async Task<ServiceResult<RevenueDto>> UpdateRevenueAsync(Guid id, UpdateRevenueDto dto)
    {
        var revenue = await _revenueRepository.GetByIdAsync(id);
        if (revenue == null)
            return ServiceResult<RevenueDto>.Failure("Doanh thu không tồn tại.");

        if (revenue.IsRecognized)
            return ServiceResult<RevenueDto>.Failure("Doanh thu đã được ghi nhận, không thể cập nhật.");

        revenue.Amount = dto.Amount;
        revenue.TaxAmount = dto.TaxAmount;
        revenue.RecognitionDate = dto.RecognitionDate;
        revenue.Description = dto.Description;
        revenue.ModifiedAt = DateTime.UtcNow;

        await _revenueRepository.UpdateAsync(revenue);
        await _unitOfWork.SaveChangesAsync();

        var customer = await _customerRepository.GetByIdAsync(revenue.CustomerId);
        return ServiceResult<RevenueDto>.Success(MapToDto(revenue, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<RevenueDto>> GetRevenueByIdAsync(Guid id)
    {
        var revenue = await _revenueRepository.GetByIdAsync(id);
        if (revenue == null)
            return ServiceResult<RevenueDto>.Failure("Doanh thu không tồn tại.");

        var customer = await _customerRepository.GetByIdAsync(revenue.CustomerId);
        return ServiceResult<RevenueDto>.Success(MapToDto(revenue, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<List<RevenueDto>>> GetRevenuesByCustomerAsync(Guid customerId)
    {
        var revenues = await _revenueRepository.GetByCustomerAsync(customerId);
        var customer = await _customerRepository.GetByIdAsync(customerId);
        var dtos = revenues.Select(r => MapToDto(r, customer?.Name ?? "")).ToList();
        return ServiceResult<List<RevenueDto>>.Success(dtos);
    }

    public async Task<ServiceResult<List<RevenueDto>>> GetRevenuesByPeriodAsync(int year, int month)
    {
        var revenues = await _revenueRepository.GetByPeriodAsync(year, month);
        var dtos = new List<RevenueDto>();

        foreach (var r in revenues)
        {
            var customer = await _customerRepository.GetByIdAsync(r.CustomerId);
            dtos.Add(MapToDto(r, customer?.Name ?? ""));
        }

        return ServiceResult<List<RevenueDto>>.Success(dtos);
    }

    public async Task<ServiceResult<List<RevenueDto>>> GetUnrecognizedRevenuesAsync(int page, int pageSize)
    {
        var (revenues, totalCount) = await _revenueRepository.GetAllPagedAsync(page, pageSize);
        var unrecognized = revenues.Where(r => !r.IsRecognized).ToList();
        var dtos = new List<RevenueDto>();

        foreach (var r in unrecognized)
        {
            var customer = await _customerRepository.GetByIdAsync(r.CustomerId);
            dtos.Add(MapToDto(r, customer?.Name ?? ""));
        }

        return ServiceResult<List<RevenueDto>>.Success(dtos);
    }

    public async Task<ServiceResult<RevenueDto>> RecognizeRevenueAsync(Guid id, int year, int month)
    {
        var revenue = await _revenueRepository.GetByIdAsync(id);
        if (revenue == null)
            return ServiceResult<RevenueDto>.Failure("Doanh thu không tồn tại.");

        if (revenue.IsRecognized)
            return ServiceResult<RevenueDto>.Failure("Doanh thu đã được ghi nhận.");

        var recognition = new RevenueRecognition
        {
            Id = Guid.NewGuid(),
            RevenueId = id,
            Year = year,
            Month = month,
            Amount = revenue.NetAmount,
            RecognizedAmount = revenue.NetAmount,
            RemainingAmount = 0,
            RecognitionDate = DateTime.UtcNow,
            Description = $"Ghi nhận doanh thu {year}/{month}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        revenue.IsRecognized = true;
        revenue.RecognitionDate = DateTime.UtcNow;
        revenue.ModifiedAt = DateTime.UtcNow;

        await _recognitionRepository.AddAsync(recognition);
        await _revenueRepository.UpdateAsync(revenue);
        await _unitOfWork.SaveChangesAsync();

        var customer = await _customerRepository.GetByIdAsync(revenue.CustomerId);
        return ServiceResult<RevenueDto>.Success(MapToDto(revenue, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<RevenueReportDto>> GetRevenueSummaryAsync(int year, int month)
    {
        return await GenerateRevenueReportAsync(year, month, null);
    }

    public async Task<ServiceResult<RevenueReportDto>> GenerateRevenueReportAsync(int year, int month, Guid? customerId = null)
    {
        var revenues = customerId.HasValue
            ? await _revenueRepository.GetByCustomerAsync(customerId.Value)
            : (await _revenueRepository.GetByPeriodAsync(year, month)).ToList();

        var previousPeriod = await _revenueRepository.GetByPeriodAsync(year, month - 1);
        var previousTotal = previousPeriod.Sum(r => r.NetAmount);

        var totalRevenue = revenues.Sum(r => r.Amount);
        var totalTax = revenues.Sum(r => r.TaxAmount);
        var netRevenue = totalRevenue - totalTax;

        var growthPercentage = previousTotal > 0 ? ((netRevenue - previousTotal) / previousTotal) * 100 : 0;

        var customerIds = revenues.Select(r => r.CustomerId).Distinct();
        var details = new List<RevenueReportDetailDto>();

        foreach (var cid in customerIds)
        {
            var customer = await _customerRepository.GetByIdAsync(cid);
            var customerRevenues = revenues.Where(r => r.CustomerId == cid).ToList();
            var custRevenue = customerRevenues.Sum(r => r.Amount);
            var custTax = customerRevenues.Sum(r => r.TaxAmount);

            details.Add(new RevenueReportDetailDto
            {
                CustomerId = cid,
                CustomerCode = customer?.Code ?? "",
                CustomerName = customer?.Name ?? "",
                RevenueAmount = custRevenue,
                TaxAmount = custTax,
                NetRevenue = custRevenue - custTax,
                Percentage = totalRevenue > 0 ? (custRevenue / totalRevenue) * 100 : 0
            });
        }

        var result = new RevenueReportDto
        {
            Year = year,
            Month = month,
            ReportType = customerId.HasValue ? "ByCustomer" : "Summary",
            TotalRevenue = totalRevenue,
            TotalTax = totalTax,
            NetRevenue = netRevenue,
            PreviousPeriodRevenue = previousTotal,
            GrowthPercentage = Math.Round(growthPercentage, 2),
            Details = details.OrderByDescending(d => d.NetRevenue).ToList()
        };

        return ServiceResult<RevenueReportDto>.Success(result);
    }

    private static RevenueDto MapToDto(Revenue entity, string customerName)
    {
        return new RevenueDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = customerName,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            RevenueType = entity.RevenueType.ToString(),
            Amount = entity.Amount,
            TaxAmount = entity.TaxAmount,
            NetAmount = entity.NetAmount,
            RecognitionDate = entity.RecognitionDate,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode,
            IsRecognized = entity.IsRecognized,
            CreatedDate = entity.CreatedAt
        };
    }
}