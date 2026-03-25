using Microsoft.AspNetCore.Mvc;
using AMS.Application.Common.Results;

namespace AMS.Web.Common;

public abstract class BaseController : Controller
{
    protected IActionResult Success<T>(T data) => Ok(new { success = true, data });
    protected IActionResult Success() => Ok(new { success = true });
    protected IActionResult Failure(string message) => BadRequest(new { success = false, message });
    protected IActionResult Failure(List<string> errors) => BadRequest(new { success = false, errors });
    protected IActionResult NotFound(string message = "Không tìm thấy dữ liệu") => NotFound(new { success = false, message });
    protected IActionResult Error(string message = "Lỗi hệ thống") => StatusCode(500, new { success = false, message });

    protected IActionResult FromResult<T>(ServiceResult<T> result) where T : class
    {
        if (result.IsSuccess)
            return Success(result.Data);

        if (!string.IsNullOrEmpty(result.ErrorMessage))
            return Failure(result.ErrorMessage);

        return Failure(result.Errors);
    }

    protected IActionResult FromResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return Success();

        if (!string.IsNullOrEmpty(result.ErrorMessage))
            return Failure(result.ErrorMessage);

        return Failure(result.Errors);
    }
}