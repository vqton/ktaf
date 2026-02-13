using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Tax;
using AccountingERP.Domain.Tax.Services;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.FraudDetection
{
    /// <summary>
    /// Domain Service: Phát hiện gian lận và rủi ro
    /// Phát hiện các dấu hiệu bất thường theo thực tiễn thanh tra thuế
    /// </summary>
    public interface IFraudDetectionService
    {
        FraudDetectionReport AnalyzePeriod(AccountingPeriod period, 
            IEnumerable<JournalEntry> entries, 
            IEnumerable<VatInvoice> invoices);
        IEnumerable<FraudAlert> DetectVatMismatch(IEnumerable<JournalEntry> entries, VatReport vatReport);
        IEnumerable<FraudAlert> DetectRevenueWithoutInvoice(IEnumerable<JournalEntry> entries, IEnumerable<VatInvoice> invoices);
        IEnumerable<FraudAlert> DetectDuplicatePayments(IEnumerable<JournalEntry> entries);
        IEnumerable<FraudAlert> DetectRoundNumberTransactions(IEnumerable<JournalEntry> entries);
        IEnumerable<FraudAlert> DetectExpenseInflation(IEnumerable<JournalEntry> entries, decimal historicalAverage);
        IEnumerable<FraudAlert> DetectCashAnomalies(IEnumerable<JournalEntry> entries);
        IEnumerable<FraudAlert> DetectInvoiceGaps(IEnumerable<VatInvoice> invoices);
    }

    /// <summary>
    /// Implementation of Fraud Detection Service
    /// </summary>
    public class FraudDetectionService : IFraudDetectionService
    {
        /// <summary>
        /// Phân tích toàn diện một kỳ kế toán
        /// </summary>
        public FraudDetectionReport AnalyzePeriod(
            AccountingPeriod period, 
            IEnumerable<JournalEntry> entries, 
            IEnumerable<VatInvoice> invoices)
        {
            var alerts = new List<FraudAlert>();
            var postedEntries = entries.Where(e => e.IsPosted);

            // 1. VAT Mismatch Detection
            // This would require VAT report, simplified here
            // alerts.AddRange(DetectVatMismatch(postedEntries, vatReport));

            // 2. Revenue without Invoice
            alerts.AddRange(DetectRevenueWithoutInvoice(postedEntries, invoices));

            // 3. Duplicate Payments
            alerts.AddRange(DetectDuplicatePayments(postedEntries));

            // 4. Round Number Transactions
            alerts.AddRange(DetectRoundNumberTransactions(postedEntries));

            // 5. Expense Inflation (need historical data)
            // alerts.AddRange(DetectExpenseInflation(postedEntries, historicalAverage));

            // 6. Cash Anomalies
            alerts.AddRange(DetectCashAnomalies(postedEntries));

            // 7. Invoice Gaps
            alerts.AddRange(DetectInvoiceGaps(invoices));

            // 8. Input VAT Spike
            alerts.AddRange(DetectInputVatSpike(invoices, period));

            // 9. Revenue Decline with Expense Rise
            alerts.AddRange(DetectRevenueExpenseMismatch(postedEntries));

            // 10. Concentration Risk
            alerts.AddRange(DetectConcentrationRisk(postedEntries));

            return new FraudDetectionReport
            {
                Period = period,
                AnalysisDate = DateTime.UtcNow,
                TotalAlerts = alerts.Count,
                CriticalAlerts = alerts.Count(a => a.Severity == FraudSeverity.Critical),
                HighAlerts = alerts.Count(a => a.Severity == FraudSeverity.High),
                MediumAlerts = alerts.Count(a => a.Severity == FraudSeverity.Medium),
                LowAlerts = alerts.Count(a => a.Severity == FraudSeverity.Low),
                Alerts = alerts.OrderByDescending(a => a.Severity).ToList()
            };
        }

        /// <summary>
        /// Phát hiện chênh lệch VAT
        /// </summary>
        public IEnumerable<FraudAlert> DetectVatMismatch(IEnumerable<JournalEntry> entries, VatReport vatReport)
        {
            var alerts = new List<FraudAlert>();
            
            // Calculate VAT from entries
            var entryOutputVat = entries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode == "33311" && l.IsCredit)
                .Sum(l => l.Amount);

            var entryInputVat = entries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("133") && l.IsDebit)
                .Sum(l => l.Amount);

            // Compare with invoice-based VAT report
            var outputDiff = Math.Abs(entryOutputVat - vatReport.OutputVat.Amount);
            var inputDiff = Math.Abs(entryInputVat - vatReport.InputVat.Amount);

            const decimal TOLERANCE = 1000m; // 1,000 VND

            if (outputDiff > TOLERANCE)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "VAT-001",
                    Title = "Chênh lệch VAT đầu ra",
                    Description = $"VAT đầu ra từ bút toán: {entryOutputVat:N0} VND, từ hóa đơn: {vatReport.OutputVat.Amount:N0} VND. Chênh lệch: {outputDiff:N0} VND",
                    Severity = FraudSeverity.Critical,
                    Category = FraudCategory.TaxEvasion,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Kiểm tra ngay các hóa đơn chưa được ghi nhận hoặc bút toán không có hóa đơn"
                });
            }

            if (inputDiff > TOLERANCE)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "VAT-002",
                    Title = "Chênh lệch VAT đầu vào",
                    Description = $"VAT đầu vào từ bút toán: {entryInputVat:N0} VND, từ hóa đơn: {vatReport.InputVat.Amount:N0} VND. Chênh lệch: {inputDiff:N0} VND",
                    Severity = FraudSeverity.Critical,
                    Category = FraudCategory.TaxEvasion,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Kiểm tra các khoản VAT đầu vào không có hóa đơn hợp lệ"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện doanh thu không có hóa đơn
        /// </summary>
        public IEnumerable<FraudAlert> DetectRevenueWithoutInvoice(
            IEnumerable<JournalEntry> entries, 
            IEnumerable<VatInvoice> invoices)
        {
            var alerts = new List<FraudAlert>();
            
            var revenueEntries = entries
                .Where(e => e.Lines.Any(l => l.AccountCode.StartsWith("5") && l.IsCredit))
                .ToList();

            var totalRevenue = revenueEntries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("5") && l.IsCredit)
                .Sum(l => l.Amount);

            var invoicedRevenue = invoices
                .Where(i => i.Type == InvoiceType.Output && i.IsValid)
                .Sum(i => i.TotalAmount.Amount);

            var gap = totalRevenue - invoicedRevenue;
            var gapPercentage = totalRevenue > 0 ? gap / totalRevenue : 0;

            // Alert if gap > 0.1% of revenue
            if (gap > 200_000_000m && gapPercentage > 0.001m)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "REV-001",
                    Title = "Doanh thu chưa xuất hóa đơn",
                    Description = $"Tổng doanh thu: {totalRevenue:N0} VND. Doanh thu có hóa đơn: {invoicedRevenue:N0} VND. Chênh lệch: {gap:N0} VND ({gapPercentage:P2})",
                    Severity = FraudSeverity.Critical,
                    Category = FraudCategory.RevenueSuppression,
                    Amount = gap,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Kiểm tra ngay các khoản doanh thu chưa xuất hóa đơn GTGT"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện thanh toán trùng lặp
        /// </summary>
        public IEnumerable<FraudAlert> DetectDuplicatePayments(IEnumerable<JournalEntry> entries)
        {
            var alerts = new List<FraudAlert>();
            var postedEntries = entries.Where(e => e.IsPosted).ToList();

            // Group by amount, vendor, and date proximity
            var paymentEntries = postedEntries
                .Where(e => e.Lines.Any(l => l.AccountCode.StartsWith("3") && l.IsDebit))
                .ToList();

            for (int i = 0; i < paymentEntries.Count; i++)
            {
                for (int j = i + 1; j < paymentEntries.Count; j++)
                {
                    var entry1 = paymentEntries[i];
                    var entry2 = paymentEntries[j];

                    var daysDiff = Math.Abs((entry1.EntryDate - entry2.EntryDate).Days);
                    var amount1 = entry1.TotalDebit;
                    var amount2 = entry2.TotalDebit;

                    // Same amount, within 7 days, possible duplicate
                    if (amount1 == amount2 && daysDiff <= 7 && amount1 > 10_000_000m)
                    {
                        alerts.Add(new FraudAlert
                        {
                            Code = "PAY-001",
                            Title = "Nghi vấn thanh toán trùng lặp",
                            Description = $"Hai bút toán cùng số tiền {amount1:N0} VND trong vòng {daysDiff} ngày. BT1: {entry1.EntryNumber}, BT2: {entry2.EntryNumber}",
                            Severity = FraudSeverity.High,
                            Category = FraudCategory.DuplicatePayment,
                            Amount = amount1,
                            RelatedEntryIds = new List<Guid> { entry1.Id, entry2.Id },
                            DetectionDate = DateTime.UtcNow,
                            RecommendedAction = "Kiểm tra xem có phải thanh toán trùng lặp hay không"
                        });
                    }
                }
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện giao dịch số tròn bất thường
        /// </summary>
        public IEnumerable<FraudAlert> DetectRoundNumberTransactions(IEnumerable<JournalEntry> entries)
        {
            var alerts = new List<FraudAlert>();
            var postedEntries = entries.Where(e => e.IsPosted).ToList();

            var roundNumberEntries = postedEntries
                .Where(e => IsRoundNumber(e.TotalDebit) && e.TotalDebit >= 100_000_000m)
                .ToList();

            if (roundNumberEntries.Count >= 5)
            {
                var percentage = (double)roundNumberEntries.Count / postedEntries.Count;
                
                alerts.Add(new FraudAlert
                {
                    Code = "PAT-001",
                    Title = "Tỷ lệ giao dịch số tròn cao bất thường",
                    Description = $"Có {roundNumberEntries.Count} giao dịch số tròn (>= 100 triệu) chiếm {percentage:P1} tổng số giao dịch",
                    Severity = FraudSeverity.Medium,
                    Category = FraudCategory.UnusualPattern,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Kiểm tra các giao dịch số tròn có thể là ước tính thay vì giá trị thực tế"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện chi phí tăng đột biến
        /// </summary>
        public IEnumerable<FraudAlert> DetectExpenseInflation(
            IEnumerable<JournalEntry> entries, 
            decimal historicalAverage)
        {
            var alerts = new List<FraudAlert>();
            
            var currentMonthExpense = entries
                .Where(e => e.IsPosted)
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("6") && l.IsDebit)
                .Sum(l => l.Amount);

            if (historicalAverage > 0 && currentMonthExpense > historicalAverage * 1.5m)
            {
                var increasePercentage = (currentMonthExpense - historicalAverage) / historicalAverage;
                
                alerts.Add(new FraudAlert
                {
                    Code = "EXP-001",
                    Title = "Chi phí tăng đột biến",
                    Description = $"Chi phí tháng này: {currentMonthExpense:N0} VND, trung bình: {historicalAverage:N0} VND. Tăng {increasePercentage:P0}",
                    Severity = FraudSeverity.High,
                    Category = FraudCategory.ExpenseInflation,
                    Amount = currentMonthExpense - historicalAverage,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Phân tích chi tiết các khoản chi phí tăng đột biến"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện bất thường về tiền mặt
        /// </summary>
        public IEnumerable<FraudAlert> DetectCashAnomalies(IEnumerable<JournalEntry> entries)
        {
            var alerts = new List<FraudAlert>();

            // Calculate cash position (account 111)
            var cashEntries = entries
                .Where(e => e.IsPosted)
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("111"))
                .ToList();

            var cashDebits = cashEntries.Where(l => l.IsDebit).Sum(l => l.Amount);
            var cashCredits = cashEntries.Where(l => l.IsCredit).Sum(l => l.Amount);
            var cashBalance = cashDebits - cashCredits;

            // Alert if cash balance > 50M at end of day (regulatory limit)
            if (cashBalance > 50_000_000m)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "CASH-001",
                    Title = "Dư tiền mặt vượt ngưỡng",
                    Description = $"Số dư tiền mặt: {cashBalance:N0} VND vượt quá ngưỡng quy định 50 triệu đồng",
                    Severity = FraudSeverity.High,
                    Category = FraudCategory.CashAnomaly,
                    Amount = cashBalance - 50_000_000m,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Chuyển tiền mặt vượt ngưỡng vào tài khoản ngân hàng"
                });
            }

            // Alert if cash withdrawals > 20% of revenue
            var revenue = entries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("5") && l.IsCredit)
                .Sum(l => l.Amount);

            if (revenue > 0 && cashCredits > revenue * 0.2m)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "CASH-002",
                    Title = "Rút tiền mặt bất thường",
                    Description = $"Tổng chi tiền mặt: {cashCredits:N0} VND chiếm {(cashCredits/revenue):P1} doanh thu",
                    Severity = FraudSeverity.Medium,
                    Category = FraudCategory.CashAnomaly,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Kiểm tra mục đích rút tiền mặt lớn"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện khoảng cách số hóa đơn
        /// </summary>
        public IEnumerable<FraudAlert> DetectInvoiceGaps(IEnumerable<VatInvoice> invoices)
        {
            var alerts = new List<FraudAlert>();
            
            var validInvoices = invoices
                .Where(i => i.IsValid && i.Type == InvoiceType.Output)
                .OrderBy(i => i.InvoiceNumber)
                .ToList();

            for (int i = 1; i < validInvoices.Count; i++)
            {
                var current = validInvoices[i];
                var previous = validInvoices[i - 1];

                // Try to parse invoice numbers
                if (int.TryParse(current.InvoiceNumber, out int currentNum) &&
                    int.TryParse(previous.InvoiceNumber, out int prevNum))
                {
                    var gap = currentNum - prevNum - 1;
                    
                    if (gap > 0)
                    {
                        alerts.Add(new FraudAlert
                        {
                            Code = "INV-001",
                            Title = "Khoảng cách số hóa đơn",
                            Description = $"Thiếu {gap} số hóa đơn giữa {previous.InvoiceNumber} và {current.InvoiceNumber}",
                            Severity = gap > 5 ? FraudSeverity.Critical : FraudSeverity.Medium,
                            Category = FraudCategory.InvoiceManipulation,
                            DetectionDate = DateTime.UtcNow,
                            RecommendedAction = "Giải trình ngay các số hóa đơn bị thiếu"
                        });
                    }
                }
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện VAT đầu vào tăng đột biến
        /// </summary>
        private IEnumerable<FraudAlert> DetectInputVatSpike(IEnumerable<VatInvoice> invoices, AccountingPeriod currentPeriod)
        {
            var alerts = new List<FraudAlert>();
            
            // This would compare with previous periods
            // Simplified implementation
            var currentInputVat = invoices
                .Where(i => i.Type == InvoiceType.Input && i.IsValid)
                .Sum(i => i.VatAmount.Amount);

            if (currentInputVat > 300_000_000m)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "VAT-003",
                    Title = "VAT đầu vào khấu trừ dồn tích lớn",
                    Description = $"VAT đầu vào tích lũy: {currentInputVat:N0} VND. Có thể đề nghị hoàn thuế",
                    Severity = FraudSeverity.Medium,
                    Category = FraudCategory.VatAnomaly,
                    Amount = currentInputVat,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Xem xét nộp đơn hoàn thuế GTGT"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện doanh thu giảm nhưng chi phí tăng
        /// </summary>
        private IEnumerable<FraudAlert> DetectRevenueExpenseMismatch(IEnumerable<JournalEntry> entries)
        {
            var alerts = new List<FraudAlert>();
            
            var revenue = entries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("5") && l.IsCredit)
                .Sum(l => l.Amount);

            var expenses = entries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode.StartsWith("6") && l.IsDebit)
                .Sum(l => l.Amount);

            // Alert if expenses > 95% of revenue
            if (revenue > 0 && expenses > revenue * 0.95m)
            {
                alerts.Add(new FraudAlert
                {
                    Code = "MARG-001",
                    Title = "Biên lợi nhuận rất thấp",
                    Description = $"Doanh thu: {revenue:N0} VND, Chi phí: {expenses:N0} VND. Tỷ lệ chi phí: {(expenses/revenue):P1}",
                    Severity = FraudSeverity.High,
                    Category = FraudCategory.ProfitManipulation,
                    DetectionDate = DateTime.UtcNow,
                    RecommendedAction = "Phân tích cơ cấu chi phí và đánh giá tính hợp lý"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Phát hiện rủi ro tập trung
        /// </summary>
        private IEnumerable<FraudAlert> DetectConcentrationRisk(IEnumerable<JournalEntry> entries)
        {
            var alerts = new List<FraudAlert>();
            
            // Group by customer/supplier
            var customerAmounts = entries
                .SelectMany(e => e.Lines)
                .Where(l => !string.IsNullOrEmpty(l.CustomerCode) && l.IsCredit)
                .GroupBy(l => l.CustomerCode)
                .Select(g => new { Customer = g.Key, Amount = g.Sum(l => l.Amount) })
                .ToList();

            var totalRevenue = customerAmounts.Sum(c => c.Amount);

            foreach (var customer in customerAmounts)
            {
                var percentage = totalRevenue > 0 ? customer.Amount / totalRevenue : 0;
                
                if (percentage > 0.30m) // > 30% from single customer
                {
                    alerts.Add(new FraudAlert
                    {
                        Code = "RISK-001",
                        Title = "Phụ thuộc vào khách hàng lớn",
                        Description = $"Khách hàng {customer.Customer} chiếm {percentage:P0} tổng doanh thu ({customer.Amount:N0} VND)",
                        Severity = FraudSeverity.Medium,
                        Category = FraudCategory.ConcentrationRisk,
                        Amount = customer.Amount,
                        DetectionDate = DateTime.UtcNow,
                        RecommendedAction = "Đa dạng hóa khách hàng để giảm rủi ro"
                    });
                }
            }

            return alerts;
        }

        private bool IsRoundNumber(decimal amount)
        {
            // Check if amount is round (ends with 000000)
            return amount % 1_000_000m == 0;
        }
    }

    /// <summary>
    /// Báo cáo phát hiện gian lận
    /// </summary>
    public class FraudDetectionReport
    {
        public AccountingPeriod Period { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int HighAlerts { get; set; }
        public int MediumAlerts { get; set; }
        public int LowAlerts { get; set; }
        public List<FraudAlert> Alerts { get; set; } = new();

        public bool HasCriticalIssues => CriticalAlerts > 0;
        public bool RequiresImmediateAction => CriticalAlerts > 0 || HighAlerts > 0;
    }

    /// <summary>
    /// Cảnh báo gian lận
    /// </summary>
    public class FraudAlert
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FraudSeverity Severity { get; set; }
        public FraudCategory Category { get; set; }
        public decimal? Amount { get; set; }
        public List<Guid> RelatedEntryIds { get; set; } = new();
        public DateTime DetectionDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public enum FraudSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum FraudCategory
    {
        TaxEvasion,
        RevenueSuppression,
        ExpenseInflation,
        DuplicatePayment,
        CashAnomaly,
        InvoiceManipulation,
        UnusualPattern,
        VatAnomaly,
        ProfitManipulation,
        ConcentrationRisk
    }
}
