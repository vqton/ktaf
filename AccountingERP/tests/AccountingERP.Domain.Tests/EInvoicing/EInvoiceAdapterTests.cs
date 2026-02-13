using System;
using System.Linq;
using System.Threading.Tasks;
using AccountingERP.Domain.EInvoicing;
using Xunit;

namespace AccountingERP.Domain.Tests.EInvoicing
{
    /// <summary>
    /// Test Suite cho Feature 4: E-Invoice Adapter
    /// TT78/2021 Compliance
    /// </summary>
    public class EInvoiceAdapterTests
    {
        private EInvoiceRequest CreateSampleRequest()
        {
            return new EInvoiceRequest
            {
                InvoiceNumber = "INV-2024-00001",
                InvoiceSeries = "1C25TAA",
                IssueDate = new DateTime(2024, 1, 15),
                BuyerTaxCode = "0123456789",
                BuyerName = "Công ty TNHH ABC",
                SellerTaxCode = "9876543210",
                SellerName = "Công ty XYZ",
                TotalAmount = 11000000,
                VatAmount = 1000000,
                VatRate = 10,
                Currency = "VND",
                Items = new System.Collections.Generic.List<EInvoiceLineItem>
                {
                    new EInvoiceLineItem
                    {
                        ProductCode = "P001",
                        ProductName = "Sản phẩm A",
                        Unit = "cái",
                        Quantity = 10,
                        UnitPrice = 1000000,
                        TotalAmount = 10000000
                    }
                }
            };
        }

        #region SendInvoice Tests

        [Fact]
        public async Task SendInvoice_ShouldReturnSuccess()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();

            // Act
            var result = await provider.SendInvoiceAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty(result.ExternalInvoiceId);
            Assert.NotEmpty(result.VerificationCode);
            Assert.Contains("thành công", result.Message);
        }

        [Fact]
        public async Task SendInvoice_ShouldGenerateExternalId()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();

            // Act
            var result = await provider.SendInvoiceAsync(request);

