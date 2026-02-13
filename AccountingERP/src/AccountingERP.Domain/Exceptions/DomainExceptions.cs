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

/// <summary>
/// Exception khi kỳ kế toán đã đóng
/// </summary>
public class PeriodClosedException : DomainException
{
    public PeriodClosedException(string message) : base(message) { }
    
    public PeriodClosedException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception khi không đủ quyền thực hiện
/// </summary>
public class InsufficientPermissionException : DomainException
{
    public string RequiredRole { get; }
    public string CurrentRole { get; }
    
    public InsufficientPermissionException(string message) : base(message) { }
    
    public InsufficientPermissionException(string requiredRole, string currentRole)
        : base($"Yêu cầu quyền '{requiredRole}' nhưng hiện tại chỉ có '{currentRole}'")
    {
        RequiredRole = requiredRole;
        CurrentRole = currentRole;
    }
}

/// <summary>
/// Exception khi vượt quá hạn mức phê duyệt
/// </summary>
public class AuthorizationLimitExceededException : DomainException
{
    public decimal RequestedAmount { get; }
    public decimal AuthorizedLimit { get; }
    
    public AuthorizationLimitExceededException(decimal requested, decimal limit)
        : base($"Số tiền {requested:N0} VND vượt quá hạn mức {limit:N0} VND")
    {
        RequestedAmount = requested;
        AuthorizedLimit = limit;
    }
}

/// <summary>
/// Exception khi phát hiện giao dịch trùng lặp
/// </summary>
public class DuplicateTransactionException : DomainException
{
    public Guid? ExistingEntryId { get; }
    
    public DuplicateTransactionException(string message) : base(message) { }
    
    public DuplicateTransactionException(string message, Guid existingEntryId)
        : base(message)
    {
        ExistingEntryId = existingEntryId;
    }
}

/// <summary>
/// Exception khi dữ liệu không nhất quán
/// </summary>
public class DataInconsistencyException : DomainException
{
    public DataInconsistencyException(string message) : base(message) { }
    
    public DataInconsistencyException(string entityType, Guid entityId, string issue)
        : base($"Dữ liệu không nhất quán: {entityType} [{entityId}] - {issue}") { }
}

/// <summary>
/// Exception khi vi phạm tuân thủ thuế
/// </summary>
public class TaxComplianceException : DomainException
{
    public string TaxRegulation { get; }
    
    public TaxComplianceException(string message) : base(message) { }
    
    public TaxComplianceException(string regulation, string message)
        : base($"[{regulation}] {message}")
    {
        TaxRegulation = regulation;
    }
}
