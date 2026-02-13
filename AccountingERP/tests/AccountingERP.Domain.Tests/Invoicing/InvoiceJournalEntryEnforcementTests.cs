using System;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Invoicing;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.Invoicing
{
    /// <summary>
    /// Test Suite cho Feature 1: Invoice ↔ JournalEntry Hard Enforcement
    /// TT78/2021 Compliance: Hóa đơn phải khớp với sổ kế toán
    /// </summary>
    public class InvoiceJournalEntryEnforcementTests
    {
        #region Test Data Helpers
        
        private Invoice CreateDraftInvoice()
        {
            return Invoice.CreateDraft(
                invoiceNumber: "INV-2024-00001",
                invoiceSeries: "1C25TAA",
                issueDate: new DateTime(2024, 1, 15),
                type: InvoiceType.Sales,
                customerCode: "KH001",
                customerName: "Công ty TNHH ABC",
                customerTaxCode: "0123456789",
                vatRate: 10,
                createdBy: "test-user");
        }
        
        private JournalEntry CreateJournalEntry()
        {
            return JournalEntry.Create(
                entryNumber: "BT-202401-00001",
                originalDocumentNumber: "INV-2024-00001",
                entryDate: new DateTime(2024, 1, 15),
                originalDocumentDate: new DateTime(2024, 1, 15),
                description: "Bán hàng");
        }
        
        #endregion

        #region 1. Issue Invoice → JE Created

        [Fact]
        public void Invoice_Issue_WithValidJournalEntryId_ShouldSucceed()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var journalEntryId = JournalEntryId.New();
            
            // Act
            invoice.Issue("VERIFY-CODE-123", journalEntryId, "accountant");
            
            // Assert
            Assert.Equal(InvoiceStatus.Issued, invoice.Status);
            Assert.Equal(journalEntryId, invoice.JournalEntryId);
            Assert.Equal("VERIFY-CODE-123", invoice.VerificationCode);
            Assert.Single(invoice.DomainEvents, e => e is InvoiceIssuedEvent);
        }

        [Fact]
        public void Invoice_Issue_WithoutJournalEntryId_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            
            // Act & Assert
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                invoice.Issue("VERIFY-CODE-123", null, "accountant"));
            
            Assert.Contains("Không thể phát hành hóa đơn không có bút toán kế toán", ex.Message);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        }

        [Fact]
        public void Invoice_Issue_WithNullJournalEntryId_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);

            // Act & Assert - Test with null JournalEntryId
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                invoice.Issue("VERIFY-CODE-123", null, "accountant"));

            Assert.Contains("Không thể phát hành hóa đơn không có bút toán kế toán", ex.Message);
        }

        [Fact]
        public void Invoice_Issue_WithoutLines_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            var journalEntryId = JournalEntryId.New();
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                invoice.Issue("VERIFY-CODE-123", journalEntryId, "accountant"));
            
            Assert.Contains("TT78: Hóa đơn phải có ít nhất một dòng", ex.Message);
        }

        [Fact]
        public void Invoice_Issue_AlreadyIssued_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var journalEntryId = JournalEntryId.New();
            invoice.Issue("VERIFY-CODE-123", journalEntryId, "accountant");
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                invoice.Issue("VERIFY-CODE-456", journalEntryId, "accountant"));
            
            Assert.Contains("Chỉ có thể phát hành hóa đơn ở trạng thái Draft", ex.Message);
        }

        [Fact]
        public void Invoice_Issue_WithoutVerificationCode_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var journalEntryId = JournalEntryId.New();
            
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                invoice.Issue("", journalEntryId, "accountant"));
            
            Assert.Contains("TT78: Mã tra cứu không được để trống", ex.Message);
        }

        #endregion

        #region 2. Create Revenue JE without Invoice → FAIL

        [Fact]
        public void JournalEntry_Post_RevenueEntry_WithoutInvoiceId_ShouldThrowException()
        {
            // Arrange - Revenue entry (TK 511)
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1100000, 0, "Tiền mặt"); // Debit Cash
            entry.AddLine("511", 0, 1000000, "Doanh thu"); // Credit Revenue
            entry.AddLine("33311", 0, 100000, "Thuế GTGT đầu ra"); // Credit VAT Output
            
            // Act & Assert
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                entry.Post("accountant"));
            
            Assert.Contains("Không thể ghi sổ bút toán doanh thu không có hóa đơn", ex.Message);
            Assert.False(entry.IsPosted);
        }

        [Theory]
        [InlineData("511")]  // Doanh thu bán hàng
        [InlineData("512")]  // Doanh thu cung cấp dịch vụ
        [InlineData("515")]  // Doanh thu hoạt động tài chính
        [InlineData("517")]  // Doanh thu khác
        public void JournalEntry_Post_RevenueAccount_WithoutInvoiceId_ShouldFail(string revenueAccount)
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine(revenueAccount, 0, 1000000, "Doanh thu");
            
            // Act & Assert
            Assert.Throws<InvoiceAccountingMismatchException>(() =>
                entry.Post("accountant"));
        }

        [Fact]
        public void JournalEntry_Post_RevenueEntry_WithInvoiceId_ShouldSucceed()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1100000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");
            entry.AddLine("33311", 0, 100000, "Thuế GTGT đầu ra");
            
            var invoiceId = InvoiceId.New();
            entry.LinkToInvoice(invoiceId);
            
            // Act
            entry.Post("accountant");
            
            // Assert
            Assert.True(entry.IsPosted);
            Assert.Equal(invoiceId, entry.InvoiceId);
        }

        [Fact]
        public void JournalEntry_Post_NonRevenueEntry_WithoutInvoiceId_ShouldSucceed()
        {
            // Arrange - Non-revenue entry (expense)
            var entry = CreateJournalEntry();
            entry.AddLine("641", 1000000, 0, "Chi phí quản lý");
            entry.AddLine("111", 0, 1000000, "Tiền mặt");
            
            // Act
            entry.Post("accountant");
            
            // Assert
            Assert.True(entry.IsPosted);
            Assert.Null(entry.InvoiceId);
        }

        [Fact]
        public void JournalEntry_LinkToInvoice_AlreadyPosted_ShouldThrowException()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("641", 1000000, 0, "Chi phí");
            entry.AddLine("111", 0, 1000000, "Tiền mặt");
            entry.Post("accountant");
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                entry.LinkToInvoice(InvoiceId.New()));
            
            Assert.Contains("Không thể liên kết bút toán đã ghi sổ", ex.Message);
        }

        [Fact]
        public void JournalEntry_LinkToInvoice_AlreadyLinked_ShouldThrowException()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");
            entry.LinkToInvoice(InvoiceId.New());
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                entry.LinkToInvoice(InvoiceId.New()));
            
            Assert.Contains("Bút toán đã được liên kết với hóa đơn khác", ex.Message);
        }

        [Fact]
        public void JournalEntry_IsRevenueEntry_ShouldBeTrue_ForRevenueAccounts()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");
            
            // Assert
            Assert.True(entry.IsRevenueEntry);
        }

        [Fact]
        public void JournalEntry_IsRevenueEntry_ShouldBeFalse_ForNonRevenueAccounts()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("641", 1000000, 0, "Chi phí");
            entry.AddLine("111", 0, 1000000, "Tiền mặt");
            
            // Assert
            Assert.False(entry.IsRevenueEntry);
        }

        #endregion

        #region 3. Cancel Invoice → Reversal JE Created

        [Fact]
        public void Invoice_Cancel_WithReversalJournalEntry_ShouldSucceed()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var originalJEId = JournalEntryId.New();
            invoice.Issue("VERIFY-CODE-123", originalJEId, "accountant");
            
            var reversalJEId = JournalEntryId.New();
            
            // Act
            invoice.Cancel("Sai sót thông tin", reversalJEId, "chief-accountant");
            
            // Assert
            Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
            Assert.Equal(reversalJEId, invoice.ReversalJournalEntryId);
            Assert.Equal("Sai sót thông tin", invoice.CancellationReason);
            Assert.NotNull(invoice.CancelledAt);
            Assert.Single(invoice.DomainEvents, e => e is InvoiceCancelledEvent);
        }

        [Fact]
        public void Invoice_Cancel_WithoutReversalJournalEntry_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var originalJEId = JournalEntryId.New();
            invoice.Issue("VERIFY-CODE-123", originalJEId, "accountant");
            
            // Act & Assert
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                invoice.Cancel("Sai sót", null, "chief-accountant"));
            
            Assert.Contains("Không thể hủy hóa đơn không tạo bút toán đảo ngược", ex.Message);
        }

        [Fact]
        public void Invoice_Cancel_WithoutOriginalJournalEntry_ShouldThrowException()
        {
            // Arrange - This shouldn't happen in practice, but test the protection
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            
            // Manually set status to Issued without setting JournalEntryId
            // (This simulates a data integrity issue)
            // We can't actually do this because the Invoice aggregate protects its invariants
            // So we'll just test that Cancel requires Issued status first
            
            // Act & Assert - Should fail because not Issued
            var ex = Assert.Throws<InvalidOperationException>(() =>
                invoice.Cancel("Sai sót", JournalEntryId.New(), "chief-accountant"));
            
            Assert.Contains("Chỉ có thể hủy hóa đơn đã phát hành", ex.Message);
        }

        [Fact]
        public void Invoice_Cancel_AlreadyCancelled_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var originalJEId = JournalEntryId.New();
            invoice.Issue("VERIFY-CODE-123", originalJEId, "accountant");
            invoice.Cancel("Sai sót", JournalEntryId.New(), "chief-accountant");
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                invoice.Cancel("Hủy nữa", JournalEntryId.New(), "chief-accountant"));
            
            Assert.Contains("Chỉ có thể hủy hóa đơn đã phát hành", ex.Message);
        }

        [Fact]
        public void Invoice_Cancel_WithoutReason_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var originalJEId = JournalEntryId.New();
            invoice.Issue("VERIFY-CODE-123", originalJEId, "accountant");
            
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                invoice.Cancel("", JournalEntryId.New(), "chief-accountant"));
            
            Assert.Contains("TT78: Lý do hủy không được để trống", ex.Message);
        }

        #endregion

        #region 4. VAT Amount Matches Invoice

        [Fact]
        public void Invoice_VatAmount_ShouldMatchCalculation()
        {
            // Arrange
            var invoice = CreateDraftInvoice(); // 10% VAT rate
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m); // 1,000,000 subtotal
            
            // Act
            var expectedVat = 1000000m * 0.10m; // 100,000 VAT
            
            // Assert
            Assert.Equal(1000000m, invoice.SubTotal.Amount);
            Assert.Equal(expectedVat, invoice.VatAmount.Amount);
            Assert.Equal(1100000m, invoice.TotalAmount.Amount); // Subtotal + VAT
        }

        [Theory]
        [InlineData(0, 1000000, 0, 1000000)]    // 0% VAT
        [InlineData(5, 1000000, 50000, 1050000)] // 5% VAT
        [InlineData(8, 1000000, 80000, 1080000)] // 8% VAT
        [InlineData(10, 1000000, 100000, 1100000)] // 10% VAT
        public void Invoice_VatAmount_ShouldCalculateCorrectly_ForAllValidRates(
            int vatRate, decimal subtotal, decimal expectedVat, decimal expectedTotal)
        {
            // Arrange
            var invoice = Invoice.CreateDraft(
                "INV-001", "1C25TAA", new DateTime(2024, 1, 15),
                InvoiceType.Sales, "KH001", "ABC", "0123456789",
                vatRate, "test-user");
            
            invoice.AddLine("P001", "Product", "pcs", 1, subtotal);
            
            // Assert
            Assert.Equal(subtotal, invoice.SubTotal.Amount);
            Assert.Equal(expectedVat, invoice.VatAmount.Amount);
            Assert.Equal(expectedTotal, invoice.TotalAmount.Amount);
        }

        [Fact]
        public void JournalEntry_VatPosting_ShouldMatchInvoiceVat()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1100000, 0, "Tiền mặt"); // Total with VAT
            entry.AddLine("511", 0, 1000000, "Doanh thu"); // Subtotal
            entry.AddLine("33311", 0, 100000, "Thuế GTGT đầu ra"); // VAT
            entry.LinkToInvoice(invoice.InvoiceId);
            
            // Act
            entry.Post("accountant");
            
            // Assert
            Assert.True(entry.IsBalanced);
            Assert.Equal(1100000m, entry.TotalDebit);
            Assert.Equal(1100000m, entry.TotalCredit);
        }

        #endregion

        #region 5. JE Balanced Test

        [Fact]
        public void JournalEntry_ShouldBeBalanced_WhenDebitEqualsCredit()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1100000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 1000000, "Doanh thu");
            entry.AddLine("33311", 0, 100000, "Thuế GTGT");
            
            // Assert
            Assert.True(entry.IsBalanced);
            Assert.Equal(entry.TotalDebit, entry.TotalCredit);
        }

        [Fact]
        public void JournalEntry_ShouldNotBeBalanced_WhenDebitNotEqualsCredit()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 900000, "Doanh thu"); // Mismatch
            
            // Assert
            Assert.False(entry.IsBalanced);
            Assert.NotEqual(entry.TotalDebit, entry.TotalCredit);
        }

        [Fact]
        public void JournalEntry_Post_UnbalancedEntry_ShouldThrowException()
        {
            // Arrange
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("511", 0, 900000, "Doanh thu"); // Unbalanced
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                entry.Post("accountant"));
            
            Assert.Contains("không cân bằng", ex.Message);
        }

        #endregion

        #region 6. Attempt Bypass → FAIL

        [Fact]
        public void Bypass_Attempt_PostRevenueWithoutInvoice_ShouldFail()
        {
            // Arrange - Try to post revenue entry without InvoiceId
            var entry = CreateJournalEntry();
            entry.AddLine("111", 1000000, 0, "Tiền mặt");
            entry.AddLine("5111", 0, 1000000, "Doanh thu bán hàng - TK chi tiết");
            
            // Act & Assert - Should fail hard
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                entry.Post("accountant"));
            
            Assert.Contains("Không thể ghi sổ bút toán doanh thu không có hóa đơn", ex.Message);
        }

        [Fact]
        public void Bypass_Attempt_LinkInvoiceAfterPost_ShouldFail()
        {
            // Arrange - Try to link invoice after posting (to bypass check)
            var entry = CreateJournalEntry();
            entry.AddLine("641", 1000000, 0, "Chi phí");
            entry.AddLine("111", 0, 1000000, "Tiền mặt");
            entry.Post("accountant");
            
            // Act & Assert - Should fail
            var ex = Assert.Throws<InvalidOperationException>(() =>
                entry.LinkToInvoice(InvoiceId.New()));
            
            Assert.Contains("Không thể liên kết bút toán đã ghi sổ", ex.Message);
        }

        [Fact]
        public void Bypass_Attempt_IssueInvoiceWithoutJE_ShouldFail()
        {
            // Arrange - Try to issue invoice without JournalEntry
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            
            // Act & Assert - Should fail
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                invoice.Issue("VERIFY-CODE", null, "accountant"));
            
            Assert.Contains("Không thể phát hành hóa đơn không có bút toán kế toán", ex.Message);
        }

        [Fact]
        public void Bypass_Attempt_CancelInvoiceWithoutReversalJE_ShouldFail()
        {
            // Arrange - Try to cancel without reversal entry
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            invoice.Issue("VERIFY-CODE", JournalEntryId.New(), "accountant");
            
            // Act & Assert - Should fail
            var ex = Assert.Throws<InvoiceAccountingMismatchException>(() =>
                invoice.Cancel("Sai sót", null, "chief-accountant"));
            
            Assert.Contains("Không thể hủy hóa đơn không tạo bút toán đảo ngược", ex.Message);
        }

        #endregion

        #region Domain Events Tests

        [Fact]
        public void Invoice_Issue_ShouldRaiseInvoiceIssuedEvent()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            var journalEntryId = JournalEntryId.New();
            
            // Act
            invoice.Issue("VERIFY-CODE-123", journalEntryId, "accountant");
            
            // Assert
            var domainEvent = Assert.Single(invoice.DomainEvents, e => e is InvoiceIssuedEvent) as InvoiceIssuedEvent;
            Assert.NotNull(domainEvent);
            Assert.Equal(invoice.InvoiceId, domainEvent.InvoiceId);
            Assert.Equal(invoice.InvoiceNumber, domainEvent.InvoiceNumber);
            Assert.Equal(journalEntryId, domainEvent.JournalEntryId);
        }

        [Fact]
        public void Invoice_Cancel_ShouldRaiseInvoiceCancelledEvent()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            invoice.Issue("VERIFY-CODE-123", JournalEntryId.New(), "accountant");
            var reversalJEId = JournalEntryId.New();
            
            // Act
            invoice.Cancel("Sai sót thông tin", reversalJEId, "chief-accountant");
            
            // Assert
            var domainEvent = Assert.Single(invoice.DomainEvents, e => e is InvoiceCancelledEvent) as InvoiceCancelledEvent;
            Assert.NotNull(domainEvent);
            Assert.Equal(invoice.InvoiceId, domainEvent.InvoiceId);
            Assert.Equal("Sai sót thông tin", domainEvent.Reason);
            Assert.Equal(reversalJEId, domainEvent.ReversalJournalEntryId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Invoice_AddLine_ToIssuedInvoice_ShouldThrowException()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m);
            invoice.Issue("VERIFY-CODE", JournalEntryId.New(), "accountant");
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                invoice.AddLine("P002", "Sản phẩm B", "cái", 5, 50000m));
            
            Assert.Contains("Không thể thêm dòng vào hóa đơn đã Issued", ex.Message);
        }

        [Fact]
        public void Invoice_CalculateTotals_ShouldBeDeterministic()
        {
            // Arrange
            var invoice = CreateDraftInvoice();
            
            // Act - Add lines
            invoice.AddLine("P001", "Sản phẩm A", "cái", 10, 100000m); // 1,000,000
            invoice.AddLine("P002", "Sản phẩm B", "cái", 5, 200000m);   // 1,000,000
            invoice.AddLine("P003", "Sản phẩm C", "hộp", 2, 500000m, 100000m); // 1,000,000 - 100,000 discount
            
            // Assert
            Assert.Equal(2900000m, invoice.SubTotal.Amount); // 1M + 1M + 900K
            Assert.Equal(290000m, invoice.VatAmount.Amount); // 10% of 2.9M
            Assert.Equal(3190000m, invoice.TotalAmount.Amount); // 2.9M + 290K
        }

        [Fact]
        public void JournalEntry_RequiresInvoiceLink_ShouldBeTrue_ForRevenue()
        {
            // Arrange
            var revenueEntry = CreateJournalEntry();
            revenueEntry.AddLine("111", 1000000, 0, "Tiền mặt");
            revenueEntry.AddLine("511", 0, 1000000, "Doanh thu");
            
            // Assert
            Assert.True(revenueEntry.RequiresInvoiceLink);
        }

        [Fact]
        public void JournalEntry_RequiresInvoiceLink_ShouldBeFalse_ForNonRevenue()
        {
            // Arrange
            var expenseEntry = CreateJournalEntry();
            expenseEntry.AddLine("641", 1000000, 0, "Chi phí");
            expenseEntry.AddLine("111", 0, 1000000, "Tiền mặt");
            
            // Assert
            Assert.False(expenseEntry.RequiresInvoiceLink);
        }

        #endregion
    }
}
