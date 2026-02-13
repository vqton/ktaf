using System;
using System.Collections.Generic;

namespace AccountingERP.Domain.AccountingPrinciples
{
    // Basic ledger/model representations used by simple in-memory engines for TT99 Phase A
    public class AccrualRecord
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string RevenueAccount { get; set; } = string.Empty;
        public string ExpenseAccount { get; set; } = string.Empty;
        public bool Reversed { get; set; }
    }

    public class MonthlyAllocation
    {
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class PrepaidRecord
    {
        public string Id { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Months { get; set; }
        public string AssetAccount { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public List<MonthlyAllocation> Schedule { get; set; } = new();
    }

    public class ProvisionRecord
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime AsOf { get; set; }
    }
}
