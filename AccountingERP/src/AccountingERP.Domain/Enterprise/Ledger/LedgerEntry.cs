using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AccountingERP.Domain.Entities;

namespace AccountingERP.Domain.Enterprise.Ledger
{
    /// <summary>
    /// Enterprise-grade immutable ledger entry with SHA256 hash chain
    /// Ensures tamper-evident accounting records
    /// </summary>
    public class LedgerEntry
    {
        public Guid Id { get; private set; }
        public Guid JournalEntryId { get; private set; }
        public string EntryNumber { get; private set; } = string.Empty;
        public string Hash { get; private set; } = string.Empty;
        public string? PreviousHash { get; private set; }
        public long SequenceNumber { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string DataSnapshot { get; private set; } = string.Empty;
        public string CreatedBy { get; private set; } = string.Empty;

        private static long _sequenceCounter = 0;
        private static readonly object _lock = new object();

        private LedgerEntry() { }

        /// <summary>
        /// Create a new immutable ledger entry with SHA256 hash
        /// </summary>
        public static LedgerEntry Create(JournalEntry entry, string? previousHash, string createdBy = "system")
        {
            lock (_lock)
            {
                _sequenceCounter++;
            }

            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                JournalEntryId = entry.Id,
                EntryNumber = entry.EntryNumber,
                PreviousHash = previousHash,
                SequenceNumber = _sequenceCounter,
                Timestamp = DateTime.UtcNow,
                CreatedBy = createdBy,
                DataSnapshot = SerializeEntry(entry)
            };

            // Generate hash after all fields are set
            ledgerEntry.Hash = ledgerEntry.CalculateHash();

            return ledgerEntry;
        }

        /// <summary>
        /// Calculate SHA256 hash of this entry
        /// </summary>
        public string CalculateHash()
        {
            var data = $"{SequenceNumber}|{Timestamp:O}|{JournalEntryId}|{EntryNumber}|{PreviousHash ?? "null"}|{DataSnapshot}|{CreatedBy}";
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToHexString(bytes).ToLower();
            }
        }

        /// <summary>
        /// Verify the integrity of this entry
        /// </summary>
        public bool VerifyIntegrity()
        {
            var calculatedHash = CalculateHash();
            return Hash.Equals(calculatedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Serialize journal entry for hash calculation
        /// </summary>
        private static string SerializeEntry(JournalEntry entry)
        {
            var data = new
            {
                entry.Id,
                entry.EntryNumber,
                entry.OriginalDocumentNumber,
                entry.EntryDate,
                entry.OriginalDocumentDate,
                entry.Description,
                entry.Reference,
                entry.IsPosted,
                entry.TotalDebit,
                entry.TotalCredit,
                Lines = entry.Lines.Select(l => new
                {
                    l.AccountCode,
                    l.DebitAmount,
                    l.CreditAmount,
                    l.Description
                }).ToList()
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        /// <summary>
        /// Reset sequence counter (for testing only)
        /// </summary>
        public static void ResetSequenceCounter()
        {
            lock (_lock)
            {
                _sequenceCounter = 0;
            }
        }
    }

    /// <summary>
    /// Validator for ledger integrity and hash chain verification
    /// </summary>
    public static class LedgerIntegrityValidator
    {
        /// <summary>
        /// Verify the entire hash chain
        /// </summary>
        public static bool VerifyChain(List<LedgerEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return true;

            // Sort by sequence number
            var sorted = entries.OrderBy(e => e.SequenceNumber).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var entry = sorted[i];

                // Verify entry integrity
                if (!entry.VerifyIntegrity())
                    return false;

                // Verify chain linkage (except for first entry)
                if (i > 0)
                {
                    var previousEntry = sorted[i - 1];
                    if (entry.PreviousHash != previousEntry.Hash)
                        return false;
                }
                else
                {
                    // First entry should have null previous hash
                    if (entry.PreviousHash != null)
                        return false;
                }

                // Verify sequence continuity
                if (entry.SequenceNumber != i + 1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Detect tampering in the ledger
        /// </summary>
        public static TamperingResult DetectTampering(List<LedgerEntry> entries)
        {
            var result = new TamperingResult();

            if (entries == null || entries.Count == 0)
                return result;

            var sorted = entries.OrderBy(e => e.SequenceNumber).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var entry = sorted[i];

                // Check individual entry integrity
                if (!entry.VerifyIntegrity())
                {
                    result.HasTampering = true;
                    result.TamperedIndices.Add(i);
                    result.Details.Add($"Entry {i} (Seq: {entry.SequenceNumber}) has invalid hash");
                }

                // Check chain linkage
                if (i > 0)
                {
                    var previousEntry = sorted[i - 1];
                    if (entry.PreviousHash != previousEntry.Hash)
                    {
                        result.HasTampering = true;
                        result.TamperedIndices.Add(i);
                        result.Details.Add($"Broken chain at entry {i}: PreviousHash mismatch");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Find the last valid entry before tampering
        /// </summary>
        public static LedgerEntry? FindLastValidEntry(List<LedgerEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return null;

            var sorted = entries.OrderBy(e => e.SequenceNumber).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                if (!sorted[i].VerifyIntegrity())
                {
                    // Return the previous valid entry
                    return i > 0 ? sorted[i - 1] : null;
                }
            }

            // All entries valid
            return sorted.Last();
        }

        /// <summary>
        /// Generate integrity report
        /// </summary>
        public static IntegrityReport GenerateReport(List<LedgerEntry> entries)
        {
            var report = new IntegrityReport
            {
                TotalEntries = entries?.Count ?? 0,
                VerifiedAt = DateTime.UtcNow
            };

            if (entries == null || entries.Count == 0)
            {
                report.Status = IntegrityStatus.Empty;
                return report;
            }

            var tamperingResult = DetectTampering(entries);
            
            if (tamperingResult.HasTampering)
            {
                report.Status = IntegrityStatus.Compromised;
                report.TamperedEntries = tamperingResult.TamperedIndices.Count;
                report.Details = tamperingResult.Details;
            }
            else
            {
                report.Status = IntegrityStatus.Valid;
                report.ChainLength = entries.Count;
                report.FirstEntryHash = entries.OrderBy(e => e.SequenceNumber).First().Hash;
                report.LastEntryHash = entries.OrderBy(e => e.SequenceNumber).Last().Hash;
            }

            return report;
        }
    }

    /// <summary>
    /// Result of tampering detection
    /// </summary>
    public class TamperingResult
    {
        public bool HasTampering { get; set; }
        public List<int> TamperedIndices { get; set; } = new List<int>();
        public List<string> Details { get; set; } = new List<string>();
    }

    /// <summary>
    /// Integrity report for the ledger
    /// </summary>
    public class IntegrityReport
    {
        public IntegrityStatus Status { get; set; }
        public int TotalEntries { get; set; }
        public int TamperedEntries { get; set; }
        public int ChainLength { get; set; }
        public string? FirstEntryHash { get; set; }
        public string? LastEntryHash { get; set; }
        public List<string> Details { get; set; } = new List<string>();
        public DateTime VerifiedAt { get; set; }
    }

    public enum IntegrityStatus
    {
        Empty,
        Valid,
        Compromised
    }
}
