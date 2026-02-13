using System;

namespace AccountingERP.Domain.Events
{
    /// <summary>
    /// Base class cho tất cả domain events
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
