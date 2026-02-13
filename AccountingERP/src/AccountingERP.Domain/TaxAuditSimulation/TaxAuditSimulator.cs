using System;
using System.Collections.Generic;

namespace AccountingERP.Domain.TaxAuditSimulation
{
    // Lightweight tax audit simulator to stress TT99 compliance checks
    public class TaxAuditSimulator
    {
        public TaxAuditReport RunSimulation(IEnumerable<JournalEntry> journals,
                                          IEnumerable<RevenueRecord> revenues,
                                          BankStatement bank,
                                          IEnumerable<InventoryRecord> inventory)
        {
            var report = new TaxAuditReport();

            // 1) VAT consistency check: sum VAT in journals vs revenue records
            decimal totalVatFromJournals = 0m;
            foreach (var j in journals) totalVatFromJournals += j.VATAmount;

            decimal totalVatFromRevenues = 0m;
            foreach (var r in revenues) totalVatFromRevenues += r.VATAmount;

            if (totalVatFromJournals != totalVatFromRevenues)
            {
                report.AddIssue($"VAT mismatch: journals {totalVatFromJournals} vs revenues {totalVatFromRevenues}");
            }

            // 2) Bank reconciliation check: simplistic comparison between journal cash activity and bank balance
            decimal netCashFromJournals = 0m;
            foreach (var j in journals)
            {
                if (j.IsCashRelated)
                    netCashFromJournals += j.DebitAmount - j.CreditAmount;
            }
            decimal bankBalance = bank?.Balance ?? 0m;
            if (Math.Abs(netCashFromJournals - bankBalance) > 0.01m)
            {
                report.AddIssue($"Bank reconciliation mismatch: net cash {netCashFromJournals} vs bank balance {bankBalance}");
            }

            // 3) Inventory/COGS sanity: basic value check
            decimal inventoryValue = 0m;
            foreach (var it in inventory) inventoryValue += it.Value;
            if (inventoryValue < 0)
            {
                report.AddIssue("Inventory value anomaly: negative total value");
            }

            report.IsAuditReady = report.Issues.Count == 0;
            report.Summary = report.IsAuditReady ? "PASS" : "FAIL";
            return report;
        }
    }

    // Domain models for the simulator (simple, lightweight only)
    public class JournalEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public bool IsCashRelated { get; set; }
        public decimal VATAmount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class RevenueRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal VATAmount { get; set; }
    }

    public class BankStatement
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public string BankName { get; set; } = string.Empty;
    }

    public class InventoryRecord
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TaxAuditReport
    {
        public bool IsAuditReady { get; set; }
        public string Summary { get; set; } = "NOT RUN";
        public List<string> Issues { get; } = new List<string>();

        internal void AddIssue(string message) => Issues.Add(message);
    }
}
