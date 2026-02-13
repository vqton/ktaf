using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.FinancialReporting;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.FinancialReporting.Snapshot
{
    /// <summary>
    /// Domain Service: Snapshot Reporting (Point-in-time reconstruction)
    /// Reconstruct financial position at historical date
    /// </summary>
    public interface ISnapshotReportingService
    {
        /// <summary>
        /// Generate financial snapshot at specific date
        /// </summary>
        FinancialSnapshot GenerateSnapshot(DateTime asOfDate, IEnumerable<JournalEntry> allEntries);
        
        /// <summary>
        /// Validate snapshot integrity
        /// </summary>
        bool ValidateSnapshotIntegrity(FinancialSnapshot snapshot);
    }

    /// <summary>
    /// Financial Snapshot at a point in time
    /// </summary>
    public class FinancialSnapshot
    {
        public Guid Id { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        
        // Trial Balance at snapshot date
        public List<SnapshotAccountBalance> AccountBalances { get; set; } = new();
        
        // Summary
        public Money TotalAssets { get; set; } = Money.Zero(Currency.VND);
        public Money TotalLiabilities { get; set; } = Money.Zero(Currency.VND);
        public Money TotalEquity { get; set; } = Money.Zero(Currency.VND);
        
        // Validation
        public bool IsBalanced => TotalAssets == TotalLiabilities.Add(TotalEquity);
        public List<string> ValidationErrors { get; set; } = new();
        
        // Metadata
        public int EntryCount { get; set; }
        public int ReversalCount { get; set; }
    }

    public class SnapshotAccountBalance
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public Money DebitBalance { get; set; } = Money.Zero(Currency.VND);
        public Money CreditBalance { get; set; } = Money.Zero(Currency.VND);
        public Money NetBalance => DebitBalance.Subtract(CreditBalance);
    }

    /// <summary>
    /// Implementation
    /// </summary>
    public class SnapshotReportingService : ISnapshotReportingService
    {
        public FinancialSnapshot GenerateSnapshot(DateTime asOfDate, IEnumerable<JournalEntry> allEntries)
        {
            var snapshot = new FinancialSnapshot
            {
                Id = Guid.NewGuid(),
                AsOfDate = asOfDate.Date,
                GeneratedAt = DateTime.UtcNow,
                EntryCount = 0,
                ReversalCount = 0
            };

            // Filter entries up to snapshot date
            var relevantEntries = allEntries
                .Where(e => e.IsPosted && e.EntryDate.Date <= asOfDate.Date)
                .OrderBy(e => e.EntryDate)
                .ThenBy(e => e.EntryNumber)
                .ToList();

            snapshot.EntryCount = relevantEntries.Count;

            // Calculate account balances
            var accountBalances = new Dictionary<string, SnapshotAccountBalance>();

            foreach (var entry in relevantEntries)
            {
                // Check if this is a reversal by checking Description or other indicators
                if (entry.Description.ToUpper().Contains("ĐẢO NGƯỢC") || 
                    entry.Description.ToUpper().Contains("REVERSAL"))
                {
                    snapshot.ReversalCount++;
                }

                foreach (var line in entry.Lines)
                {
                    if (!accountBalances.ContainsKey(line.AccountCode))
                    {
                        accountBalances[line.AccountCode] = new SnapshotAccountBalance
                        {
                            AccountCode = line.AccountCode,
                            AccountName = line.Description
                        };
                    }

                    var balance = accountBalances[line.AccountCode];
                    
                    if (line.IsDebit)
                    {
                        balance.DebitBalance = balance.DebitBalance.Add(line.GetMoney());
                    }
                    else
                    {
                        balance.CreditBalance = balance.CreditBalance.Add(line.GetMoney());
                    }
                }
            }

            snapshot.AccountBalances = accountBalances.Values.OrderBy(a => a.AccountCode).ToList();

            // Calculate totals (simplified classification)
            foreach (var balance in snapshot.AccountBalances)
            {
                var accountCode = balance.AccountCode;
                var netBalance = balance.NetBalance;

                // Assets: Group 1 accounts (debit balance)
                if (accountCode.StartsWith("1") && netBalance.Amount > 0)
                {
                    snapshot.TotalAssets = snapshot.TotalAssets.Add(netBalance);
                }
                // Liabilities: Group 3 accounts (credit balance)
                else if (accountCode.StartsWith("3") && netBalance.Amount < 0)
                {
                    snapshot.TotalLiabilities = snapshot.TotalLiabilities.Add(netBalance.Multiply(-1));
                }
                // Equity: Group 4 accounts (credit balance)
                else if (accountCode.StartsWith("4") && netBalance.Amount < 0)
                {
                    snapshot.TotalEquity = snapshot.TotalEquity.Add(netBalance.Multiply(-1));
                }
            }

            // Validate integrity
            if (!ValidateSnapshotIntegrity(snapshot))
            {
                snapshot.ValidationErrors.Add("Snapshot không cân bằng: Assets != Liabilities + Equity");
            }

            return snapshot;
        }

        public bool ValidateSnapshotIntegrity(FinancialSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            // Check balance sheet equation: Assets = Liabilities + Equity
            var expectedAssets = snapshot.TotalLiabilities.Add(snapshot.TotalEquity);
            var variance = Math.Abs(snapshot.TotalAssets.Amount - expectedAssets.Amount);
            
            // Allow small rounding tolerance
            return variance < 1000;
        }

        /// <summary>
        /// Compare two snapshots to detect changes
        /// </summary>
        public SnapshotComparison CompareSnapshots(FinancialSnapshot before, FinancialSnapshot after)
        {
            var comparison = new SnapshotComparison
            {
                BeforeDate = before.AsOfDate,
                AfterDate = after.AsOfDate,
                AssetChange = after.TotalAssets.Subtract(before.TotalAssets),
                LiabilityChange = after.TotalLiabilities.Subtract(before.TotalLiabilities),
                EquityChange = after.TotalEquity.Subtract(before.TotalEquity)
            };

            // Find changed accounts
            var beforeAccounts = before.AccountBalances.ToDictionary(a => a.AccountCode);
            var afterAccounts = after.AccountBalances.ToDictionary(a => a.AccountCode);

            foreach (var afterAccount in after.AccountBalances)
            {
                if (beforeAccounts.TryGetValue(afterAccount.AccountCode, out var beforeAccount))
                {
                    var change = afterAccount.NetBalance.Subtract(beforeAccount.NetBalance);
                    if (change.Amount != 0)
                    {
                        comparison.AccountChanges.Add(new AccountChange
                        {
                            AccountCode = afterAccount.AccountCode,
                            AccountName = afterAccount.AccountName,
                            BeforeBalance = beforeAccount.NetBalance,
                            AfterBalance = afterAccount.NetBalance,
                            Change = change
                        });
                    }
                }
                else
                {
                    // New account
                    comparison.AccountChanges.Add(new AccountChange
                    {
                        AccountCode = afterAccount.AccountCode,
                        AccountName = afterAccount.AccountName,
                        BeforeBalance = Money.Zero(Currency.VND),
                        AfterBalance = afterAccount.NetBalance,
                        Change = afterAccount.NetBalance
                    });
                }
            }

            return comparison;
        }
    }

    public class SnapshotComparison
    {
        public DateTime BeforeDate { get; set; }
        public DateTime AfterDate { get; set; }
        public Money AssetChange { get; set; } = Money.Zero(Currency.VND);
        public Money LiabilityChange { get; set; } = Money.Zero(Currency.VND);
        public Money EquityChange { get; set; } = Money.Zero(Currency.VND);
        public List<AccountChange> AccountChanges { get; set; } = new();
    }

    public class AccountChange
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public Money BeforeBalance { get; set; } = Money.Zero(Currency.VND);
        public Money AfterBalance { get; set; } = Money.Zero(Currency.VND);
        public Money Change { get; set; } = Money.Zero(Currency.VND);
    }
}
