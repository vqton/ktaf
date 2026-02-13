using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;

namespace AccountingERP.Domain.Audit
{
    /// <summary>
    /// Entity: Audit Log - Field-level change tracking
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public string FieldName { get; private set; } = string.Empty;
        public string? OldValue { get; private set; }
        public string? NewValue { get; private set; }
        public string ChangedBy { get; private set; } = string.Empty;
        public DateTime ChangedAt { get; private set; }
        public string? TransactionId { get; private set; }

        private AuditLog() : base() { }

        public static AuditLog Create(
            string entityName,
            string entityId,
            string fieldName,
            string? oldValue,
            string? newValue,
            string changedBy,
            string? transactionId = null)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity name cannot be empty", nameof(entityName));
            
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));
            
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be empty", nameof(fieldName));

            // Don't create audit log if value hasn't changed
            if (oldValue == newValue)
                throw new InvalidOperationException("Cannot create audit log for unchanged value");

            return new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                FieldName = fieldName,
                OldValue = oldValue?.Length > 4000 ? oldValue[..4000] : oldValue,
                NewValue = newValue?.Length > 4000 ? newValue[..4000] : newValue,
                ChangedBy = changedBy.Trim(),
                ChangedAt = DateTime.UtcNow,
                TransactionId = transactionId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Interface for auditable entities
    /// </summary>
    public interface IAuditable
    {
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; }
        string CreatedBy { get; }
        string? UpdatedBy { get; }
    }

    /// <summary>
    /// Configuration for audit trail
    /// </summary>
    public class AuditConfiguration
    {
        /// <summary>
        /// Entities to exclude from audit
        /// </summary>
        public List<string> ExcludedEntities { get; set; } = new()
        {
            "AuditLog",
            "DomainEvent",
            "TrialBalance"
        };

        /// <summary>
        /// Properties to exclude from audit (common technical fields)
        /// </summary>
        public List<string> ExcludedProperties { get; set; } = new()
        {
            "Id",
            "CreatedAt",
            "UpdatedAt",
            "CreatedBy",
            "UpdatedBy",
            "RowVersion",
            "ConcurrencyStamp"
        };

        /// <summary>
        /// Maximum length for stored values
        /// </summary>
        public int MaxValueLength { get; set; } = 4000;

        /// <summary>
        /// Check if entity should be audited
        /// </summary>
        public bool ShouldAuditEntity(string entityName)
        {
            return !ExcludedEntities.Contains(entityName);
        }

        /// <summary>
        /// Check if property should be audited
        /// </summary>
        public bool ShouldAuditProperty(string propertyName)
        {
            return !ExcludedProperties.Contains(propertyName);
        }
    }
}
