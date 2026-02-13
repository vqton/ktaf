using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Enterprise.Audit;
using Xunit;

namespace AccountingERP.Domain.Tests.Enterprise.Audit
{
    /// <summary>
    /// Enterprise-grade field-level audit trail tests
    /// Ensures all data changes are tracked with before/after values
    /// </summary>
    public class FieldLevelAuditTrailTests
    {
        private readonly FieldLevelAuditService _service;
        private readonly MockAuditRepository _repository;

        public FieldLevelAuditTrailTests()
        {
            _repository = new MockAuditRepository();
            _service = new FieldLevelAuditService(_repository);
        }

        [Fact]
        public void RecordChange_SingleFieldChange_ShouldCaptureBeforeAndAfter()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var change = new FieldChange
            {
                FieldName = "Amount",
                OldValue = "1000.00",
                NewValue = "1500.00",
                ChangeType = ChangeType.Modified
            };

            // Act
            var result = _service.RecordChange(
                entityId, 
                "JournalEntry", 
                change, 
                "accountant", 
                "Updated invoice amount"
            );

            // Assert
            Assert.True(result.IsSuccess);
            var audit = _repository.GetByEntityId(entityId).First();
            Assert.Equal("1000.00", audit.Changes[0].OldValue);
            Assert.Equal("1500.00", audit.Changes[0].NewValue);
        }

        [Fact]
        public void RecordChange_MultipleFields_ShouldCaptureAllChanges()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var changes = new List<FieldChange>
            {
                new() { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                new() { FieldName = "Description", OldValue = "Old desc", NewValue = "New desc", ChangeType = ChangeType.Modified },
                new() { FieldName = "Status", OldValue = "Draft", NewValue = "Posted", ChangeType = ChangeType.Modified }
            };

            // Act
            var result = _service.RecordChanges(
                entityId, 
                "JournalEntry", 
                changes, 
                "accountant", 
                "Multiple updates"
            );

            // Assert
            Assert.True(result.IsSuccess);
            var audit = _repository.GetByEntityId(entityId).First();
            Assert.Equal(3, audit.Changes.Count);
        }

        [Fact]
        public void RecordChange_CreateOperation_ShouldCaptureInitialValues()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var changes = new List<FieldChange>
            {
                new() { FieldName = "Id", OldValue = null, NewValue = entityId.ToString(), ChangeType = ChangeType.Created },
                new() { FieldName = "Amount", OldValue = null, NewValue = "1000.00", ChangeType = ChangeType.Created },
                new() { FieldName = "CreatedBy", OldValue = null, NewValue = "accountant", ChangeType = ChangeType.Created }
            };

            // Act
            var result = _service.RecordChanges(
                entityId, 
                "JournalEntry", 
                changes, 
                "system", 
                "Entity created"
            );

