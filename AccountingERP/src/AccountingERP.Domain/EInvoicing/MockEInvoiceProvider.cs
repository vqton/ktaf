using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountingERP.Domain.EInvoicing
{
    /// <summary>
    /// Adapter: Mock E-Invoice Provider
    /// For testing and development without real API
    /// </summary>
    public class MockEInvoiceProvider : IEInvoiceProvider
    {
        private readonly Dictionary<string, MockEInvoiceRecord> _invoices = new();
        private readonly List<EInvoiceLogEntry> _logs = new();
        private int _invoiceCounter = 0;

        public async Task<EInvoiceResult> SendInvoiceAsync(EInvoiceRequest request)
        {
            await Task.Delay(100); // Simulate API delay

            var externalId = $"EINV-{DateTime.Now:yyyyMMdd}-{_invoiceCounter++:D5}";
            var verificationCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var record = new MockEInvoiceRecord
            {
                ExternalId = externalId,
                InvoiceNumber = request.InvoiceNumber,
                Status = EInvoiceStatus.Sent,
                VerificationCode = verificationCode,
                RequestPayload = System.Text.Json.JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow
            };

            _invoices[externalId] = record;

            _logs.Add(new EInvoiceLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "SendInvoice",
                ExternalInvoiceId = externalId,
                RequestPayload = record.RequestPayload,
                ResponsePayload = $"{{\"status\": \"sent\", \"id\": \"{externalId}\"}}"
            });

            // Auto-confirm after 2 seconds (simulate async processing)
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                if (_invoices.ContainsKey(externalId))
                {
                    _invoices[externalId].Status = EInvoiceStatus.Confirmed;
                    _logs.Add(new EInvoiceLogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Action = "AutoConfirm",
                        ExternalInvoiceId = externalId,
                        ResponsePayload = "{\"status\": \"confirmed\"}"
                    });
                }
            });

            return new EInvoiceResult
            {
                Success = true,
                ExternalInvoiceId = externalId,
                VerificationCode = verificationCode,
                Message = "Hóa đơn đã được gửi thành công",
                ResponsePayload = $"{{\"externalId\": \"{externalId}\", \"verificationCode\": \"{verificationCode}\"}}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        public async Task<EInvoiceStatus> GetStatusAsync(string externalInvoiceId)
        {
            await Task.Delay(50); // Simulate API delay

            _logs.Add(new EInvoiceLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "GetStatus",
                ExternalInvoiceId = externalInvoiceId,
                ResponsePayload = $"{{\"status\": \"{(_invoices.ContainsKey(externalInvoiceId) ? _invoices[externalInvoiceId].Status : EInvoiceStatus.Rejected)}\"}}"
            });

            if (_invoices.TryGetValue(externalInvoiceId, out var record))
            {
                return record.Status;
            }

            return EInvoiceStatus.Rejected;
        }

        public async Task<EInvoiceResult> CancelInvoiceAsync(string externalInvoiceId, string reason)
        {
            await Task.Delay(100); // Simulate API delay

            _logs.Add(new EInvoiceLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "CancelInvoice",
                ExternalInvoiceId = externalInvoiceId,
                RequestPayload = $"{{\"reason\": \"{reason}\"}}"
            });

            if (_invoices.TryGetValue(externalInvoiceId, out var record))
            {
                if (record.Status == EInvoiceStatus.Cancelled)
                {
                    return new EInvoiceResult
                    {
                        Success = false,
                        ExternalInvoiceId = externalInvoiceId,
                        Message = "Hóa đơn đã bị hủy trước đó",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                record.Status = EInvoiceStatus.Cancelled;
                record.CancelledAt = DateTime.UtcNow;
                record.CancellationReason = reason;

                return new EInvoiceResult
                {
                    Success = true,
                    ExternalInvoiceId = externalInvoiceId,
                    Message = "Hóa đơn đã được hủy thành công",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            return new EInvoiceResult
            {
                Success = false,
                ExternalInvoiceId = externalInvoiceId,
                Message = "Không tìm thấy hóa đơn",
                ProcessedAt = DateTime.UtcNow
            };
        }

        public async Task<byte[]> DownloadPdfAsync(string externalInvoiceId)
        {
            await Task.Delay(200); // Simulate API delay

            _logs.Add(new EInvoiceLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "DownloadPdf",
                ExternalInvoiceId = externalInvoiceId
            });

            // Return a mock PDF (just some bytes)
            var mockPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
            return mockPdf;
        }

        public async Task<bool> HealthCheckAsync()
        {
            await Task.Delay(50);
            return true; // Mock provider is always healthy
        }

        /// <summary>
        /// Get all logs for audit purposes
        /// </summary>
        public IReadOnlyCollection<EInvoiceLogEntry> GetLogs()
        {
            return _logs.AsReadOnly();
        }

        /// <summary>
        /// Get invoice details (for testing)
        /// </summary>
        public MockEInvoiceRecord GetInvoice(string externalInvoiceId)
        {
            return _invoices.TryGetValue(externalInvoiceId, out var record) ? record : null;
        }
    }

    /// <summary>
    /// Internal record for mock storage
    /// </summary>
    public class MockEInvoiceRecord
    {
        public string ExternalId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public EInvoiceStatus Status { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
        public string RequestPayload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Log entry for all API interactions
    /// </summary>
    public class EInvoiceLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ExternalInvoiceId { get; set; } = string.Empty;
        public string RequestPayload { get; set; } = string.Empty;
        public string ResponsePayload { get; set; } = string.Empty;
    }
}
