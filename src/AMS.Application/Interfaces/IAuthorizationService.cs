using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for handling authorization, role-based access control, and Active Directory group mapping.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Gets the current logged-in user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current user DTO or null if not authenticated.</returns>
    Task<UserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of role DTOs assigned to the user.</returns>
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user based on direct role assignments and AD group mappings.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of permission names.</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user with optional role assignments.
    /// </summary>
    /// <param name="dto">The create user DTO containing user details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result containing the created user DTO.</returns>
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role to assign.</param>
    /// <param name="assignedBy">The username of the user performing the assignment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result indicating success or failure.</returns>
    Task<ServiceResult> AssignRoleAsync(Guid userId, Guid roleId, string assignedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes Active Directory groups for a user, creating new mappings and removing obsolete ones.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="adGroups">The collection of AD group names to synchronize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result indicating success or failure.</returns>
    Task<ServiceResult> SyncADGroupsAsync(string username, IEnumerable<string> adGroups, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permission">The name of the permission to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all Active Directory groups that are mapped to roles in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of AD group DTOs with their mapped roles.</returns>
    Task<IEnumerable<ADGroupDto>> GetMappedADGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps an Active Directory group to a role for automatic role assignment based on AD membership.
    /// </summary>
    /// <param name="adGroupId">The unique identifier of the AD group.</param>
    /// <param name="roleId">The unique identifier of the role to map.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A service result indicating success or failure.</returns>
    Task<ServiceResult> MapADGroupToRoleAsync(Guid adGroupId, Guid roleId, CancellationToken cancellationToken = default);
}
