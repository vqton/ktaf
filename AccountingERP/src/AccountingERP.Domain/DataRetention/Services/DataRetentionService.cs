using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.DataRetention;
using AccountingERP.Domain.Entities;

namespace AccountingERP.Domain.DataRetention.Services
{
    /// <summary>
    /// Domain Service: Data Retention Policy
    /// Article 16 - Luật Kế toán: 10 year retention
    /// </summary>
    public interface IDataRetentionService
    {
        /// <summary>
        /// Kiểm tra xem có thể xóa entity không
        /// </summary>
        ValidationResult ValidateDeletion<T>(T entity, int currentYear) where T : BaseEntity;
        
        /// <summary>
        /// Kiểm tra xem có thể archive entity không
        /// </summary>
        ValidationResult ValidateArchive<T>(T entity, int currentYear) where T : BaseEntity;
        
        /// <summary>
        /// Kiểm tra entity có phải là accounting entry (không được xóa)
        /// </summary>
        bool IsImmutableEntity<T>() where T : BaseEntity;
        
        /// <summary>
        /// Lấy danh sách entities đủ điều kiện archive
        /// </summary>
        IEnumerable<T> GetArchivableEntities<T>(IEnumerable<T> entities, int currentYear) where T : BaseEntity;
    }

    /// <summary>
    /// Kết quả validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EntityAgeInYears { get; set; }
        public int MinimumRetentionYears { get; set; }
    }

    /// <summary>
    /// Implementation
    /// </summary>
    public class DataRetentionService : IDataRetentionService
    {
        private const int MINIMUM_RETENTION_YEARS = 10;
        private readonly List<string> _immutableEntities = new()
        {
            "JournalEntry",
            "JournalEntryLine",
            "AccountingPeriod",
            "Invoice"
        };

        /// <summary>
        /// Validate deletion request
        /// RULE: No deletion < 10 years for accounting data
        /// </summary>
        public ValidationResult ValidateDeletion<T>(T entity, int currentYear) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Check if immutable
            if (IsImmutableEntity<T>())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Không thể xóa bút toán kế toán. Theo Luật Kế toán Article 16, chứng từ kế toán phải được lưu trữ vĩnh viễn.",
                    EntityAgeInYears = CalculateAge(entity.CreatedAt, currentYear),
                    MinimumRetentionYears = MINIMUM_RETENTION_YEARS
                };
            }

            var age = CalculateAge(entity.CreatedAt, currentYear);

            if (age < MINIMUM_RETENTION_YEARS)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Không thể xóa dữ liệu dưới {MINIMUM_RETENTION_YEARS} năm. " +
                             $"Dữ liệu hiện tại {age} năm tuổi, cần lưu trữ thêm {MINIMUM_RETENTION_YEARS - age} năm nữa.",
                    EntityAgeInYears = age,
                    MinimumRetentionYears = MINIMUM_RETENTION_YEARS
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Có thể xóa dữ liệu",
                EntityAgeInYears = age,
                MinimumRetentionYears = MINIMUM_RETENTION_YEARS
            };
        }

        /// <summary>
        /// Validate archive request
        /// RULE: Can archive after 10 years
        /// </summary>
        public ValidationResult ValidateArchive<T>(T entity, int currentYear) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var age = CalculateAge(entity.CreatedAt, currentYear);

            if (age < MINIMUM_RETENTION_YEARS)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Chưa đủ điều kiện archive. Dữ liệu mới {age} năm, cần đủ {MINIMUM_RETENTION_YEARS} năm.",
                    EntityAgeInYears = age,
                    MinimumRetentionYears = MINIMUM_RETENTION_YEARS
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Có thể archive dữ liệu",
                EntityAgeInYears = age,
                MinimumRetentionYears = MINIMUM_RETENTION_YEARS
            };
        }

        /// <summary>
        /// Check if entity is immutable (accounting entries)
        /// </summary>
        public bool IsImmutableEntity<T>() where T : BaseEntity
        {
            var entityName = typeof(T).Name;
            return _immutableEntities.Contains(entityName);
        }

        /// <summary>
        /// Get entities eligible for archiving
        /// </summary>
        public IEnumerable<T> GetArchivableEntities<T>(IEnumerable<T> entities, int currentYear) where T : BaseEntity
        {
            return entities.Where(e => ValidateArchive(e, currentYear).IsValid);
        }

        private int CalculateAge(DateTime createdAt, int currentYear)
        {
            return currentYear - createdAt.Year;
        }
    }
}
