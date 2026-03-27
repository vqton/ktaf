using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AMS.Application.Interfaces;

namespace AMS.Web.Filters;

/// <summary>
/// Authorization attribute that checks if the current user has a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    /// <summary>
    /// Initializes a new instance of the RequirePermissionAttribute class.
    /// </summary>
    /// <param name="permission">The required permission name.</param>
    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (authorizationService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value
            ?? context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new { success = false, message = "User not authenticated." });
            return;
        }

        var hasPermission = await authorizationService.HasPermissionAsync(userId, _permission);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
