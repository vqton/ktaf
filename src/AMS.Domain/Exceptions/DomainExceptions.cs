namespace AMS.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related errors.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    /// <param name="message">The error message that describes the exception.</param>
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a nested exception.
    /// </summary>
    /// <param name="message">The error message that describes the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// The name of the business rule that was violated.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleException class.
    /// </summary>
    /// <param name="ruleName">The name of the business rule that was violated.</param>
    /// <param name="message">The error message that describes the exception.</param>
    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs during data updates.
/// </summary>
public class ConcurrencyException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="message">The error message that describes the exception.</param>
    public ConcurrencyException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when validation errors occur in domain objects.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Dictionary of field names to arrays of error messages.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a single error message.
    /// </summary>
    /// <param name="message">The error message that describes the exception.</param>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a dictionary of errors.
    /// </summary>
    /// <param name="errors">Dictionary of field names to arrays of error messages.</param>
    public ValidationException(Dictionary<string, string[]> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