            // Assert
            Assert.True(result.IsSuccess);
            var audit = _repository.GetByEntityId(entityId).First();
            Assert.Equal(OperationType.Create, audit.Operation);
            Assert.All(audit.Changes, c => Assert.Equal(ChangeType.Created, c.ChangeType));
        }

        [Fact]
        public void RecordChange_DeleteOperation_ShouldCaptureFinalValues()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var changes = new List<FieldChange>
            {
                new() { FieldName = "Id", OldValue = entityId.ToString(), NewValue = null, ChangeType = ChangeType.Deleted },
                new() { FieldName = "Amount", OldValue = "1000.00", NewValue = null, ChangeType = ChangeType.Deleted }
            };

            // Act
            var result = _service.RecordChanges(
                entityId, 
                "JournalEntry", 
                changes, 
                "admin", 
                "Entity deleted"
            );

            // Assert
            Assert.True(result.IsSuccess);
            var audit = _repository.GetByEntityId(entityId).First();
            Assert.Equal(OperationType.Delete, audit.Operation);
        }

        [Fact]
        public void GetAuditHistory_ShouldReturnAllChangesForEntity()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            _service.RecordChange(entityId, "JournalEntry", 
                new FieldChange { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                "user1", "First update");
            _service.RecordChange(entityId, "JournalEntry", 
                new FieldChange { FieldName = "Amount", OldValue = "1500", NewValue = "2000", ChangeType = ChangeType.Modified },
                "user2", "Second update");

            // Act
            var history = _service.GetAuditHistory(entityId);

            // Assert
            Assert.Equal(2, history.Count);
            Assert.Equal("user1", history[0].PerformedBy);
            Assert.Equal("user2", history[1].PerformedBy);
        }

        [Fact]
        public void GetAuditHistory_WithDateRange_ShouldFilterByDate()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var oldDate = DateTime.UtcNow.AddDays(-10);
            var recentDate = DateTime.UtcNow.AddDays(-1);
            
            _repository.Add(CreateAuditRecord(entityId, oldDate, "1000", "1500"));
            _repository.Add(CreateAuditRecord(entityId, recentDate, "1500", "2000"));

            // Act
            var history = _service.GetAuditHistory(entityId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

            // Assert
            Assert.Single(history);
            Assert.Equal("1500", history[0].Changes[0].OldValue);
        }

        [Fact]
        public void GetFieldHistory_ShouldReturnChangesForSpecificField()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            _service.RecordChanges(entityId, "JournalEntry",
                new List<FieldChange>
                {
                    new() { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                    new() { FieldName = "Description", OldValue = "Old", NewValue = "New", ChangeType = ChangeType.Modified }
                },
                "user1", "Update");

            // Act
            var amountHistory = _service.GetFieldHistory(entityId, "Amount");

            // Assert
            Assert.Single(amountHistory);
            Assert.Equal("Amount", amountHistory[0].Changes[0].FieldName);
        }

        [Fact]
        public void CompareVersions_ShouldShowDifferences()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var audit1 = CreateAuditRecord(entityId, DateTime.UtcNow.AddHours(-2), "1000", "1500");
            var audit2 = CreateAuditRecord(entityId, DateTime.UtcNow.AddHours(-1), "1500", "2000");
            _repository.Add(audit1);
            _repository.Add(audit2);

            // Act
            var comparison = _service.CompareVersions(entityId, audit1.Id, audit2.Id);

            // Assert
            Assert.NotNull(comparison);
            Assert.Equal(audit1.Id, comparison.FromVersionId);
            Assert.Equal(audit2.Id, comparison.ToVersionId);
        }

        [Fact]
        public void DetectUnauthorizedChange_WithSensitiveField_ShouldFlag()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var sensitiveFields = new[] { "Amount", "TaxCode", "BankAccount" };

            // Act
            var result = _service.RecordChange(entityId, "JournalEntry",
                new FieldChange { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                "junior_user", "Update", sensitiveFields);

            // Assert
            Assert.True(result.RequiresReview);
            Assert.Contains("Amount", result.FlaggedFields);
        }

        [Fact]
        public void AuditRecord_ShouldIncludeTimestamp()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var before = DateTime.UtcNow.AddSeconds(-1);

            // Act
            _service.RecordChange(entityId, "JournalEntry",
                new FieldChange { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                "accountant", "Update");

            var after = DateTime.UtcNow.AddSeconds(1);
            var audit = _repository.GetByEntityId(entityId).First();

            // Assert
            Assert.True(audit.Timestamp >= before);
            Assert.True(audit.Timestamp <= after);
        }

        [Fact]
        public void AuditRecord_ShouldIncludeTransactionId()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            // Act
            _service.RecordChange(entityId, "JournalEntry",
                new FieldChange { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                "accountant", "Update", transactionId: transactionId);

            var audit = _repository.GetByEntityId(entityId).First();

            // Assert
            Assert.Equal(transactionId, audit.TransactionId);
        }

        [Fact]
        public void GetAuditReport_ShouldGenerateSummary()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            _service.RecordChange(entityId, "JournalEntry",
                new FieldChange { FieldName = "Amount", OldValue = "1000", NewValue = "1500", ChangeType = ChangeType.Modified },
                "user1", "Update1");
            _service.RecordChange(entityId, "JournalEntry",
                new FieldChange { FieldName = "Amount", OldValue = "1500", NewValue = "2000", ChangeType = ChangeType.Modified },
                "user2", "Update2");

            // Act
            var report = _service.GetAuditReport(entityId);

            // Assert
            Assert.Equal(2, report.TotalChanges);
            Assert.Equal(1, report.UniqueFieldsChanged);
            Assert.Contains("user1", report.UsersInvolved);
            Assert.Contains("user2", report.UsersInvolved);
        }

        private FieldLevelAuditRecord CreateAuditRecord(Guid entityId, DateTime timestamp, string oldValue, string newValue)
        {
            return new FieldLevelAuditRecord
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                EntityType = "JournalEntry",
                Operation = OperationType.Update,
                Changes = new List<FieldChange>
                {
                    new() { FieldName = "Amount", OldValue = oldValue, NewValue = newValue, ChangeType = ChangeType.Modified }
                },
                PerformedBy = "test_user",
                Timestamp = timestamp,
                Reason = "Test"
            };
        }
    }

    /// <summary>
    /// Mock repository for testing
    /// </summary>
    public class MockAuditRepository : AccountingERP.Domain.Enterprise.Audit.MockAuditRepository
    {
        private readonly List<FieldLevelAuditRecord> _records = new();

        public void Add(FieldLevelAuditRecord record) => _records.Add(record);
        public List<FieldLevelAuditRecord> GetByEntityId(Guid entityId) => 
            _records.Where(r => r.EntityId == entityId).OrderBy(r => r.Timestamp).ToList();
        public List<FieldLevelAuditRecord> GetByTransactionId(Guid transactionId) =>
            _records.Where(r => r.TransactionId == transactionId).ToList();
        public List<FieldLevelAuditRecord> GetAll() => _records.OrderBy(r => r.Timestamp).ToList();
    }
}