            // Assert
            Assert.StartsWith("EINV-", result.ExternalInvoiceId);
        }

        [Fact]
        public async Task SendInvoice_ShouldStoreInvoice()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();

            // Act
            var result = await provider.SendInvoiceAsync(request);
            var stored = provider.GetInvoice(result.ExternalInvoiceId);

            // Assert
            Assert.NotNull(stored);
            Assert.Equal(request.InvoiceNumber, stored.InvoiceNumber);
            Assert.Equal(EInvoiceStatus.Sent, stored.Status);
        }

        [Fact]
        public async Task SendInvoice_ShouldLogTransaction()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();

            // Act
            var result = await provider.SendInvoiceAsync(request);
            var logs = provider.GetLogs();

            // Assert
            Assert.Contains(logs, l => l.Action == "SendInvoice" && l.ExternalInvoiceId == result.ExternalInvoiceId);
        }

        #endregion

        #region GetStatus Tests

        [Fact]
        public async Task GetStatus_ExistingInvoice_ShouldReturnStatus()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            var status = await provider.GetStatusAsync(sendResult.ExternalInvoiceId);

            // Assert
            Assert.True(status == EInvoiceStatus.Sent || status == EInvoiceStatus.Confirmed);
        }

        [Fact]
        public async Task GetStatus_NonExistingInvoice_ShouldReturnRejected()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();

            // Act
            var status = await provider.GetStatusAsync("NON-EXISTENT");

            // Assert
            Assert.Equal(EInvoiceStatus.Rejected, status);
        }

        [Fact]
        public async Task GetStatus_ShouldLogTransaction()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            await provider.GetStatusAsync(sendResult.ExternalInvoiceId);
            var logs = provider.GetLogs();

            // Assert
            Assert.Contains(logs, l => l.Action == "GetStatus" && l.ExternalInvoiceId == sendResult.ExternalInvoiceId);
        }

        #endregion

        #region CancelInvoice Tests

        [Fact]
        public async Task CancelInvoice_ExistingInvoice_ShouldSucceed()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            var cancelResult = await provider.CancelInvoiceAsync(sendResult.ExternalInvoiceId, "Sai sót thông tin");

            // Assert
            Assert.True(cancelResult.Success);
            Assert.Contains("hủy", cancelResult.Message);
        }

        [Fact]
        public async Task CancelInvoice_ShouldChangeStatus()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            await provider.CancelInvoiceAsync(sendResult.ExternalInvoiceId, "Sai sót");
            var status = await provider.GetStatusAsync(sendResult.ExternalInvoiceId);

            // Assert
            Assert.Equal(EInvoiceStatus.Cancelled, status);
        }

        [Fact]
        public async Task CancelInvoice_AlreadyCancelled_ShouldFail()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);
            await provider.CancelInvoiceAsync(sendResult.ExternalInvoiceId, "Lần 1");

            // Act
            var cancelResult = await provider.CancelInvoiceAsync(sendResult.ExternalInvoiceId, "Lần 2");

            // Assert
            Assert.False(cancelResult.Success);
            Assert.Contains("đã bị hủy", cancelResult.Message);
        }

        [Fact]
        public async Task CancelInvoice_NonExisting_ShouldFail()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();

            // Act
            var cancelResult = await provider.CancelInvoiceAsync("NON-EXISTENT", "Test");

            // Assert
            Assert.False(cancelResult.Success);
            Assert.Contains("Không tìm thấy", cancelResult.Message);
        }

        [Fact]
        public async Task CancelInvoice_ShouldLogTransaction()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            await provider.CancelInvoiceAsync(sendResult.ExternalInvoiceId, "Sai sót");
            var logs = provider.GetLogs();

            // Assert
            Assert.Contains(logs, l => l.Action == "CancelInvoice" && l.ExternalInvoiceId == sendResult.ExternalInvoiceId);
        }

        #endregion

        #region DownloadPdf Tests

        [Fact]
        public async Task DownloadPdf_ShouldReturnPdfData()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            var pdfData = await provider.DownloadPdfAsync(sendResult.ExternalInvoiceId);

            // Assert
            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 0);
        }

        [Fact]
        public async Task DownloadPdf_ShouldReturnPdfHeader()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            var pdfData = await provider.DownloadPdfAsync(sendResult.ExternalInvoiceId);

            // Assert - PDF files start with %PDF
            Assert.Equal(0x25, pdfData[0]); // '%'
            Assert.Equal(0x50, pdfData[1]); // 'P'
            Assert.Equal(0x44, pdfData[2]); // 'D'
            Assert.Equal(0x46, pdfData[3]); // 'F'
        }

        [Fact]
        public async Task DownloadPdf_ShouldLogTransaction()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var sendResult = await provider.SendInvoiceAsync(request);

            // Act
            await provider.DownloadPdfAsync(sendResult.ExternalInvoiceId);
            var logs = provider.GetLogs();

            // Assert
            Assert.Contains(logs, l => l.Action == "DownloadPdf" && l.ExternalInvoiceId == sendResult.ExternalInvoiceId);
        }

        #endregion

        #region HealthCheck Tests

        [Fact]
        public async Task HealthCheck_ShouldReturnTrue()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();

            // Act
            var isHealthy = await provider.HealthCheckAsync();

            // Assert
            Assert.True(isHealthy);
        }

        #endregion

        #region Auto-Confirm Tests

        [Fact]
        public async Task SendInvoice_ShouldAutoConfirmAfterDelay()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();

            // Act
            var sendResult = await provider.SendInvoiceAsync(request);
            
            // Wait for auto-confirm
            await Task.Delay(2500);
            
            var status = await provider.GetStatusAsync(sendResult.ExternalInvoiceId);

            // Assert
            Assert.Equal(EInvoiceStatus.Confirmed, status);
        }

        #endregion

        #region Log Tests

        [Fact]
        public async Task GetLogs_ShouldReturnAllTransactions()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var result = await provider.SendInvoiceAsync(request);
            await provider.GetStatusAsync(result.ExternalInvoiceId);

            // Act
            var logs = provider.GetLogs();

            // Assert
            Assert.True(logs.Count >= 2);
            Assert.Contains(logs, l => l.Action == "SendInvoice");
            Assert.Contains(logs, l => l.Action == "GetStatus");
        }

        [Fact]
        public async Task Logs_ShouldContainTimestamps()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            await provider.SendInvoiceAsync(request);

            // Act
            var logs = provider.GetLogs();

            // Assert
            var log = logs.First();
            Assert.True(log.Timestamp > DateTime.MinValue);
            Assert.True(log.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public async Task Logs_ShouldContainPayloads()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            var result = await provider.SendInvoiceAsync(request);

            // Act
            var logs = provider.GetLogs();

            // Assert
            var log = logs.First(l => l.Action == "SendInvoice");
            Assert.NotEmpty(log.RequestPayload);
            Assert.NotEmpty(log.ResponsePayload);
            Assert.Contains(result.ExternalInvoiceId, log.ResponsePayload);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task MultipleInvoices_ShouldHaveUniqueExternalIds()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request1 = CreateSampleRequest();
            var request2 = CreateSampleRequest();

            // Act
            var result1 = await provider.SendInvoiceAsync(request1);
            var result2 = await provider.SendInvoiceAsync(request2);

            // Assert
            Assert.NotEqual(result1.ExternalInvoiceId, result2.ExternalInvoiceId);
        }

        [Fact]
        public async Task SendInvoice_WithDifferentNumbers_ShouldStoreCorrectly()
        {
            // Arrange
            var provider = new MockEInvoiceProvider();
            var request = CreateSampleRequest();
            request.InvoiceNumber = "CUSTOM-001";

            // Act
            var result = await provider.SendInvoiceAsync(request);
            var stored = provider.GetInvoice(result.ExternalInvoiceId);

            // Assert
            Assert.Equal("CUSTOM-001", stored.InvoiceNumber);
        }

        #endregion
    }
}
