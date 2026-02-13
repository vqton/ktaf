using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Events;

namespace AccountingERP.Domain.Entities
{
    /// <summary>
    /// Base entity class cho tất cả entities trong hệ thống
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string CreatedBy { get; protected set; } = string.Empty;
        public string? UpdatedBy { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Base class cho Aggregate Roots - hỗ trợ domain events
    /// </summary>
    public abstract class AggregateRoot : BaseEntity
    {
        private List<DomainEvent> _domainEvents = new();
        
        /// <summary>
        /// Danh sách domain events chưa được dispatch
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Thêm domain event
        /// </summary>
        protected void AddDomainEvent(DomainEvent eventItem)
        {
            _domainEvents ??= new List<DomainEvent>();
            _domainEvents.Add(eventItem);
        }

        /// <summary>
        /// Xóa domain event
        /// </summary>
        public void RemoveDomainEvent(DomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        /// <summary>
        /// Xóa tất cả domain events
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        /// <summary>
        /// Kiểm tra có domain events không
        /// </summary>
        public bool HasDomainEvents => _domainEvents?.Any() ?? false;
    }
}
