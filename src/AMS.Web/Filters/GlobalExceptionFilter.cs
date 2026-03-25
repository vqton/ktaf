using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace AMS.Web.Filters;

/// <summary>
/// Global exception filter that handles unhandled exceptions and returns appropriate responses.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionFilter class.
    /// </summary>
    /// <param name="logger">The Serilog logger instance.</param>
    public GlobalExceptionFilter(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
        var requestPath = context.HttpContext.Request.Path;

        _logger.Fatal(exception,
            "Unhandled exception at {Controller}/{Action}. Path: {Path}",
            controllerName, actionName, requestPath);

        var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (isAjax)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
            })
            {
                StatusCode = 500
            };
        }
        else
        {
            context.Result = new ViewResult
            {
                ViewName = "Error",
                StatusCode = 500
            };
        }

        context.ExceptionHandled = true;
    }
}