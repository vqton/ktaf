using System;

namespace AccountingERP.Domain.Invoicing
{
    /// <summary>
    /// Strongly-typed ID for JournalEntry
    /// </summary>
    public readonly struct JournalEntryId : IEquatable<JournalEntryId>
    {
        public Guid Value { get; }
        
        public JournalEntryId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("JournalEntryId cannot be empty", nameof(value));
            Value = value;
        }
        
        public static JournalEntryId New() => new(Guid.NewGuid());
        public static JournalEntryId FromGuid(Guid guid) => new(guid);
        
        public bool Equals(JournalEntryId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is JournalEntryId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        
        public static bool operator ==(JournalEntryId left, JournalEntryId right) => left.Equals(right);
        public static bool operator !=(JournalEntryId left, JournalEntryId right) => !left.Equals(right);
    }
}
