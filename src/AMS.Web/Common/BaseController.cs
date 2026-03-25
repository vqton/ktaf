using Microsoft.AspNetCore.Mvc;
using AMS.Application.Common.Results;

namespace AMS.Web.Common;

/// <summary>
/// Base controller providing standardized JSON response helpers.
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// Returns a successful response with data.
    /// </summary>
    protected IActionResult Success<T>(T data) => Ok(new { success = true, data });

    /// <summary>
    /// Returns a successful response without data.
    /// </summary>
    protected IActionResult Success() => Ok(new { success = true });

    /// <summary>
    /// Returns a failure response with a single error message.
    /// </summary>
    protected IActionResult Failure(string message) => BadRequest(new { success = false, message });

    /// <summary>
    /// Returns a failure response with multiple error messages.
    /// </summary>
    protected IActionResult Failure(List<string> errors) => BadRequest(new { success = false, errors });

    /// <summary>
    /// Returns a not found response.
    /// </summary>
    protected IActionResult NotFound(string message = "Không tìm thấy dữ liệu") => NotFound(new { success = false, message });

    /// <summary>
    /// Returns a server error response.
    /// </summary>
    protected IActionResult Error(string message = "Lỗi hệ thống") => StatusCode(500, new { success = false, message });

    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult.
    /// </summary>
    protected IActionResult FromResult<T>(ServiceResult<T> result) where T : class
    {
        if (result.IsSuccess)
            return Success(result.Data);

        if (!string.IsNullOrEmpty(result.ErrorMessage))
            return Failure(result.ErrorMessage);

        return Failure(result.Errors);
    }

    /// <summary>
    /// Converts a ServiceResult to an appropriate IActionResult.
    /// </summary>
    protected IActionResult FromResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return Success();

        if (!string.IsNullOrEmpty(result.ErrorMessage))
            return Failure(result.ErrorMessage);

        return Failure(result.Errors);
    }
}