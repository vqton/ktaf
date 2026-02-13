namespace AccountingERP.Domain.Exceptions;

/// <summary>
/// Exception cơ bản cho domain
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception khi validation thất bại
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}

/// <summary>
/// Exception khi entity không tìm thấy
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id) 
        : base($"{entityName} với ID {id} không tìm thấy") { }
}

/// <summary>
/// Exception khi vi phạm quy tắc nghiệp vụ
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
