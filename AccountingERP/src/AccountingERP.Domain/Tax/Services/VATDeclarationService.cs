using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Invoicing;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.Services
{
    /// <summary>
    /// Domain Service: VAT Declaration Automation
    /// Generates VAT declaration dataset from ledger + invoice data
    /// </summary>
    public interface IVATDeclarationService
    {
        /// <summary>
        /// Generate VAT declaration for period
        /// </summary>
        VATDeclaration GenerateDeclaration(
            AccountingPeriod period,
            IEnumerable<JournalEntry> entries,
            IEnumerable<Invoice> invoices);

        /// <summary>
        /// Validate VAT reconciliation (invoices vs ledger)
        /// </summary>
        ReconciliationResult ValidateReconciliation(
            IEnumerable<JournalEntry> entries,
            IEnumerable<Invoice> invoices);
    }

    /// <summary>
    /// VAT Declaration data structure
    /// </summary>
    public class VATDeclaration
    {
        public AccountingPeriod Period { get; set; } = null!;
        public Money OutputVAT { get; set; } = Money.Zero(Currency.VND);
        public Money DeductibleInputVAT { get; set; } = Money.Zero(Currency.VND);
        public Money NonDeductibleInputVAT { get; set; } = Money.Zero(Currency.VND);
        public Money VATPayable => OutputVAT.Subtract(DeductibleInputVAT);
        public List<VATDeclarationLine> Lines { get; set; } = new();
        public bool IsReconciled { get; set; }
        public List<string> ReconciliationErrors { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class VATDeclarationLine
    {
        public string TaxCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Money Amount { get; set; } = Money.Zero(Currency.VND);
        public int VATRate { get; set; }
        public Money VATAmount { get; set; } = Money.Zero(Currency.VND);
        public LineType Type { get; set; }
    }

    public enum LineType
    {
        OutputVAT = 1,
        DeductibleInputVAT = 2,
        NonDeductibleInputVAT = 3
    }

    public class ReconciliationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public Money InvoiceOutputVAT { get; set; } = Money.Zero(Currency.VND);
        public Money LedgerOutputVAT { get; set; } = Money.Zero(Currency.VND);
        public decimal Variance => Math.Abs(OutputVATVariance.Amount);
        public Money OutputVATVariance => InvoiceOutputVAT.Subtract(LedgerOutputVAT);
    }

    /// <summary>
    /// Implementation
    /// </summary>
    public class VATDeclarationService : IVATDeclarationService
    {
        public VATDeclaration GenerateDeclaration(
            AccountingPeriod period,
            IEnumerable<JournalEntry> entries,
            IEnumerable<Invoice> invoices)
        {
            var declaration = new VATDeclaration
            {
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };

            // Validate reconciliation first
            var reconciliation = ValidateReconciliation(entries, invoices);
            declaration.IsReconciled = reconciliation.IsValid;
            declaration.ReconciliationErrors = reconciliation.Errors;

            if (!reconciliation.IsValid)
            {
                return declaration;
            }

            // Calculate Output VAT from invoices
            var outputInvoices = invoices.Where(i => 
                i.Status == Invoicing.InvoiceStatus.Issued && 
                i.Type == Invoicing.InvoiceType.Sales);

            foreach (var invoice in outputInvoices)
            {
                declaration.OutputVAT = declaration.OutputVAT.Add(invoice.VatAmount);
                
                declaration.Lines.Add(new VATDeclarationLine
                {
                    TaxCode = invoice.CustomerTaxCode,
                    Description = $"Hóa đơn {invoice.InvoiceNumber}",
                    Amount = invoice.SubTotal,
                    VATRate = invoice.VatRate,
                    VATAmount = invoice.VatAmount,
                    Type = LineType.OutputVAT
                });
            }

            // Calculate Input VAT from journal entries
            var postedEntries = entries.Where(e => e.IsPosted);
            
            foreach (var entry in postedEntries)
            {
                // Deductible Input VAT (TK 1331)
                var deductibleVatLines = entry.Lines.Where(l => 
                    l.AccountCode.StartsWith("1331") && l.IsDebit);
                
                foreach (var line in deductibleVatLines)
                {
                    declaration.DeductibleInputVAT = declaration.DeductibleInputVAT.Add(line.GetMoney());
                    
                    declaration.Lines.Add(new VATDeclarationLine
                    {
                        TaxCode = "1331",
                        Description = entry.Description,
                        Amount = line.GetMoney(),
                        VATRate = 0, // Already VAT amount
                        VATAmount = line.GetMoney(),
                        Type = LineType.DeductibleInputVAT
                    });
                }

                // Non-deductible Input VAT (TK 1332)
                var nonDeductibleVatLines = entry.Lines.Where(l => 
                    l.AccountCode.StartsWith("1332") && l.IsDebit);
                
                foreach (var line in nonDeductibleVatLines)
                {
                    declaration.NonDeductibleInputVAT = declaration.NonDeductibleInputVAT.Add(line.GetMoney());
                    
                    declaration.Lines.Add(new VATDeclarationLine
                    {
                        TaxCode = "1332",
                        Description = entry.Description,
                        Amount = line.GetMoney(),
                        VATRate = 0,
                        VATAmount = line.GetMoney(),
                        Type = LineType.NonDeductibleInputVAT
                    });
                }
            }

            return declaration;
        }

        public ReconciliationResult ValidateReconciliation(
            IEnumerable<JournalEntry> entries,
            IEnumerable<Invoice> invoices)
        {
            var result = new ReconciliationResult();

            // Calculate Output VAT from invoices
            var outputInvoices = invoices.Where(i => 
                i.Status == Invoicing.InvoiceStatus.Issued && 
                i.Type == Invoicing.InvoiceType.Sales);
            
            result.InvoiceOutputVAT = Money.Create(
                outputInvoices.Sum(i => i.VatAmount.Amount), Currency.VND);

            // Calculate Output VAT from ledger (TK 33311)
            var postedEntries = entries.Where(e => e.IsPosted);
            var ledgerOutputVat = postedEntries
                .SelectMany(e => e.Lines)
                .Where(l => l.AccountCode == "33311" && l.IsCredit)
                .Sum(l => l.GetMoney().Amount);
            
            result.LedgerOutputVAT = Money.Create(ledgerOutputVat, Currency.VND);

            // Check variance
            var variance = Math.Abs(result.InvoiceOutputVAT.Amount - result.LedgerOutputVAT.Amount);
            
            if (variance > 1000) // Allow 1000 VND tolerance
            {
                result.IsValid = false;
                result.Errors.Add(
                    $"Chênh lệch VAT đầu ra: Hóa đơn {result.InvoiceOutputVAT.Amount:N0} " +
                    $"vs Sổ kế toán {result.LedgerOutputVAT.Amount:N0} " +
                    $"(chênh lệch {variance:N0} VND)");
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }
    }
}
