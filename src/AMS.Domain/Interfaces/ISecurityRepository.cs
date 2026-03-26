using AMS.Domain.Entities;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all user entities.</returns>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added user entity.</returns>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the database.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Role entity operations.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all role entities.</returns>
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role to the database.
    /// </summary>
    /// <param name="role">The role entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added role entity.</returns>
    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role in the database.
    /// </summary>
    /// <param name="role">The role entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Active Directory group entity operations.
/// </summary>
public interface IADGroupRepository
{
    /// <summary>
    /// Gets an AD group by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the AD group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The AD group entity if found; otherwise, null.</returns>
    Task<ADGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an AD group by its group name.
    /// </summary>
    /// <param name="groupName">The name of the AD group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The AD group entity if found; otherwise, null.</returns>
    Task<ADGroup?> GetByGroupNameAsync(string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all AD groups in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all AD group entities.</returns>
    Task<IEnumerable<ADGroup>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new AD group to the database.
    /// </summary>
    /// <param name="adGroup">The AD group entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added AD group entity.</returns>
    Task<ADGroup> AddAsync(ADGroup adGroup, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing AD group in the database.
    /// </summary>
    /// <param name="adGroup">The AD group entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(ADGroup adGroup, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Permission entity operations.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets a permission by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permission entity if found; otherwise, null.</returns>
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by its name.
    /// </summary>
    /// <param name="permissionName">The name of the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permission entity if found; otherwise, null.</returns>
    Task<Permission?> GetByNameAsync(string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all permission entities.</returns>
    Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new permission to the database.
    /// </summary>
    /// <param name="permission">The permission entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added permission entity.</returns>
    Task<Permission> AddAsync(Permission permission, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserRole entity operations.
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Gets all user-role associations for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-role associations.</returns>
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user-role association to the database.
    /// </summary>
    /// <param name="userRole">The user-role association to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user-role association from the database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserADGroup entity operations.
/// </summary>
public interface IUserADGroupRepository
{
    /// <summary>
    /// Gets all AD group associations for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-AD group associations.</returns>
    Task<IEnumerable<UserADGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user-AD group association to the database.
    /// </summary>
    /// <param name="userAdGroup">The user-AD group association to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(UserADGroup userAdGroup, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes AD group associations for a user, removing old ones and adding new ones.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="adGroupNames">The collection of AD group names to associate with the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SyncUserADGroupsAsync(Guid userId, IEnumerable<string> adGroupNames, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ADGroupRole entity operations.
/// </summary>
public interface IADGroupRoleRepository
{
    /// <summary>
    /// Gets all role associations for a given AD group.
    /// </summary>
    /// <param name="adGroupId">The unique identifier of the AD group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of AD group-role associations.</returns>
    Task<IEnumerable<ADGroupRole>> GetByADGroupIdAsync(Guid adGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new AD group-role association to the database.
    /// </summary>
    /// <param name="adGroupRole">The AD group-role association to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(ADGroupRole adGroupRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an AD group-role association from the database.
    /// </summary>
    /// <param name="adGroupId">The unique identifier of the AD group.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(Guid adGroupId, Guid roleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for RolePermission entity operations.
/// </summary>
public interface IRolePermissionRepository
{
    /// <summary>
    /// Gets all permission associations for a given role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of role-permission associations.</returns>
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role-permission association to the database.
    /// </summary>
    /// <param name="rolePermission">The role-permission association to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role-permission association from the database.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}
