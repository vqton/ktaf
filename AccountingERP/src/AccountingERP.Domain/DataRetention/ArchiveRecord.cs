using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;

namespace AccountingERP.Domain.DataRetention
{
    /// <summary>
    /// Entity: Archive Record
    /// Lưu trữ dữ liệu sau 10 năm
    /// </summary>
    public class ArchiveRecord : BaseEntity
    {
        public string EntityType { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public string Data { get; private set; } = string.Empty;
        public DateTime ArchivedAt { get; private set; }
        public string ArchivedBy { get; private set; } = string.Empty;
        public DateTime OriginalCreatedAt { get; private set; }
        public int RetentionYears { get; private set; }
        public DateTime ExpiryDate => OriginalCreatedAt.AddYears(RetentionYears);

        private ArchiveRecord() : base() { }

        public static ArchiveRecord Create<T>(T entity, string data, string archivedBy, int retentionYears = 10) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new ArchiveRecord
            {
                Id = Guid.NewGuid(),
                EntityType = typeof(T).Name,
                EntityId = entity.Id.ToString(),
                Data = data,
                ArchivedAt = DateTime.UtcNow,
                ArchivedBy = archivedBy,
                OriginalCreatedAt = entity.CreatedAt,
                RetentionYears = retentionYears
            };
        }
    }

    /// <summary>
    /// Entity: Deletion Attempt Log
    /// Ghi nhận các lần thử xóa dữ liệu
    /// </summary>
    public class DeletionAttemptLog : BaseEntity
    {
        public string EntityType { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public DateTime EntityCreatedAt { get; private set; }
        public bool IsAllowed { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public string AttemptedBy { get; private set; } = string.Empty;
        public DateTime AttemptedAt { get; private set; }
        public string? ErrorMessage { get; private set; }

        private DeletionAttemptLog() : base() { }

        public static DeletionAttemptLog Create<T>(
            T entity, 
            bool isAllowed, 
            string reason, 
            string attemptedBy,
            string? errorMessage = null) where T : BaseEntity
        {
            return new DeletionAttemptLog
            {
                Id = Guid.NewGuid(),
                EntityType = typeof(T).Name,
                EntityId = entity.Id.ToString(),
                EntityCreatedAt = entity.CreatedAt,
                IsAllowed = isAllowed,
                Reason = reason,
                AttemptedBy = attemptedBy,
                AttemptedAt = DateTime.UtcNow,
                ErrorMessage = errorMessage
            };
        }
    }
}
