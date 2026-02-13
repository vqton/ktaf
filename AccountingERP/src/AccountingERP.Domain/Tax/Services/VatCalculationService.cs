using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.Services
{
    /// <summary>
    /// Domain Service: Tính toán và quản lý thuế GTGT
    /// TT219/2013/TT-BTC: Luật thuế GTGT
    /// </summary>
    public interface IVatCalculationService
    {
        VatReport CalculateVatReport(AccountingPeriod period, IEnumerable<VatInvoice> invoices);
        Money CalculateOutputVat(IEnumerable<VatInvoice> outputInvoices);
        Money CalculateInputVat(IEnumerable<VatInvoice> inputInvoices);
        Money CalculateVatPayable(Money outputVat, Money inputVat);
        bool IsInputVatDeductible(VatInvoice invoice, string expenseCategory);
        VatReconciliationResult ReconcileVat(AccountingPeriod period, VatReport accountingReport, VatDeclaration declaration);
    }

    /// <summary>
    /// Implementation of VAT Calculation Service
    /// </summary>
    public class VatCalculationService : IVatCalculationService
    {
        /// <summary>
        /// Tính toán báo cáo VAT đầy đủ cho kỳ
        /// </summary>
        public VatReport CalculateVatReport(AccountingPeriod period, IEnumerable<VatInvoice> invoices)
        {
            var outputInvoices = invoices.Where(i => 
                i.Type == InvoiceType.Output && 
                i.IsValid && 
                i.IssueDate.Month == period.Month && 
                i.IssueDate.Year == period.Year);

            var inputInvoices = invoices.Where(i => 
                i.Type == InvoiceType.Input && 
                i.IsValid && 
                i.IssueDate.Month == period.Month && 
                i.IssueDate.Year == period.Year);

            var outputVat = CalculateOutputVat(outputInvoices);
            var inputVat = CalculateInputVat(inputInvoices);
            var deductibleInputVat = CalculateDeductibleInputVat(inputInvoices);
            var nonDeductibleInputVat = CalculateNonDeductibleInputVat(inputInvoices);
            var payable = CalculateVatPayable(outputVat, deductibleInputVat);

            return new VatReport
            {
                Period = period,
                OutputVat = outputVat,
                InputVat = inputVat,
                DeductibleInputVat = deductibleInputVat,
                NonDeductibleInputVat = nonDeductibleInputVat,
                VatPayable = payable,
                OutputInvoiceCount = outputInvoices.Count(),
                InputInvoiceCount = inputInvoices.Count(),
                TotalOutputAmount = Money.VND(outputInvoices.Sum(i => i.TotalAmount.Amount)),
                TotalInputAmount = Money.VND(inputInvoices.Sum(i => i.TotalAmount.Amount))
            };
        }

        /// <summary>
        /// Tính thuế GTGT đầu ra
        /// </summary>
        public Money CalculateOutputVat(IEnumerable<VatInvoice> outputInvoices)
        {
            var validInvoices = outputInvoices.Where(i => i.IsValid);
            return Money.VND(validInvoices.Sum(i => i.VatAmount.Amount));
        }

        /// <summary>
        /// Tính thuế GTGT đầu vào (tổng)
        /// </summary>
        public Money CalculateInputVat(IEnumerable<VatInvoice> inputInvoices)
        {
            var validInvoices = inputInvoices.Where(i => i.IsValid);
            return Money.VND(validInvoices.Sum(i => i.VatAmount.Amount));
        }

        /// <summary>
        /// Tính thuế GTGT đầu vào được khấu trừ
        /// </summary>
        private Money CalculateDeductibleInputVat(IEnumerable<VatInvoice> inputInvoices)
        {
            var validInvoices = inputInvoices.Where(i => 
                i.IsValid && 
                IsInputVatDeductible(i, string.Empty));
            return Money.VND(validInvoices.Sum(i => i.VatAmount.Amount));
        }

        /// <summary>
        /// Tính thuế GTGT đầu vào không được khấu trừ
        /// </summary>
        private Money CalculateNonDeductibleInputVat(IEnumerable<VatInvoice> inputInvoices)
        {
            var validInvoices = inputInvoices.Where(i => 
                i.IsValid && 
                !IsInputVatDeductible(i, string.Empty));
            return Money.VND(validInvoices.Sum(i => i.VatAmount.Amount));
        }

        /// <summary>
        /// Tính thuế GTGT phải nộp
        /// </summary>
        public Money CalculateVatPayable(Money outputVat, Money inputVat)
        {
            var payable = outputVat.Subtract(inputVat);
            return payable;
        }

        /// <summary>
        /// Kiểm tra thuế GTGT đầu vào có được khấu trừ không
        /// TT219-Điều 14: Các trường hợp không được khấu trừ
        /// </summary>
        public bool IsInputVatDeductible(VatInvoice invoice, string expenseCategory)
        {
            // Check 1: Hóa đơn phải hợp lệ
            if (!invoice.IsValid)
                return false;

            // Check 2: MST ngườii bán phải hợp lệ (10 hoặc 14 số)
            if (!IsValidTaxCode(invoice.SellerTaxCode))
                return false;

            // Check 3: Hóa đơn phải có mã tra cứu (đã phát hành)
            if (!invoice.IsIssued)
                return false;

            // Check 4: Các khoản chi phí không được khấu trừ
            var nonDeductibleCategories = new[]
            {
                "VEHICLE_PASSENGER",     // Xe chở ngườii (trừ trường hợp đặc biệt)
                "ENTERTAINMENT",         // Chi phí giải trí
                "WELFARE_UNREGULATED",   // Phúc lợi không theo quy định
                "PENALTY",              // Tiền phạt
                "PERSONAL"              // Chi phí cá nhân
            };

            if (nonDeductibleCategories.Contains(expenseCategory))
                return false;

            // Check 5: Hóa đơn từ ngườii nộp thuế không phải là tổ chức
            // (Simplified check - in real system would query tax authority DB)
            if (invoice.SellerTaxCode.StartsWith("0"))
                return false;

            // Check 6: Hàng hóa dịch vụ sử dụng cho hoạt động không chịu thuế GTGT
            // (This would require additional context)

            return true;
        }

        /// <summary>
        /// Đối chiếu VAT sổ sách và tờ khai
        /// </summary>
        public VatReconciliationResult ReconcileVat(
            AccountingPeriod period, 
            VatReport accountingReport, 
            VatDeclaration declaration)
        {
            var outputDiff = Math.Abs(accountingReport.OutputVat.Amount - declaration.OutputVat.Amount);
            var inputDiff = Math.Abs(accountingReport.InputVat.Amount - declaration.InputVat.Amount);
            var payableDiff = Math.Abs(accountingReport.VatPayable.Amount - declaration.VatPayable.Amount);

            const decimal TOLERANCE = 1000m; // 1,000 VND

            var issues = new List<VatReconciliationIssue>();

            if (outputDiff > TOLERANCE)
            {
                issues.Add(new VatReconciliationIssue
                {
                    Type = VatReconciliationIssueType.OutputMismatch,
                    Severity = IssueSeverity.Critical,
                    Description = $"Chênh lệch VAT đầu ra: Sổ sách {accountingReport.OutputVat:N0} vs Tờ khai {declaration.OutputVat:N0}",
                    Difference = outputDiff
                });
            }

            if (inputDiff > TOLERANCE)
            {
                issues.Add(new VatReconciliationIssue
                {
                    Type = VatReconciliationIssueType.InputMismatch,
                    Severity = IssueSeverity.Critical,
                    Description = $"Chênh lệch VAT đầu vào: Sổ sách {accountingReport.InputVat:N0} vs Tờ khai {declaration.InputVat:N0}",
                    Difference = inputDiff
                });
            }

            if (payableDiff > TOLERANCE)
            {
                issues.Add(new VatReconciliationIssue
                {
                    Type = VatReconciliationIssueType.PayableMismatch,
                    Severity = IssueSeverity.Critical,
                    Description = $"Chênh lệch VAT phải nộp: Sổ sách {accountingReport.VatPayable:N0} vs Tờ khai {declaration.VatPayable:N0}",
                    Difference = payableDiff
                });
            }

            return new VatReconciliationResult
            {
                Period = period,
                AccountingReport = accountingReport,
                Declaration = declaration,
                IsReconciled = !issues.Any(i => i.Severity == IssueSeverity.Critical),
                Issues = issues,
                ReconciliationDate = DateTime.UtcNow
            };
        }

        private bool IsValidTaxCode(string taxCode)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
                return false;

            // MST có 10 hoặc 14 chữ số
            return System.Text.RegularExpressions.Regex.IsMatch(taxCode.Trim(), @"^\d{10}$|^\d{14}$");
        }
    }

    /// <summary>
    /// Báo cáo VAT
    /// </summary>
    public class VatReport
    {
        public AccountingPeriod Period { get; set; }
        public Money OutputVat { get; set; }
        public Money InputVat { get; set; }
        public Money DeductibleInputVat { get; set; }
        public Money NonDeductibleInputVat { get; set; }
        public Money VatPayable { get; set; }
        public int OutputInvoiceCount { get; set; }
        public int InputInvoiceCount { get; set; }
        public Money TotalOutputAmount { get; set; }
        public Money TotalInputAmount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Tờ khai VAT (từ ngườii dùng nhập)
    /// </summary>
    public class VatDeclaration
    {
        public AccountingPeriod Period { get; set; }
        public Money OutputVat { get; set; }
        public Money InputVat { get; set; }
        public Money VatPayable { get; set; }
        public DateTime DeclarationDate { get; set; }
    }

    /// <summary>
    /// Kết quả đối chiếu VAT
    /// </summary>
    public class VatReconciliationResult
    {
        public AccountingPeriod Period { get; set; }
        public VatReport AccountingReport { get; set; }
        public VatDeclaration Declaration { get; set; }
        public bool IsReconciled { get; set; }
        public List<VatReconciliationIssue> Issues { get; set; } = new();
        public DateTime ReconciliationDate { get; set; }
    }

    public class VatReconciliationIssue
    {
        public VatReconciliationIssueType Type { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Description { get; set; }
        public decimal Difference { get; set; }
    }

    public enum VatReconciliationIssueType
    {
        OutputMismatch,
        InputMismatch,
        PayableMismatch,
        InvoiceGap,
        RateError
    }

    public enum IssueSeverity
    {
        Information,
        Warning,
        Critical
    }
}
