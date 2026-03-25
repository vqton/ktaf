using AMS.Domain.Enums;

namespace AMS.Application.DTOs;

public class FiscalPeriodDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public FiscalPeriodStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

public class CreateFiscalPeriodDto
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class UpdateFiscalPeriodStatusDto
{
    public Guid Id { get; set; }
    public FiscalPeriodStatus NewStatus { get; set; }
    public string? ClosedBy { get; set; }
}
