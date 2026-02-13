using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountingERP.Domain.Enterprise.Audit
{
    /// <summary>
    /// Enterprise-grade field-level audit trail service
    /// Tracks all data changes with before/after values for compliance
    /// </summary>
    public class FieldLevelAuditService
    {
        private readonly MockAuditRepository _repository;

        public FieldLevelAuditService(MockAuditRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Record a single field change
        /// </summary>
        public AuditResult RecordChange(
            Guid entityId, 
            string entityType, 
            FieldChange change, 
            string performedBy, 
            string reason,
            string[]? sensitiveFields = null,
            Guid? transactionId = null)
        {
            return RecordChanges(entityId, entityType, new List<FieldChange> { change }, 
                performedBy, reason, sensitiveFields, transactionId);
        }

        /// <summary>
        /// Record multiple field changes in a single operation
        /// </summary>
        public AuditResult RecordChanges(
            Guid entityId, 
            string entityType, 
            List<FieldChange> changes, 
            string performedBy, 
            string reason,
            string[]? sensitiveFields = null,
            Guid? transactionId = null)
        {
            if (changes == null || !changes.Any())
                return AuditResult.Success();

            // Determine operation type based on changes
            var operation = DetermineOperationType(changes);

            // Check for sensitive field changes
            var flaggedFields = new List<string>();
            if (sensitiveFields != null)
            {
                flaggedFields = changes
                    .Where(c => sensitiveFields.Contains(c.FieldName, StringComparer.OrdinalIgnoreCase))
                    .Select(c => c.FieldName)
                    .ToList();
            }

            var auditRecord = new FieldLevelAuditRecord
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                EntityType = entityType,
                Operation = operation,
                Changes = changes,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow,
                Reason = reason,
                TransactionId = transactionId ?? Guid.NewGuid()
            };

            _repository.Add(auditRecord);

            return new AuditResult
            {
                IsSuccess = true,
                RequiresReview = flaggedFields.Any(),
                FlaggedFields = flaggedFields,
                AuditId = auditRecord.Id
            };
        }

        /// <summary>
        /// Get complete audit history for an entity
        /// </summary>
        public List<FieldLevelAuditRecord> GetAuditHistory(Guid entityId)
        {
            return _repository.GetByEntityId(entityId);
        }

        /// <summary>
        /// Get audit history filtered by date range
        /// </summary>
        public List<FieldLevelAuditRecord> GetAuditHistory(Guid entityId, DateTime fromDate, DateTime toDate)
        {
            return _repository.GetByEntityId(entityId)
                .Where(r => r.Timestamp >= fromDate && r.Timestamp <= toDate)
                .ToList();
        }

        /// <summary>
        /// Get history of changes for a specific field
        /// </summary>
        public List<FieldLevelAuditRecord> GetFieldHistory(Guid entityId, string fieldName)
        {
            return _repository.GetByEntityId(entityId)
                .Where(r => r.Changes.Any(c => c.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Compare two versions of an entity
        /// </summary>
        public VersionComparison? CompareVersions(Guid entityId, Guid fromVersionId, Guid toVersionId)
        {
            var allHistory = _repository.GetByEntityId(entityId);
            var fromVersion = allHistory.FirstOrDefault(r => r.Id == fromVersionId);
            var toVersion = allHistory.FirstOrDefault(r => r.Id == toVersionId);

            if (fromVersion == null || toVersion == null)
                return null;

            return new VersionComparison
            {
                EntityId = entityId,
                FromVersionId = fromVersionId,
                ToVersionId = toVersionId,
                FromTimestamp = fromVersion.Timestamp,
                ToTimestamp = toVersion.Timestamp,
                ChangesBetweenVersions = GetChangesBetween(fromVersion, toVersion)
            };
        }

        /// <summary>
        /// Generate audit report for an entity
        /// </summary>
        public AuditReport GetAuditReport(Guid entityId)
        {
            var history = _repository.GetByEntityId(entityId);
            
            if (!history.Any())
                return new AuditReport { EntityId = entityId };

            var allChanges = history.SelectMany(h => h.Changes).ToList();
            var uniqueFields = allChanges.Select(c => c.FieldName).Distinct().ToList();
            var usersInvolved = history.Select(h => h.PerformedBy).Distinct().ToList();

            return new AuditReport
            {
                EntityId = entityId,
                TotalChanges = allChanges.Count,
                UniqueFieldsChanged = uniqueFields.Count,
                FirstChangeAt = history.Min(h => h.Timestamp),
                LastChangeAt = history.Max(h => h.Timestamp),
                UsersInvolved = usersInvolved,
                MostModifiedFields = allChanges
                    .GroupBy(c => c.FieldName)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new FieldModificationStats 
                    { 
                        FieldName = g.Key, 
                        ChangeCount = g.Count() 
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Get audit trail for a specific transaction
        /// </summary>
        public List<FieldLevelAuditRecord> GetTransactionAudit(Guid transactionId)
        {
            return _repository.GetByTransactionId(transactionId);
        }

        private OperationType DetermineOperationType(List<FieldChange> changes)
        {
            if (changes.All(c => c.ChangeType == ChangeType.Created))
                return OperationType.Create;
            
            if (changes.All(c => c.ChangeType == ChangeType.Deleted))
                return OperationType.Delete;
            
            return OperationType.Update;
        }

        private List<FieldDifference> GetChangesBetween(FieldLevelAuditRecord fromVersion, FieldLevelAuditRecord toVersion)
        {
            var differences = new List<FieldDifference>();

            // This is a simplified comparison - in real implementation, 
            // you'd reconstruct entity state at each version
            foreach (var change in toVersion.Changes)
            {
                differences.Add(new FieldDifference
                {
                    FieldName = change.FieldName,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue
                });
            }

            return differences;
        }
    }

    /// <summary>
    /// Result of an audit operation
    /// </summary>
    public class AuditResult
    {
        public bool IsSuccess { get; set; }
        public bool RequiresReview { get; set; }
        public List<string> FlaggedFields { get; set; } = new();
        public Guid AuditId { get; set; }

        public static AuditResult Success() => new() { IsSuccess = true };
    }

    /// <summary>
    /// Field-level audit record
    /// </summary>
    public class FieldLevelAuditRecord
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public OperationType Operation { get; set; }
        public List<FieldChange> Changes { get; set; } = new();
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
    }

    /// <summary>
    /// Represents a change to a single field
    /// </summary>
    public class FieldChange
    {
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public ChangeType ChangeType { get; set; }
    }

    /// <summary>
    /// Type of change operation
    /// </summary>
    public enum ChangeType
    {
        Created,
        Modified,
        Deleted
    }

    /// <summary>
    /// Type of operation performed
    /// </summary>
    public enum OperationType
    {
        Create,
        Update,
        Delete
    }

    /// <summary>
    /// Comparison between two versions
    /// </summary>
    public class VersionComparison
    {
        public Guid EntityId { get; set; }
        public Guid FromVersionId { get; set; }
        public Guid ToVersionId { get; set; }
        public DateTime FromTimestamp { get; set; }
        public DateTime ToTimestamp { get; set; }
        public List<FieldDifference> ChangesBetweenVersions { get; set; } = new();
    }

    /// <summary>
    /// Difference in a single field between versions
    /// </summary>
    public class FieldDifference
    {
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }

    /// <summary>
    /// Audit report summary
    /// </summary>
    public class AuditReport
    {
        public Guid EntityId { get; set; }
        public int TotalChanges { get; set; }
        public int UniqueFieldsChanged { get; set; }
        public DateTime? FirstChangeAt { get; set; }
        public DateTime? LastChangeAt { get; set; }
        public List<string> UsersInvolved { get; set; } = new();
        public List<FieldModificationStats> MostModifiedFields { get; set; } = new();
    }

    /// <summary>
    /// Statistics for field modifications
    /// </summary>
    public class FieldModificationStats
    {
        public string FieldName { get; set; } = string.Empty;
        public int ChangeCount { get; set; }
    }

    /// <summary>
    /// Mock repository interface - replace with actual repository in production
    /// </summary>
    public interface MockAuditRepository
    {
        void Add(FieldLevelAuditRecord record);
        List<FieldLevelAuditRecord> GetByEntityId(Guid entityId);
        List<FieldLevelAuditRecord> GetByTransactionId(Guid transactionId);
        List<FieldLevelAuditRecord> GetAll();
    }
}
