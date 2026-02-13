using System;
using System.Collections.Generic;
using AccountingERP.Domain.TaxAuditSimulation;
using Xunit;

namespace AccountingERP.Domain.Tests
{
    public class TaxAuditSimulationTests
    {
        [Fact]
        public void RunSimulation_PassScenario()
        {
            var sim = new TaxAuditSimulator();

            var journals = new List<TaxAuditSimulator.JournalEntry>
            {
                new TaxAuditSimulator.JournalEntry { Id = "J1", DebitAmount = 1000, CreditAmount = 0, VATAmount = 100, IsCashRelated = true },
                new TaxAuditSimulator.JournalEntry { Id = "J2", DebitAmount = 0, CreditAmount = 100, VATAmount = 20, IsCashRelated = true }
            };

            var revenues = new List<TaxAuditSimulator.RevenueRecord>
            {
                new TaxAuditSimulator.RevenueRecord { Id = "R1", Amount = 1100, VATAmount = 120 }
            };

            var bank = new TaxAuditSimulator.BankStatement { Balance = 0m };
            var inventory = new List<TaxAuditSimulator.InventoryRecord>();

            var report = sim.RunSimulation(journals, revenues, bank, inventory);
            Assert.True(report.IsAuditReady);
        }

        [Fact]
        public void RunSimulation_VATMismatch_Fails()
        {
            var sim = new TaxAuditSimulator();
            var journals = new List<TaxAuditSimulator.JournalEntry>
            {
                new TaxAuditSimulator.JournalEntry { Id = "J1", DebitAmount = 1000, CreditAmount = 0, VATAmount = 50, IsCashRelated = true }
            };
            var revenues = new List<TaxAuditSimulator.RevenueRecord>
            {
                new TaxAuditSimulator.RevenueRecord { Id = "R1", Amount = 1000, VATAmount = 100 }
            };
            var bank = new TaxAuditSimulator.BankStatement { Balance = 0m };
            var inventory = new List<TaxAuditSimulator.InventoryRecord>();
            var report = sim.RunSimulation(journals, revenues, bank, inventory);
            Assert.False(report.IsAuditReady);
            Assert.NotEmpty(report.Issues);
        }
    }
}
