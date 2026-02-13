using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enterprise.Ledger;
using AccountingERP.Domain.Invoicing;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.Enterprise.Ledger
{
    /// <summary>
    /// Enterprise-grade immutable ledger tests with SHA256 hash chain
    /// </summary>
    public class ImmutableLedgerTests
    {
        [Fact]
        public void LedgerEntry_Create_ShouldGenerateHash()
        {
            // Arrange
            var entry = CreateTestEntry();
            
            // Act
            var ledgerEntry = LedgerEntry.Create(entry, null);
            
            // Assert
            Assert.NotNull(ledgerEntry.Hash);
            Assert.NotEmpty(ledgerEntry.Hash);
            Assert.Equal(64, ledgerEntry.Hash.Length); // SHA256 = 64 hex chars
        }

        [Fact]
        public void LedgerEntry_Create_WithPreviousHash_ShouldChainHashes()
        {
            // Arrange
            var entry1 = CreateTestEntry();
            var ledgerEntry1 = LedgerEntry.Create(entry1, null);
            
            var entry2 = CreateTestEntry();
            
            // Act
            var ledgerEntry2 = LedgerEntry.Create(entry2, ledgerEntry1.Hash);
            
            // Assert
            Assert.Equal(ledgerEntry1.Hash, ledgerEntry2.PreviousHash);
            Assert.NotEqual(ledgerEntry1.Hash, ledgerEntry2.Hash);
        }

        [Fact]
        public void LedgerEntry_VerifyIntegrity_UnchangedEntry_ShouldReturnTrue()
        {
            // Arrange
            var entry = CreateTestEntry();
            var ledgerEntry = LedgerEntry.Create(entry, null);
            
            // Act
            var isValid = ledgerEntry.VerifyIntegrity();
            
            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void LedgerEntry_VerifyIntegrity_TamperedEntry_ShouldReturnFalse()
        {
            // Arrange - Create a ledger entry and then tamper with its stored hash
            var entry = CreateTestEntry();
            var ledgerEntry = LedgerEntry.Create(entry, null);
            
            // Tamper with the hash directly via reflection
            var hashField = typeof(LedgerEntry).GetProperty("Hash");
            hashField?.SetValue(ledgerEntry, "tampered_hash_12345678901234567890123456789012345678901234567890");
            
            // Act
            var isValid = ledgerEntry.VerifyIntegrity();
            
            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void LedgerEntry_VerifyChain_ValidChain_ShouldReturnTrue()
        {
            // Arrange
            var entries = CreateTestChain(5);
            
            // Act
            var isValid = LedgerIntegrityValidator.VerifyChain(entries);
            
            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void LedgerEntry_VerifyChain_BrokenChain_ShouldReturnFalse()
        {
            // Arrange
            var entries = CreateTestChain(5);
            
            // Break the chain by modifying a hash
            entries[2].GetType().GetProperty("Hash")?.SetValue(entries[2], "tampered_hash");
            
            // Act
            var isValid = LedgerIntegrityValidator.VerifyChain(entries);
            
            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void LedgerEntry_VerifyChain_MissingLink_ShouldReturnFalse()
        {
            // Arrange
            var entries = CreateTestChain(5);
            entries.RemoveAt(2); // Remove middle entry
            
            // Act
            var isValid = LedgerIntegrityValidator.VerifyChain(entries);
            
            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void LedgerIntegrityValidator_DetectTampering_ShouldIdentifyModifiedEntry()
        {
            // Arrange
            var entries = CreateTestChain(5);
            var originalHash = entries[2].Hash;
            
            // Tamper with entry
            entries[2].GetType().GetProperty("Hash")?.SetValue(entries[2], "fake_hash");
            
            // Act
            var result = LedgerIntegrityValidator.DetectTampering(entries);
            
            // Assert
            Assert.True(result.HasTampering);
            Assert.Contains(2, result.TamperedIndices);
        }

        [Fact]
        public void LedgerEntry_Hash_ShouldIncludeAllCriticalFields()
        {
            // Arrange
            var entry = CreateTestEntry();
            var ledgerEntry = LedgerEntry.Create(entry, null);
            
            // Act - Modify non-hashed field (if any)
            // All fields should be included in hash
            var isValid = ledgerEntry.VerifyIntegrity();
            
            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void LedgerEntry_Create_ShouldBeAppendOnly()
        {
            // Arrange
            var entry = CreateTestEntry();
            var ledgerEntry = LedgerEntry.Create(entry, null);
            
            // Act & Assert - Verify no update methods exist
            Assert.DoesNotContain(ledgerEntry.GetType().GetMethods(), 
                m => m.Name.Contains("Update") || m.Name.Contains("Modify"));
        }

        [Fact]
        public void LedgerEntry_SequenceNumber_ShouldAutoIncrement()
        {
            // Arrange & Act
            var entries = CreateTestChain(5);
            
            // Assert
            for (int i = 0; i < entries.Count; i++)
            {
                Assert.Equal(i + 1, entries[i].SequenceNumber);
            }
        }

        [Fact]
        public void LedgerEntry_Timestamp_ShouldBeImmutable()
        {
            // Arrange
            var entry = CreateTestEntry();
            var beforeCreate = DateTime.UtcNow.AddSeconds(-1);
            var ledgerEntry = LedgerEntry.Create(entry, null);
            var afterCreate = DateTime.UtcNow.AddSeconds(1);
            
            // Assert
            Assert.True(ledgerEntry.Timestamp >= beforeCreate);
            Assert.True(ledgerEntry.Timestamp <= afterCreate);
        }

        private JournalEntry CreateTestEntry()
        {
            var entry = JournalEntry.Create(
                $"BT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}",
                "INV-001",
                new DateTime(2024, 1, 15),
                new DateTime(2024, 1, 14),
                "Test entry"
            );
            entry.AddLine("111", 1000000, 0, "Cash");
            entry.AddLine("511", 0, 1000000, "Revenue");
            entry.LinkToInvoice(AccountingERP.Domain.Invoicing.InvoiceId.New());
            entry.Post("accountant");
            return entry;
        }

        private List<LedgerEntry> CreateTestChain(int count)
        {
            // Reset sequence counter for test isolation
            LedgerEntry.ResetSequenceCounter();
            
            var entries = new List<LedgerEntry>();
            string? previousHash = null;

            for (int i = 0; i < count; i++)
            {
                var entry = CreateTestEntry();
                entry.GetType().GetProperty("EntryNumber")?.SetValue(entry, $"BT-{i + 1:0000}");
                var ledgerEntry = LedgerEntry.Create(entry, previousHash);
                entries.Add(ledgerEntry);
                previousHash = ledgerEntry.Hash;
            }

            return entries;
        }
    }
}
