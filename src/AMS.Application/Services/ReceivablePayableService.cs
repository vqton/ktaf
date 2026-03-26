using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class ReceivablePayableService : IReceivablePayableService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReceivableRepository _receivableRepository;
    private readonly IPayableRepository _payableRepository;
    private readonly IReceivablePaymentRepository _receivablePaymentRepository;
    private readonly IPayablePaymentRepository _payablePaymentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IAgingReportRepository _agingReportRepository;

    public ReceivablePayableService(
        IUnitOfWork unitOfWork,
        IReceivableRepository receivableRepository,
        IPayableRepository payableRepository,
        IReceivablePaymentRepository receivablePaymentRepository,
        IPayablePaymentRepository payablePaymentRepository,
        ICustomerRepository customerRepository,
        IVendorRepository vendorRepository,
        IAgingReportRepository agingReportRepository)
    {
        _unitOfWork = unitOfWork;
        _receivableRepository = receivableRepository;
        _payableRepository = payableRepository;
        _receivablePaymentRepository = receivablePaymentRepository;
        _payablePaymentRepository = payablePaymentRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _agingReportRepository = agingReportRepository;
    }

    public async Task<ServiceResult<ReceivableDto>> CreateReceivableAsync(CreateReceivableDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            return ServiceResult<ReceivableDto>.Failure("Khách hàng không tồn tại.");

        var receivable = new Receivable
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            VoucherNo = dto.VoucherNo,
            VoucherDate = dto.VoucherDate,
            ReceivableType = Enum.Parse<ReceivableType>(dto.ReceivableType),
            Amount = dto.Amount,
            PaidAmount = 0,
            DueDate = dto.DueDate,
            Description = dto.Description,
            VoucherId = dto.VoucherId,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _receivableRepository.AddAsync(receivable);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<ReceivableDto>.Success(MapToDto(receivable, customer.Name));
    }

    public async Task<ServiceResult<ReceivableDto>> UpdateReceivableAsync(Guid id, UpdateReceivableDto dto)
    {
        var receivable = await _receivableRepository.GetByIdAsync(id);
        if (receivable == null)
            return ServiceResult<ReceivableDto>.Failure("Công nợ phải thu không tồn tại.");

        if (receivable.PaidAmount > 0)
            return ServiceResult<ReceivableDto>.Failure("Công nợ đã thanh toán một phần, không thể cập nhật.");

        receivable.Amount = dto.Amount;
        receivable.DueDate = dto.DueDate;
        receivable.Description = dto.Description;
        receivable.ModifiedAt = DateTime.UtcNow;

        await _receivableRepository.UpdateAsync(receivable);
        await _unitOfWork.SaveChangesAsync();

        var customer = await _customerRepository.GetByIdAsync(receivable.CustomerId);
        return ServiceResult<ReceivableDto>.Success(MapToDto(receivable, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<ReceivableDto>> GetReceivableByIdAsync(Guid id)
    {
        var receivable = await _receivableRepository.GetByIdAsync(id);
        if (receivable == null)
            return ServiceResult<ReceivableDto>.Failure("Công nợ phải thu không tồn tại.");

        var customer = await _customerRepository.GetByIdAsync(receivable.CustomerId);
        return ServiceResult<ReceivableDto>.Success(MapToDto(receivable, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<List<ReceivableDto>>> GetReceivablesByCustomerAsync(Guid customerId)
    {
        var receivables = await _receivableRepository.GetByCustomerAsync(customerId);
        var customer = await _customerRepository.GetByIdAsync(customerId);
        var dtos = receivables.Select(r => MapToDto(r, customer?.Name ?? "")).ToList();
        return ServiceResult<List<ReceivableDto>>.Success(dtos);
    }

    public async Task<ServiceResult<List<ReceivableDto>>> GetUnpaidReceivablesAsync(int page, int pageSize)
    {
        var (receivables, totalCount) = await _receivableRepository.GetAllPagedAsync(page, pageSize);
        var unpaidReceivables = receivables.Where(r => r.RemainingAmount > 0).ToList();
        var dtos = new List<ReceivableDto>();
        
        foreach (var r in unpaidReceivables)
        {
            var customer = await _customerRepository.GetByIdAsync(r.CustomerId);
            dtos.Add(MapToDto(r, customer?.Name ?? ""));
        }
        
        return ServiceResult<List<ReceivableDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ReceivableDto>> RecordReceivablePaymentAsync(Guid receivableId, RecordPaymentDto dto)
    {
        var receivable = await _receivableRepository.GetByIdAsync(receivableId);
        if (receivable == null)
            return ServiceResult<ReceivableDto>.Failure("Công nợ phải thu không tồn tại.");

        if (dto.Amount > receivable.RemainingAmount)
            return ServiceResult<ReceivableDto>.Failure($"Số tiền thanh toán vượt quá số tiền còn lại ({receivable.RemainingAmount:N0}).");

        var payment = new ReceivablePayment
        {
            Id = Guid.NewGuid(),
            ReceivableId = receivableId,
            VoucherNo = dto.VoucherNo,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            ReferenceNo = dto.ReferenceNo,
            Notes = dto.Notes,
            VoucherId = dto.VoucherId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        receivable.PaidAmount += dto.Amount;
        receivable.IsReconciled = receivable.RemainingAmount == 0;
        receivable.ModifiedAt = DateTime.UtcNow;

        await _receivablePaymentRepository.AddAsync(payment);
        await _receivableRepository.UpdateAsync(receivable);
        await _unitOfWork.SaveChangesAsync();

        var customer = await _customerRepository.GetByIdAsync(receivable.CustomerId);
        return ServiceResult<ReceivableDto>.Success(MapToDto(receivable, customer?.Name ?? ""));
    }

    public async Task<ServiceResult<PayableDto>> CreatePayableAsync(CreatePayableDto dto)
    {
        var vendor = await _vendorRepository.GetByIdAsync(dto.VendorId);
        if (vendor == null)
            return ServiceResult<PayableDto>.Failure("Nhà cung cấp không tồn tại.");

        var payable = new Payable
        {
            Id = Guid.NewGuid(),
            VendorId = dto.VendorId,
            VoucherNo = dto.VoucherNo,
            VoucherDate = dto.VoucherDate,
            PayableType = Enum.Parse<PayableType>(dto.PayableType),
            Amount = dto.Amount,
            PaidAmount = 0,
            DueDate = dto.DueDate,
            Description = dto.Description,
            VoucherId = dto.VoucherId,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _payableRepository.AddAsync(payable);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<PayableDto>.Success(MapToDto(payable, vendor.Name));
    }

    public async Task<ServiceResult<PayableDto>> UpdatePayableAsync(Guid id, UpdatePayableDto dto)
    {
        var payable = await _payableRepository.GetByIdAsync(id);
        if (payable == null)
            return ServiceResult<PayableDto>.Failure("Công nợ phải trả không tồn tại.");

        if (payable.PaidAmount > 0)
            return ServiceResult<PayableDto>.Failure("Công nợ đã thanh toán một phần, không thể cập nhật.");

        payable.Amount = dto.Amount;
        payable.DueDate = dto.DueDate;
        payable.Description = dto.Description;
        payable.ModifiedAt = DateTime.UtcNow;

        await _payableRepository.UpdateAsync(payable);
        await _unitOfWork.SaveChangesAsync();

        var vendor = await _vendorRepository.GetByIdAsync(payable.VendorId);
        return ServiceResult<PayableDto>.Success(MapToDto(payable, vendor?.Name ?? ""));
    }

    public async Task<ServiceResult<PayableDto>> GetPayableByIdAsync(Guid id)
    {
        var payable = await _payableRepository.GetByIdAsync(id);
        if (payable == null)
            return ServiceResult<PayableDto>.Failure("Công nợ phải trả không tồn tại.");

        var vendor = await _vendorRepository.GetByIdAsync(payable.VendorId);
        return ServiceResult<PayableDto>.Success(MapToDto(payable, vendor?.Name ?? ""));
    }

    public async Task<ServiceResult<List<PayableDto>>> GetPayablesByVendorAsync(Guid vendorId)
    {
        var payables = await _payableRepository.GetByVendorAsync(vendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        var dtos = payables.Select(p => MapToDto(p, vendor?.Name ?? "")).ToList();
        return ServiceResult<List<PayableDto>>.Success(dtos);
    }

    public async Task<ServiceResult<List<PayableDto>>> GetUnpaidPayablesAsync(int page, int pageSize)
    {
        var (payables, totalCount) = await _payableRepository.GetAllPagedAsync(page, pageSize);
        var unpaidPayables = payables.Where(p => p.RemainingAmount > 0).ToList();
        var dtos = new List<PayableDto>();
        
        foreach (var p in unpaidPayables)
        {
            var vendor = await _vendorRepository.GetByIdAsync(p.VendorId);
            dtos.Add(MapToDto(p, vendor?.Name ?? ""));
        }
        
        return ServiceResult<List<PayableDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PayableDto>> RecordPayablePaymentAsync(Guid payableId, RecordPaymentDto dto)
    {
        var payable = await _payableRepository.GetByIdAsync(payableId);
        if (payable == null)
            return ServiceResult<PayableDto>.Failure("Công nợ phải trả không tồn tại.");

        if (dto.Amount > payable.RemainingAmount)
            return ServiceResult<PayableDto>.Failure($"Số tiền thanh toán vượt quá số tiền còn lại ({payable.RemainingAmount:N0}).");

        var payment = new PayablePayment
        {
            Id = Guid.NewGuid(),
            PayableId = payableId,
            VoucherNo = dto.VoucherNo,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            ReferenceNo = dto.ReferenceNo,
            Notes = dto.Notes,
            VoucherId = dto.VoucherId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        payable.PaidAmount += dto.Amount;
        payable.IsReconciled = payable.RemainingAmount == 0;
        payable.ModifiedAt = DateTime.UtcNow;

        await _payablePaymentRepository.AddAsync(payment);
        await _payableRepository.UpdateAsync(payable);
        await _unitOfWork.SaveChangesAsync();

        var vendor = await _vendorRepository.GetByIdAsync(payable.VendorId);
        return ServiceResult<PayableDto>.Success(MapToDto(payable, vendor?.Name ?? ""));
    }

    public async Task<ServiceResult<AgingReportDto>> GetReceivableAgingReportAsync(int year, int month, Guid? customerId = null)
    {
        return await GenerateAgingReportAsync(year, month, "Receivable", customerId);
    }

    public async Task<ServiceResult<AgingReportDto>> GetPayableAgingReportAsync(int year, int month, Guid? vendorId = null)
    {
        return await GenerateAgingReportAsync(year, month, "Payable", vendorId);
    }

    public async Task<ServiceResult<AgingReportDto>> GenerateAgingReportAsync(int year, int month, string reportType, Guid? partnerId = null)
    {
        var reportDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var result = new AgingReportDto
        {
            Year = year,
            Month = month,
            ReportType = reportType
        };

        if (reportType == "Receivable")
        {
            result.CustomerId = partnerId;
            var receivables = partnerId.HasValue
                ? await _receivableRepository.GetByCustomerAsync(partnerId.Value)
                : (await _receivableRepository.GetAllPagedAsync(1, int.MaxValue)).Receivables;

            foreach (var r in receivables.Where(x => x.RemainingAmount > 0))
            {
                var customer = await _customerRepository.GetByIdAsync(r.CustomerId);
                var (period, overdueDays) = CalculateAgingPeriod(r.DueDate, reportDate);
                
                var detail = new AgingReportDetailDto
                {
                    Id = r.Id,
                    VoucherNo = r.VoucherNo,
                    VoucherDate = r.VoucherDate,
                    DueDate = r.DueDate,
                    Amount = r.Amount,
                    PaidAmount = r.PaidAmount,
                    RemainingAmount = r.RemainingAmount,
                    OverdueDays = overdueDays,
                    AgingPeriod = period
                };

                result.Details.Add(detail);

                switch (period)
                {
                    case "Current": result.CurrentAmount += r.RemainingAmount; break;
                    case "1-30 days": result.Due1To30Days += r.RemainingAmount; break;
                    case "31-60 days": result.Due31To60Days += r.RemainingAmount; break;
                    case "61-90 days": result.Due61To90Days += r.RemainingAmount; break;
                    case "Over 90 days": result.DueOver90Days += r.RemainingAmount; break;
                }

                result.PartnerName = customer?.Name ?? "";
            }
        }
        else
        {
            result.VendorId = partnerId;
            var payables = partnerId.HasValue
                ? await _payableRepository.GetByVendorAsync(partnerId.Value)
                : (await _payableRepository.GetAllPagedAsync(1, int.MaxValue)).Payables;

            foreach (var p in payables.Where(x => x.RemainingAmount > 0))
            {
                var vendor = await _vendorRepository.GetByIdAsync(p.VendorId);
                var (period, overdueDays) = CalculateAgingPeriod(p.DueDate, reportDate);
                
                var detail = new AgingReportDetailDto
                {
                    Id = p.Id,
                    VoucherNo = p.VoucherNo,
                    VoucherDate = p.VoucherDate,
                    DueDate = p.DueDate,
                    Amount = p.Amount,
                    PaidAmount = p.PaidAmount,
                    RemainingAmount = p.RemainingAmount,
                    OverdueDays = overdueDays,
                    AgingPeriod = period
                };

                result.Details.Add(detail);

                switch (period)
                {
                    case "Current": result.CurrentAmount += p.RemainingAmount; break;
                    case "1-30 days": result.Due1To30Days += p.RemainingAmount; break;
                    case "31-60 days": result.Due31To60Days += p.RemainingAmount; break;
                    case "61-90 days": result.Due61To90Days += p.RemainingAmount; break;
                    case "Over 90 days": result.DueOver90Days += p.RemainingAmount; break;
                }

                result.PartnerName = vendor?.Name ?? "";
            }
        }

        result.TotalAmount = result.CurrentAmount + result.Due1To30Days + result.Due31To60Days + result.Due61To90Days + result.DueOver90Days;

        return ServiceResult<AgingReportDto>.Success(result);
    }

    private static (string period, int overdueDays) CalculateAgingPeriod(DateTime? dueDate, DateTime reportDate)
    {
        if (!dueDate.HasValue || dueDate.Value > reportDate)
            return ("Current", 0);

        var overdueDays = (reportDate - dueDate.Value).Days;

        return overdueDays switch
        {
            <= 0 => ("Current", 0),
            <= 30 => ("1-30 days", overdueDays),
            <= 60 => ("31-60 days", overdueDays),
            <= 90 => ("61-90 days", overdueDays),
            _ => ("Over 90 days", overdueDays)
        };
    }

    private static ReceivableDto MapToDto(Receivable entity, string customerName)
    {
        return new ReceivableDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = customerName,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            ReceivableType = entity.ReceivableType.ToString(),
            Amount = entity.Amount,
            PaidAmount = entity.PaidAmount,
            RemainingAmount = entity.RemainingAmount,
            DueDate = entity.DueDate,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode,
            IsReconciled = entity.IsReconciled,
            CreatedDate = entity.CreatedAt
        };
    }

    private static PayableDto MapToDto(Payable entity, string vendorName)
    {
        return new PayableDto
        {
            Id = entity.Id,
            VendorId = entity.VendorId,
            VendorName = vendorName,
            VoucherNo = entity.VoucherNo,
            VoucherDate = entity.VoucherDate,
            PayableType = entity.PayableType.ToString(),
            Amount = entity.Amount,
            PaidAmount = entity.PaidAmount,
            RemainingAmount = entity.RemainingAmount,
            DueDate = entity.DueDate,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode,
            IsReconciled = entity.IsReconciled,
            CreatedDate = entity.CreatedAt
        };
    }
}