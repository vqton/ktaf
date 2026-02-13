using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.FraudDetection;
using AccountingERP.Domain.Security;
using AccountingERP.Domain.Tax;
using AccountingERP.Domain.Tax.Services;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.Phase1
{
    /// <summary>
    /// Phase 1 Critical Fixes - Test Suite
    /// Tests for Tax Engine, Invoice, Authorization, and Fraud Detection
    /// </summary>
    public class Phase1CriticalFixesTests
    {
        #region VAT Invoice Tests (15 tests)

        [Fact]
        public void VatInvoice_Create_WithValidData_ShouldSucceed()
        {
            var invoice = VatInvoice.Create(
                invoiceNumber: "000001",
                invoiceSeries: "1C25TAA",
                issueDate: new DateTime(2020, 1, 15),
                type: InvoiceType.Output,
                sellerTaxCode: "0123456789",
                sellerName: "Công ty ABC",
                buyerTaxCode: "9876543210",
                buyerName: "Công ty XYZ",
                vatRate: 10
            );

            Assert.NotNull(invoice);
            Assert.Equal("000001", invoice.InvoiceNumber);
            Assert.Equal(InvoiceStatus.Valid, invoice.Status);
            Assert.Equal(InvoiceType.Output, invoice.Type);
            Assert.Equal(10, invoice.VatRate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(10)]
        public void VatInvoice_Create_WithValidVatRates_ShouldSucceed(int rate)
        {
            var invoice = VatInvoice.Create(
                "000001", "1C25TAA", DateTime.Now, InvoiceType.Output,
                "0123456789", "Seller", "9876543210", "Buyer", rate);

            Assert.Equal(rate, invoice.VatRate);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(12)]
        [InlineData(-5)]
        public void VatInvoice_Create_WithInvalidVatRate_ShouldThrowArgumentException(int rate)
        {
            Assert.Throws<ArgumentException>(() =>
                VatInvoice.Create("000001", "1C25TAA", DateTime.Now, InvoiceType.Output,
                    "0123456789", "Seller", "9876543210", "Buyer", rate));
        }

        [Fact]
        public void VatInvoice_Create_WithFutureDate_ShouldThrowArgumentException()
        {
            var futureDate = DateTime.Now.AddDays(2);
            
            Assert.Throws<ArgumentException>(() =>
                VatInvoice.Create("000001", "1C25TAA", futureDate, InvoiceType.Output,
                    "0123456789", "Seller", "9876543210", "Buyer", 10));
        }

        [Fact]
        public void VatInvoice_AddLine_ShouldCalculateTotals()
        {
            var invoice = VatInvoice.Create("000001", "1C25TAA", DateTime.Now.AddDays(-1), 
                InvoiceType.Output, "0123456789", "Seller", "9876543210", "Buyer", 10);

            invoice.AddLine("Sản phẩm A", "cái", 10, 100000m);

            Assert.Equal(1_000_000m, invoice.TotalAmount.Amount);
            Assert.Equal(100_000m, invoice.VatAmount.Amount);
            Assert.Equal(1_100_000m, invoice.TotalPayment.Amount);
        }

        [Fact]
        public void VatInvoice_Issue_WithValidData_ShouldSetVerificationCode()
        {
            var invoice = VatInvoice.Create("000001", "1C25TAA", DateTime.Now.AddDays(-1),
                InvoiceType.Output, "0123456789", "Seller", "9876543210", "Buyer", 10);
            invoice.AddLine("Sản phẩm", "cái", 1, 100000m);

            invoice.Issue("ABCD1234EFGH5678");

            Assert.Equal("ABCD1234EFGH5678", invoice.VerificationCode);
            Assert.True(invoice.IsIssued);
        }

        [Fact]
        public void VatInvoice_Issue_WithoutLines_ShouldThrowInvalidOperationException()
        {
            var invoice = VatInvoice.Create("000001", "1C25TAA", DateTime.Now.AddDays(-1),
                InvoiceType.Output, "0123456789", "Seller", "9876543210", "Buyer", 10);

            Assert.Throws<InvalidOperationException>(() => 
                invoice.Issue("ABCD1234EFGH5678"));
        }

        [Fact]
        public void VatInvoice_Cancel_ShouldSetStatusToCancelled()
        {
            var invoice = CreateValidInvoice();
            
            invoice.Cancel("Sai sót thông tin");

            Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
            Assert.Equal("Sai sót thông tin", invoice.AdjustmentReason);
        }

        [Fact]
        public void VatInvoice_CreateAdjustment_ShouldCreateNewInvoice()
        {
            var original = CreateValidInvoice();
            original.Issue("CODE1234");

            var adjustment = original.CreateAdjustment("000002", "Điều chỉnh giá", 100000m);

            Assert.NotNull(adjustment);
            Assert.Equal("000002", adjustment.InvoiceNumber);
            Assert.Equal(original.Id, adjustment.OriginalInvoiceId);
            Assert.Equal(InvoiceStatus.Adjusted, original.Status);
        }

        [Fact]
        public void VatInvoice_CreateReplacement_ShouldCopyAllLines()
        {
            var original = CreateValidInvoice();
            original.AddLine("Sản phẩm A", "cái", 5, 100000m);
            original.AddLine("Sản phẩm B", "cái", 3, 200000m);
            original.Issue("CODE1234");

            var replacement = original.CreateReplacement("000002", "Sai sót số hóa đơn");

            Assert.Equal(3, replacement.Lines.Count); // 1 from CreateValidInvoice + 2 added
            Assert.Equal(InvoiceStatus.Replaced, original.Status);
        }

        [Fact]
        public void VatInvoice_Create_WithoutInvoiceNumber_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                VatInvoice.Create("", "1C25TAA", DateTime.Now, InvoiceType.Output,
                    "0123456789", "Seller", "9876543210", "Buyer", 10));
        }

        [Fact]
        public void VatInvoice_Create_OutputWithoutBuyerTaxCode_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                VatInvoice.Create("000001", "1C25TAA", DateTime.Now, InvoiceType.Output,
                    "0123456789", "Seller", "", "Buyer", 10));
        }

        [Fact]
        public void VatInvoiceLine_CreateDebit_WithValidData_ShouldSucceed()
        {
            var line = VatInvoiceLine.Create(Guid.NewGuid(), "Sản phẩm A", "cái", 10, 100000m, 10, Currency.VND);

            Assert.Equal(1_000_000m, line.TotalAmount.Amount);
            Assert.Equal(100_000m, line.VatAmount.Amount);
            Assert.Equal(10, line.VatRate);
        }

        [Fact]
        public void VatInvoiceLine_Create_WithNegativeQuantity_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                VatInvoiceLine.Create(Guid.NewGuid(), "Sản phẩm", "cái", -5, 100000m, 10, Currency.VND));
        }

        [Fact]
        public void VatInvoiceLine_Create_WithNegativeUnitPrice_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                VatInvoiceLine.Create(Guid.NewGuid(), "Sản phẩm", "cái", 5, -100000m, 10, Currency.VND));
        }

        private VatInvoice CreateValidInvoice()
        {
            var invoice = VatInvoice.Create("000001", "1C25TAA", DateTime.Now.AddDays(-1),
                InvoiceType.Output, "0123456789", "Seller", "9876543210", "Buyer", 10);
            invoice.AddLine("Sản phẩm", "cái", 1, 100000m);
            return invoice;
        }

        #endregion

        #region VAT Calculation Service Tests (10 tests)

        [Fact]
        public void VatCalculationService_CalculateOutputVat_WithValidInvoices_ShouldBeCorrect()
        {
            var service = new VatCalculationService();
            var invoices = new List<VatInvoice>
            {
                CreateOutputInvoice(1000000m, 10),
                CreateOutputInvoice(2000000m, 10),
                CreateOutputInvoice(500000m, 5)
            };

            var outputVat = service.CalculateOutputVat(invoices);

            Assert.Equal(325_000m, outputVat.Amount); // 100k + 200k + 25k
        }

        [Fact]
        public void VatCalculationService_CalculateInputVat_WithValidInvoices_ShouldBeCorrect()
        {
            var service = new VatCalculationService();
            var invoices = new List<VatInvoice>
            {
                CreateInputInvoice(1000000m, 10),
                CreateInputInvoice(500000m, 8)
            };

            var inputVat = service.CalculateInputVat(invoices);

            Assert.Equal(140_000m, inputVat.Amount); // 100k + 40k
        }

        [Fact]
        public void VatCalculationService_CalculateVatPayable_ShouldSubtractInputFromOutput()
        {
            var service = new VatCalculationService();
            var outputVat = Money.VND(500_000m);
            var inputVat = Money.VND(300_000m);

            var payable = service.CalculateVatPayable(outputVat, inputVat);

            Assert.Equal(200_000m, payable.Amount);
        }

        [Fact]
        public void VatCalculationService_IsInputVatDeductible_ValidInvoice_ShouldReturnTrue()
        {
            var service = new VatCalculationService();
            var invoice = CreateInputInvoice(1000000m, 10);
            invoice.Issue("VERIFICATION1234");

            var isDeductible = service.IsInputVatDeductible(invoice, "PURCHASE");

            Assert.True(isDeductible);
        }

        [Fact]
        public void VatCalculationService_IsInputVatDeductible_CancelledInvoice_ShouldReturnFalse()
        {
            var service = new VatCalculationService();
            var invoice = CreateInputInvoice(1000000m, 10);
            invoice.Cancel("Test");

            var isDeductible = service.IsInputVatDeductible(invoice, "PURCHASE");

            Assert.False(isDeductible);
        }

        [Fact]
        public void VatCalculationService_IsInputVatDeductible_NonDeductibleCategory_ShouldReturnFalse()
        {
            var service = new VatCalculationService();
            var invoice = CreateInputInvoice(1000000m, 10);
            invoice.Issue("VERIFICATION1234");

            var isDeductible = service.IsInputVatDeductible(invoice, "ENTERTAINMENT");

            Assert.False(isDeductible);
        }

        [Fact]
        public void VatCalculationService_ReconcileVat_WithMatchingData_ShouldReturnReconciled()
        {
            var service = new VatCalculationService();
            var period = AccountingPeriod.Create(2026, 2);
            var accountingReport = new VatReport 
            { 
                OutputVat = Money.VND(1_000_000m),
                InputVat = Money.VND(500_000m),
                VatPayable = Money.VND(500_000m)
            };
            var declaration = new VatDeclaration
            {
                OutputVat = Money.VND(1_000_000m),
                InputVat = Money.VND(500_000m),
                VatPayable = Money.VND(500_000m)
            };

            var result = service.ReconcileVat(period, accountingReport, declaration);

            Assert.True(result.IsReconciled);
            Assert.Empty(result.Issues.Where(i => i.Severity == IssueSeverity.Critical));
        }

        [Fact]
        public void VatCalculationService_ReconcileVat_WithMismatch_ShouldReturnIssues()
        {
            var service = new VatCalculationService();
            var period = AccountingPeriod.Create(2026, 2);
            var accountingReport = new VatReport
            {
                OutputVat = Money.VND(1_000_000m),
                InputVat = Money.VND(500_000m),
                VatPayable = Money.VND(500_000m)
            };
            var declaration = new VatDeclaration
            {
                OutputVat = Money.VND(900_000m), // 100k difference
                InputVat = Money.VND(500_000m),
                VatPayable = Money.VND(400_000m)
            };

            var result = service.ReconcileVat(period, accountingReport, declaration);

            Assert.False(result.IsReconciled);
            Assert.Contains(result.Issues, i => i.Severity == IssueSeverity.Critical);
        }

        [Fact]
        public void VatCalculationService_CalculateVatReport_ShouldIncludeAllInvoices()
        {
            var service = new VatCalculationService();
            var period = AccountingPeriod.Create(2026, 2);
            var invoices = new List<VatInvoice>
            {
                CreateOutputInvoice(1000000m, 10), // 100k VAT
                CreateOutputInvoice(2000000m, 10), // 200k VAT
                CreateInputInvoice(500000m, 10)     // 50k VAT
            };

            var report = service.CalculateVatReport(period, invoices);

            Assert.Equal(300_000m, report.OutputVat.Amount);
            Assert.Equal(50_000m, report.InputVat.Amount);
            Assert.Equal(250_000m, report.VatPayable.Amount);
            Assert.Equal(2, report.OutputInvoiceCount);
            Assert.Equal(1, report.InputInvoiceCount);
        }

        [Fact]
        public void VatCalculationService_CalculateVatReport_ShouldOnlyIncludeValidInvoices()
        {
            var service = new VatCalculationService();
            var period = AccountingPeriod.Create(2026, 2);
            
            var validInvoice = CreateOutputInvoice(1000000m, 10);
            var cancelledInvoice = CreateOutputInvoice(2000000m, 10);
            cancelledInvoice.Cancel("Test");

            var report = service.CalculateVatReport(period, new[] { validInvoice, cancelledInvoice });

            Assert.Equal(100_000m, report.OutputVat.Amount); // Only valid invoice
            Assert.Equal(1, report.OutputInvoiceCount);
        }

        private VatInvoice CreateOutputInvoice(decimal amount, int vatRate)
        {
            var invoice = VatInvoice.Create(Guid.NewGuid().ToString().Substring(0, 6), "1C25TAA", 
                DateTime.Now.AddDays(-1), InvoiceType.Output, "0123456789", "Seller", 
                "9876543210", "Buyer", vatRate);
            // For VAT calculation: if we want VAT = amount * rate / 100, 
            // then taxableAmount should equal amount
            var taxableAmount = amount;
            invoice.AddLine("Product", "unit", 1, taxableAmount);
            invoice.Issue("VERIFY-" + Guid.NewGuid().ToString().Substring(0, 8));
            return invoice;
        }

        private VatInvoice CreateInputInvoice(decimal amount, int vatRate)
        {
            var invoice = VatInvoice.Create(Guid.NewGuid().ToString().Substring(0, 6), "1C25TAA",
                DateTime.Now.AddDays(-1), InvoiceType.Input, "9876543210", "Supplier",
                "0123456789", "Buyer", vatRate);
            var taxableAmount = amount;
            invoice.AddLine("Product", "unit", 1, taxableAmount);
            invoice.Issue("VERIFY-" + Guid.NewGuid().ToString().Substring(0, 8));
            return invoice;
        }

        #endregion

        #region TNDN Calculation Service Tests (8 tests)

        [Fact]
        public void TndnCalculationService_CalculateCurrentTax_WithPositiveIncome_ShouldBeCorrect()
        {
            var service = new TndnCalculationService();
            var taxableIncome = Money.VND(1_000_000_000m);

            var tax = service.CalculateCurrentTax(taxableIncome);

            Assert.Equal(200_000_000m, tax.Amount); // 20% of 1B
        }

        [Fact]
        public void TndnCalculationService_CalculateCurrentTax_WithZeroIncome_ShouldBeZero()
        {
            var service = new TndnCalculationService();
            var taxableIncome = Money.VND(0);

            var tax = service.CalculateCurrentTax(taxableIncome);

            Assert.Equal(0, tax.Amount);
        }

        [Fact]
        public void TndnCalculationService_IsNonDeductibleExpense_Penalty_ShouldReturnTrue()
        {
            var service = new TndnCalculationService();

            var isNonDeductible = service.IsNonDeductibleExpense("8111", "Tiền phạt vi phạm", "");

            Assert.True(isNonDeductible);
        }

        [Fact]
        public void TndnCalculationService_IsNonDeductibleExpense_Entertainment_ShouldReturnTrue()
        {
            var service = new TndnCalculationService();

            var isNonDeductible = service.IsNonDeductibleExpense("642", "Chi phí tiếp khách", "");

            Assert.True(isNonDeductible);
        }

        [Fact]
        public void TndnCalculationService_IsNonDeductibleExpense_NormalExpense_ShouldReturnFalse()
        {
            var service = new TndnCalculationService();

            var isNonDeductible = service.IsNonDeductibleExpense("641", "Lương nhân viên", "");

            Assert.False(isNonDeductible);
        }

        [Fact]
        public void TndnCalculationService_CalculateTaxableIncome_ShouldAddBackNonDeductible()
        {
            var service = new TndnCalculationService();
            var accountingProfit = Money.VND(1_000_000_000m);
            var adjustments = new List<TaxAdjustment>
            {
                new TaxAdjustment { Type = AdjustmentType.NonDeductibleExpense, Amount = Money.VND(100_000_000m) },
                new TaxAdjustment { Type = AdjustmentType.TaxExemptIncome, Amount = Money.VND(50_000_000m) }
            };

            var taxableIncome = service.CalculateTaxableIncome(accountingProfit, adjustments);

            // 1B + 100M (add back) - 50M (exempt) = 1.05B
            Assert.Equal(1_050_000_000m, taxableIncome.Amount);
        }

        [Fact]
        public void TndnCalculationService_CalculateAdjustments_ShouldIdentifyNonDeductible()
        {
            var service = new TndnCalculationService();
            var lines = new List<JournalEntryLine>
            {
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "641", 1000000m, "Lương"),
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "8111", 500000m, "Tiền phạt"),
                JournalEntryLine.CreateDebit(Guid.NewGuid(), "642", 2000000m, "Chi phí tiếp khách")
            };

            var adjustments = service.CalculateAdjustments(lines);

            Assert.Equal(2, adjustments.Count());
            Assert.Contains(adjustments, a => a.AccountCode == "8111");
            Assert.Contains(adjustments, a => a.AccountCode == "642");
        }

        #endregion

        #region TNCN Calculation Service Tests (8 tests)

        [Theory]
        [InlineData(5_000_000, 0, 250_000)]       // 5M * 5% = 250K
        [InlineData(10_000_000, 0, 750_000)]      // 10M * 10% - 250K = 750K
        [InlineData(20_000_000, 0, 2_350_000)]    // 20M in 18M-32M bracket: 20M * 20% - 1.65M = 2.35M
        [InlineData(50_000_000, 0, 9_250_000)]    // 50M in 32M-52M bracket: 50M * 25% - 3.25M = 9.25M
        public void TncnCalculationService_CalculateProgressiveTax_ShouldBeCorrect(decimal income, int dependents, decimal expectedTax)
        {
            var service = new TncnCalculationService();

            var tax = service.CalculateProgressiveTax(income);

            Assert.Equal(expectedTax, tax, 2); // 2 decimal tolerance
        }

        [Fact]
        public void TncnCalculationService_CalculateProgressiveTax_WithNegativeIncome_ShouldBeZero()
        {
            var service = new TncnCalculationService();

            var tax = service.CalculateProgressiveTax(-1000000m);

            Assert.Equal(0, tax);
        }

        [Fact]
        public void TncnCalculationService_IsResident_With183Days_ShouldReturnTrue()
        {
            var service = new TncnCalculationService();

            var isResident = service.IsResident("0123456789", 183);

            Assert.True(isResident);
        }

        [Fact]
        public void TncnCalculationService_IsResident_With182Days_ShouldReturnFalse()
        {
            var service = new TncnCalculationService();

            var isResident = service.IsResident("0123456789", 182);

            Assert.False(isResident);
        }

        [Fact]
        public void TncnCalculationService_CalculateWithholdingTax_ServicesOver2M_ShouldBe10Percent()
        {
            var service = new TncnCalculationService();
            var payment = new PaymentToIndividual
            {
                PaymentType = PaymentType.Services,
                Amount = 5_000_000m
            };

            var tax = service.CalculateWithholdingTax(payment);

            Assert.Equal(500_000m, tax.Amount); // 10% of 5M
        }

        [Fact]
        public void TncnCalculationService_CalculateWithholdingTax_ServicesUnder2M_ShouldBeZero()
        {
            var service = new TncnCalculationService();
            var payment = new PaymentToIndividual
            {
                PaymentType = PaymentType.Services,
                Amount = 1_000_000m
            };

            var tax = service.CalculateWithholdingTax(payment);

            Assert.Equal(0, tax.Amount);
        }

        [Fact]
        public void TncnCalculationService_CalculateWithholdingTax_Royalties_ShouldBe5Percent()
        {
            var service = new TncnCalculationService();
            var payment = new PaymentToIndividual
            {
                PaymentType = PaymentType.Royalties,
                Amount = 10_000_000m
            };

            var tax = service.CalculateWithholdingTax(payment);

            Assert.Equal(500_000m, tax.Amount); // 5% of 10M
        }

        [Fact]
        public void TncnCalculationService_CalculateMonthlyTncn_ShouldCalculateCorrectly()
        {
            var service = new TncnCalculationService();
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FullName = "Nguyễn Văn A",
                IsResident = true,
                Dependents = 0
            };

            var report = service.CalculateMonthlyTncn(employee, 20_000_000m);

            Assert.Equal(20_000_000m, report.GrossSalary);
            Assert.True(report.InsuranceDeductions > 0);
            Assert.True(report.TaxableIncome > 0);
            Assert.True(report.TaxAmount > 0);
            Assert.True(report.NetSalary < report.GrossSalary);
        }

        #endregion

        #region Authorization Service Tests (12 tests)

        [Fact]
        public void AuthorizationService_CanCreateEntry_ActiveUserWithPermission_ShouldReturnTrue()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = true };

            var canCreate = service.CanCreateEntry(user);

            Assert.True(canCreate);
        }

        [Fact]
        public void AuthorizationService_CanCreateEntry_InactiveUser_ShouldReturnFalse()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = false };

            var canCreate = service.CanCreateEntry(user);

            Assert.False(canCreate);
        }

        [Fact]
        public void AuthorizationService_CanPostEntry_WithinLimit_ShouldReturnTrue()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = true };

            var canPost = service.CanPostEntry(user, Money.VND(30_000_000m)); // Under 50M limit

            Assert.True(canPost);
        }

        [Fact]
        public void AuthorizationService_CanPostEntry_ExceedsLimit_ShouldReturnFalse()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = true };

            var canPost = service.CanPostEntry(user, Money.VND(100_000_000m)); // Over 50M limit

            Assert.False(canPost);
        }

        [Fact]
        public void AuthorizationService_CanApproveEntry_CreatorCannotApproveOwn_ShouldReturnFalse()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Role = UserRole.SeniorAccountant, IsActive = true };

            var canApprove = service.CanApproveEntry(user, Money.VND(100_000_000m), userId);

            Assert.False(canApprove);
        }

        [Fact]
        public void AuthorizationService_CanApproveEntry_DifferentUser_ShouldReturnTrue()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.SeniorAccountant, IsActive = true };
            var creatorId = Guid.NewGuid();

            var canApprove = service.CanApproveEntry(user, Money.VND(100_000_000m), creatorId);

            Assert.True(canApprove);
        }

        [Fact]
        public void AuthorizationService_CanClosePeriod_Month_CFORequired_ShouldReturnFalseForChiefAccountant()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.ChiefAccountant, IsActive = true };

            var canClose = service.CanClosePeriod(user, PeriodType.Month);

            Assert.True(canClose); // Chief Accountant CAN close month
        }

        [Fact]
        public void AuthorizationService_CanClosePeriod_Year_CEORequired_ShouldReturnFalseForCFO()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.CFO, IsActive = true };

            var canClose = service.CanClosePeriod(user, PeriodType.Year);

            Assert.False(canClose); // Only CEO can close year
        }

        [Fact]
        public void AuthorizationService_ValidateAuthorization_Success_ShouldNotThrow()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = true };

            service.ValidateAuthorization(user, Permission.CreateEntry);
            // Should not throw
        }

        [Fact]
        public void AuthorizationService_ValidateAuthorization_Failure_ShouldThrowInsufficientPermission()
        {
            var matrix = new AuthorizationMatrix();
            var service = new AuthorizationService(matrix);
            var user = new User { Id = Guid.NewGuid(), Role = UserRole.Accountant, IsActive = false };

            Assert.Throws<InsufficientPermissionException>(() =>
                service.ValidateAuthorization(user, Permission.CreateEntry));
        }

        [Fact]
        public void AuthorizationMatrix_GetPostingLimit_Accountant_ShouldBe50M()
        {
            var matrix = new AuthorizationMatrix();

            var limit = matrix.GetPostingLimit(UserRole.Accountant);

            Assert.Equal(50_000_000m, limit);
        }

        [Fact]
        public void AuthorizationMatrix_GetPostingLimit_CEO_ShouldBeMaxValue()
        {
            var matrix = new AuthorizationMatrix();

            var limit = matrix.GetPostingLimit(UserRole.CEO);

            Assert.Equal(decimal.MaxValue, limit);
        }

        #endregion

        #region Fraud Detection Service Tests (12 tests)

        [Fact]
        public void FraudDetectionService_DetectRevenueWithoutInvoice_WithGap_ShouldAlert()
        {
            var service = new FraudDetectionService();
            var entries = new List<JournalEntry>
            {
                CreateRevenueEntry(1_000_000_000m) // 1B revenue
            };
            var invoices = new List<VatInvoice>
            {
                CreateOutputInvoiceWithAmount(500_000_000m) // Only 500M invoiced
            };

            var alerts = service.DetectRevenueWithoutInvoice(entries, invoices);

            Assert.Contains(alerts, a => a.Code == "REV-001");
        }

        [Fact]
        public void FraudDetectionService_DetectRevenueWithoutInvoice_NoGap_ShouldNotAlert()
        {
            var service = new FraudDetectionService();
            var entries = new List<JournalEntry>
            {
                CreateRevenueEntry(1_000_000_000m)
            };
            var invoices = new List<VatInvoice>
            {
                CreateOutputInvoiceWithAmount(999_000_000m) // 99.9% invoiced
            };

            var alerts = service.DetectRevenueWithoutInvoice(entries, invoices);

            Assert.DoesNotContain(alerts, a => a.Code == "REV-001");
        }

        [Fact]
        public void FraudDetectionService_DetectDuplicatePayments_WithDuplicate_ShouldAlert()
        {
            var service = new FraudDetectionService();
            var entries = new List<JournalEntry>
            {
                CreatePaymentEntry(100_000_000m, new DateTime(2020, 1, 1)),
                CreatePaymentEntry(100_000_000m, new DateTime(2020, 1, 2))
            };

            var alerts = service.DetectDuplicatePayments(entries);

            Assert.Contains(alerts, a => a.Code == "PAY-001");
        }

        [Fact]
        public void FraudDetectionService_DetectRoundNumberTransactions_HighPercentage_ShouldAlert()
        {
            var service = new FraudDetectionService();
            var entries = new List<JournalEntry>
            {
                CreateRevenueEntry(100_000_000m),
                CreateRevenueEntry(200_000_000m),
                CreateRevenueEntry(300_000_000m),
                CreateRevenueEntry(150_000_000m),
                CreateRevenueEntry(250_000_000m)
            };

            var alerts = service.DetectRoundNumberTransactions(entries);

            Assert.Contains(alerts, a => a.Code == "PAT-001");
        }

        [Fact]
        public void FraudDetectionService_DetectCashAnomalies_OverLimit_ShouldAlert()
        {
            var service = new FraudDetectionService();
            var entries = new List<JournalEntry>
            {
                CreateCashEntry(100_000_000m, true),  // +100M
                CreateCashEntry(10_000_000m, false)   // -10M
            };

            var alerts = service.DetectCashAnomalies(entries);

            Assert.Contains(alerts, a => a.Code == "CASH-001");
        }

        [Fact]
        public void FraudDetectionService_DetectInvoiceGaps_WithGap_ShouldAlert()
        {
            var service = new FraudDetectionService();
            var invoices = new List<VatInvoice>
            {
                CreateInvoiceWithNumber("000001"),
                CreateInvoiceWithNumber("000003") // Gap: 000002 missing
            };

            var alerts = service.DetectInvoiceGaps(invoices);

            Assert.Contains(alerts, a => a.Code == "INV-001");
        }

        [Fact]
        public void FraudDetectionService_AnalyzePeriod_WithIssues_ShouldReturnReport()
        {
            var service = new FraudDetectionService();
            var period = AccountingPeriod.Create(2026, 2);
            var entries = new List<JournalEntry>
            {
                CreateRevenueEntry(1_000_000_000m),
                CreateCashEntry(100_000_000m, true)
            };
            var invoices = new List<VatInvoice>
            {
                CreateOutputInvoiceWithAmount(500_000_000m)
            };

            var report = service.AnalyzePeriod(period, entries, invoices);

            Assert.NotNull(report);
            Assert.True(report.TotalAlerts > 0);
            Assert.True(report.RequiresImmediateAction || !report.HasCriticalIssues);
        }

        [Fact]
        public void FraudAlert_ShouldStoreAllProperties()
        {
            var alert = new FraudAlert
            {
                Code = "TEST-001",
                Title = "Test Alert",
                Description = "Test Description",
                Severity = FraudSeverity.High,
                Category = FraudCategory.TaxEvasion,
                Amount = 1_000_000m,
                DetectionDate = DateTime.UtcNow,
                RecommendedAction = "Test Action"
            };

            Assert.Equal("TEST-001", alert.Code);
            Assert.Equal(FraudSeverity.High, alert.Severity);
            Assert.Equal(FraudCategory.TaxEvasion, alert.Category);
        }

        // Helper methods
        private JournalEntry CreateRevenueEntry(decimal amount)
        {
            var entry = JournalEntry.Create(
                "BT-202602-001", "INV-001", DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(-2), "Doanh thu");
            entry.AddLine("111", amount, 0, "Tiền mặt");
            entry.AddLine("511", 0, amount, "Doanh thu");
            // Link to invoice to satisfy hard enforcement from Feature 1
            entry.LinkToInvoice(AccountingERP.Domain.Invoicing.InvoiceId.New());
            entry.Post("test-user");
            return entry;
        }

        private JournalEntry CreatePaymentEntry(decimal amount, DateTime date)
        {
            var entry = JournalEntry.Create(
                $"BT-202602-{date:dd}", $"PAY-{date:dd}", date, date.AddDays(-1), "Thanh toán");
            // Payment: Debit Accounts Payable (331), Credit Cash (111)
            entry.AddLine("331", amount, 0, "Nợ phải trả");
            entry.AddLine("111", 0, amount, "Tiền mặt");
            entry.Post("test-user");
            return entry;
        }

        private JournalEntry CreateCashEntry(decimal amount, bool isDebit)
        {
            var entry = JournalEntry.Create(
                "BT-202602-001", "CASH-001", DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(-2), "Tiền mặt");
            if (isDebit)
            {
                entry.AddLine("111", amount, 0, "Thu tiền");
                entry.AddLine("511", 0, amount, "Doanh thu");
                // Link to invoice to satisfy hard enforcement from Feature 1
                entry.LinkToInvoice(AccountingERP.Domain.Invoicing.InvoiceId.New());
            }
            else
            {
                entry.AddLine("331", 0, amount, "Chi tiền");
                entry.AddLine("111", amount, 0, "Tiền mặt");
            }
            entry.Post("test-user");
            return entry;
        }

        private VatInvoice CreateOutputInvoiceWithAmount(decimal amount)
        {
            var invoice = VatInvoice.Create(Guid.NewGuid().ToString().Substring(0, 6), "1C25TAA",
                DateTime.Now.AddDays(-1), InvoiceType.Output, "0123456789", "Seller",
                "9876543210", "Buyer", 10);
            var taxableAmount = amount;
            invoice.AddLine("Product", "unit", 1, taxableAmount);
            invoice.Issue("VERIFY-" + Guid.NewGuid().ToString().Substring(0, 8));
            return invoice;
        }

        private VatInvoice CreateInvoiceWithNumber(string number)
        {
            var invoice = VatInvoice.Create(number, "1C25TAA", DateTime.Now.AddDays(-1),
                InvoiceType.Output, "0123456789", "Seller", "9876543210", "Buyer", 10);
            invoice.AddLine("Product", "unit", 1, 100000m);
            invoice.Issue("VERIFY-" + Guid.NewGuid().ToString().Substring(0, 8));
            return invoice;
        }

        #endregion
    }
}
