using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AccountingERP.Domain.Audit;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Audit.Services
{
    /// <summary>
    /// Domain Service: Audit Trail tracking
    /// Tracks before/after values of entity changes
    /// </summary>
    public interface IAuditTrailService
    {
        /// <summary>
        /// Track changes between old and new entity states
        /// </summary>
        IEnumerable<AuditLog> TrackChanges<T>(T oldEntity, T newEntity, string changedBy) where T : BaseEntity;
        
        /// <summary>
        /// Track property changes for an entity update
        /// </summary>
        IEnumerable<AuditLog> TrackPropertyChanges<T>(
            T entity, 
            Dictionary<string, (object? OldValue, object? NewValue)> changes, 
            string changedBy) where T : BaseEntity;

        /// <summary>
        /// Check if entity type should be audited
        /// </summary>
        bool ShouldAuditEntity<T>() where T : BaseEntity;

        /// <summary>
        /// Check if property should be audited
        /// </summary>
        bool ShouldAuditProperty(string propertyName);
    }

    /// <summary>
    /// Implementation of Audit Trail Service
    /// </summary>
    public class AuditTrailService : IAuditTrailService
    {
        private readonly AuditConfiguration _configuration;

        public AuditTrailService(AuditConfiguration? configuration = null)
        {
            _configuration = configuration ?? new AuditConfiguration();
        }

        /// <summary>
        /// Track all changes between old and new entity states
        /// </summary>
        public IEnumerable<AuditLog> TrackChanges<T>(T oldEntity, T newEntity, string changedBy) where T : BaseEntity
        {
            if (oldEntity == null)
                throw new ArgumentNullException(nameof(oldEntity));
            
            if (newEntity == null)
                throw new ArgumentNullException(nameof(newEntity));

            if (oldEntity.Id != newEntity.Id)
                throw new ArgumentException("Cannot compare entities with different IDs");

            var entityName = typeof(T).Name;
            var entityId = oldEntity.Id.ToString();
            var auditLogs = new List<AuditLog>();

            // Get all public properties
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => ShouldAuditProperty(p.Name));

            foreach (var property in properties)
            {
                var oldValue = property.GetValue(oldEntity);
                var newValue = property.GetValue(newEntity);

                // Compare values
                if (!AreValuesEqual(oldValue, newValue))
                {
                    var oldValueStr = SerializeValue(oldValue);
                    var newValueStr = SerializeValue(newValue);

                    try
                    {
                        var auditLog = AuditLog.Create(
                            entityName,
                            entityId,
                            property.Name,
                            oldValueStr,
                            newValueStr,
                            changedBy);

                        auditLogs.Add(auditLog);
                    }
                    catch (InvalidOperationException)
                    {
                        // Skip if values are effectively the same after serialization
                        continue;
                    }
                }
            }

            return auditLogs;
        }

        /// <summary>
        /// Track specific property changes
        /// </summary>
        public IEnumerable<AuditLog> TrackPropertyChanges<T>(
            T entity, 
            Dictionary<string, (object? OldValue, object? NewValue)> changes, 
            string changedBy) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityName = typeof(T).Name;
            var entityId = entity.Id.ToString();
            var auditLogs = new List<AuditLog>();

            foreach (var change in changes)
            {
                var propertyName = change.Key;
                
                if (!ShouldAuditProperty(propertyName))
                    continue;

                var oldValueStr = SerializeValue(change.Value.OldValue);
                var newValueStr = SerializeValue(change.Value.NewValue);

                if (oldValueStr == newValueStr)
                    continue;

                try
                {
                    var auditLog = AuditLog.Create(
                        entityName,
                        entityId,
                        propertyName,
                        oldValueStr,
                        newValueStr,
                        changedBy);

                    auditLogs.Add(auditLog);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }

            return auditLogs;
        }

        /// <summary>
        /// Check if entity should be audited
        /// </summary>
        public bool ShouldAuditEntity<T>() where T : BaseEntity
        {
            return _configuration.ShouldAuditEntity(typeof(T).Name);
        }

        /// <summary>
        /// Check if property should be audited
        /// </summary>
        public bool ShouldAuditProperty(string propertyName)
        {
            return _configuration.ShouldAuditProperty(propertyName);
        }

        /// <summary>
        /// Compare two values for equality
        /// </summary>
        private bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null)
                return true;

            if (value1 == null || value2 == null)
                return false;

            // Handle Money type specifically
            if (value1 is Money money1 && value2 is Money money2)
                return money1 == money2;

            // Handle DateTime comparison
            if (value1 is DateTime dt1 && value2 is DateTime dt2)
                return dt1.ToUniversalTime() == dt2.ToUniversalTime();

            // Handle decimal comparison with precision
            if (value1 is decimal dec1 && value2 is decimal dec2)
                return dec1 == dec2;

            return value1.Equals(value2);
        }

        /// <summary>
        /// Serialize value to string for storage
        /// </summary>
        private string? SerializeValue(object? value)
        {
            if (value == null)
                return null;

            // Handle Money type
            if (value is Money money)
                return $"{money.Amount}|{money.Currency}";

            // Handle DateTime
            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Handle primitive types
            if (value.GetType().IsPrimitive || value is string || value is decimal)
                return value.ToString();

            // Handle enums
            if (value.GetType().IsEnum)
                return $"{value.GetType().Name}.{value}";

            // Handle complex types - serialize to JSON
            try
            {
                var json = JsonSerializer.Serialize(value);
                if (json.Length > _configuration.MaxValueLength)
                {
                    return json.Substring(0, _configuration.MaxValueLength - 3) + "...";
                }
                return json;
            }
            catch
            {
                return value.ToString();
            }
        }
    }

    /// <summary>
    /// Helper for creating audit snapshots
    /// </summary>
    public static class AuditSnapshot
    {
        /// <summary>
        /// Create a snapshot of entity state for later comparison
        /// </summary>
        public static Dictionary<string, object?> CreateSnapshot<T>(T entity) where T : BaseEntity
        {
            var snapshot = new Dictionary<string, object?>();
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && !p.GetIndexParameters().Any());

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(entity);
                    snapshot[property.Name] = value;
                }
                catch
                {
                    // Skip properties that can't be read
                    continue;
                }
            }

            return snapshot;
        }
    }
}
