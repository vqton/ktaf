using System;
using System.Threading.Tasks;

namespace AccountingERP.Domain.EInvoicing
{
    /// <summary>
    /// Port: E-Invoice Provider Interface
    /// Adapter pattern for e-invoice integration
    /// TT78/2021 Compliance
    /// </summary>
    public interface IEInvoiceProvider
    {
        /// <summary>
        /// Gửi hóa đơn đến hệ thống e-invoice
        /// </summary>
        Task<EInvoiceResult> SendInvoiceAsync(EInvoiceRequest request);
        
        /// <summary>
        /// Kiểm tra trạng thái hóa đơn
        /// </summary>
        Task<EInvoiceStatus> GetStatusAsync(string externalInvoiceId);
        
        /// <summary>
        /// Hủy hóa đơn
        /// </summary>
        Task<EInvoiceResult> CancelInvoiceAsync(string externalInvoiceId, string reason);
        
        /// <summary>
        /// Tải file PDF hóa đơn
        /// </summary>
        Task<byte[]> DownloadPdfAsync(string externalInvoiceId);
        
        /// <summary>
        /// Kiểm tra kết nối
        /// </summary>
        Task<bool> HealthCheckAsync();
    }

    /// <summary>
    /// Yêu cầu gửi hóa đơn điện tử
    /// </summary>
    public class EInvoiceRequest
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceSeries { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string BuyerTaxCode { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string SellerTaxCode { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public int VatRate { get; set; }
        public string Currency { get; set; } = "VND";
        public List<EInvoiceLineItem> Items { get; set; } = new();
    }

    public class EInvoiceLineItem
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Kết quả xử lý hóa đơn điện tử
    /// </summary>
    public class EInvoiceResult
    {
        public bool Success { get; set; }
        public string ExternalInvoiceId { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ResponsePayload { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Trạng thái hóa đơn điện tử
    /// </summary>
    public enum EInvoiceStatus
    {
        Draft = 0,
        Sent = 1,
        Confirmed = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
