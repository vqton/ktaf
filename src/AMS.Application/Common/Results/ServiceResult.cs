namespace AMS.Application.Common.Results;

/// <summary>
/// Represents the result of a service operation that returns data.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ServiceResult<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The data returned by the operation (null if failed).
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// List of error messages for validation failures.
    /// </summary>
    public List<string> Errors { get; private set; } = new();

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <returns>A successful ServiceResult with the data.</returns>
    public static ServiceResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed ServiceResult with the error message.</returns>
    public static ServiceResult<T> Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, Errors = new List<string> { error } };

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="errors">List of error messages.</param>
    /// <returns>A failed ServiceResult with the error messages.</returns>
    public static ServiceResult<T> Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    /// <summary>
    /// Creates a failed result with no specific error message.
    /// </summary>
    /// <returns>A failed ServiceResult.</returns>
    public static ServiceResult<T> Failure() =>
        new() { IsSuccess = false };
}

/// <summary>
/// Represents the result of a service operation that doesn't return data.
/// </summary>
public class ServiceResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// List of error messages for validation failures.
    /// </summary>
    public List<string> Errors { get; private set; } = new();

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful ServiceResult.</returns>
    public static ServiceResult Success() =>
        new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed ServiceResult with the error message.</returns>
    public static ServiceResult Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, Errors = new List<string> { error } };

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="errors">List of error messages.</param>
    /// <returns>A failed ServiceResult with the error messages.</returns>
    public static ServiceResult Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };
}