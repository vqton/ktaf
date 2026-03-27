using Microsoft.AspNetCore.Mvc;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using AMS.Application.Common.Results;

namespace AMS.Web.Controllers;

/// <summary>
/// API controller for managing authorization, roles, users, and AD group mappings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the AuthorizationController class.
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets the current authenticated user.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(cancellationToken);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        return Ok(new { success = true, data = user });
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    [HttpGet("users/{userId:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var roles = await _authorizationService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(new { success = true, data = roles });
    }

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    [HttpGet("users/{userId:guid}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var permissions = await _authorizationService.GetUserPermissionsAsync(userId, cancellationToken);
        return Ok(new { success = true, data = permissions });
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _authorizationService.CreateUserAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, errors = result.Errors });

        return CreatedAtAction(nameof(GetCurrentUser), new { id = result.Data!.Id }, new { success = true, data = result.Data });
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    [HttpPost("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var assignedBy = User.Identity?.Name ?? "system";
        var result = await _authorizationService.AssignRoleAsync(userId, dto.RoleId, assignedBy, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        return Ok(new { success = true });
    }

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    [HttpGet("users/{userId:guid}/has-permission")]
    public async Task<IActionResult> HasPermission(Guid userId, [FromQuery] string permission, CancellationToken cancellationToken)
    {
        var hasPermission = await _authorizationService.HasPermissionAsync(userId, permission, cancellationToken);
        return Ok(new { success = true, hasPermission });
    }

    /// <summary>
    /// Gets all Active Directory groups mapped to roles.
    /// </summary>
    [HttpGet("ad-groups")]
    public async Task<IActionResult> GetMappedADGroups(CancellationToken cancellationToken)
    {
        var adGroups = await _authorizationService.GetMappedADGroupsAsync(cancellationToken);
        return Ok(new { success = true, data = adGroups });
    }

    /// <summary>
    /// Maps an Active Directory group to a role.
    /// </summary>
    [HttpPost("ad-groups/{adGroupId:guid}/roles")]
    public async Task<IActionResult> MapADGroupToRole(Guid adGroupId, [FromBody] MapADGroupToRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _authorizationService.MapADGroupToRoleAsync(adGroupId, dto.RoleId, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        return Ok(new { success = true });
    }

    /// <summary>
    /// Synchronizes Active Directory groups for a user.
    /// </summary>
    [HttpPost("sync-ad-groups")]
    public async Task<IActionResult> SyncADGroups([FromBody] SyncADGroupsRequest request, CancellationToken cancellationToken)
    {
        var result = await _authorizationService.SyncADGroupsAsync(request.Username, request.ADGroups, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        return Ok(new { success = true });
    }
}

/// <summary>
/// Request model for synchronizing AD groups.
/// </summary>
public class SyncADGroupsRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of AD group names.
    /// </summary>
    public IEnumerable<string> ADGroups { get; set; } = Enumerable.Empty<string>();
}
