using AMS.Domain.Enums;

namespace AMS.Domain.Entities.DM;

public enum ReceivableType
{
    TradeReceivable,
    AdvancePayment,
    OtherReceivable
}

public enum PayableType
{
    TradePayable,
    AdvancePayment,
    OtherPayable
}

public enum AgingPeriod
{
    Current,
    Due1To30Days,
    Due31To60Days,
    Due61To90Days,
    DueOver90Days
}

public class Receivable : BaseAuditEntity
{
    public Guid CustomerId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public ReceivableType ReceivableType { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
    public bool IsReconciled { get; set; }
    public Customer? Customer { get; set; }
    public Voucher? Voucher { get; set; }
}

public class Payable : BaseAuditEntity
{
    public Guid VendorId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public PayableType PayableType { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
    public bool IsReconciled { get; set; }
    public Vendor? Vendor { get; set; }
    public Voucher? Voucher { get; set; }
}

public class ReceivablePayment : BaseAuditEntity
{
    public Guid ReceivableId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public Receivable? Receivable { get; set; }
    public Voucher? Voucher { get; set; }
}

public class PayablePayment : BaseAuditEntity
{
    public Guid PayableId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public Payable? Payable { get; set; }
    public Voucher? Voucher { get; set; }
}

public class AgingReport : BaseAuditEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VendorId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal Due1To30Days { get; set; }
    public decimal Due31To60Days { get; set; }
    public decimal Due61To90Days { get; set; }
    public decimal DueOver90Days { get; set; }
    public decimal TotalAmount { get; set; }
}