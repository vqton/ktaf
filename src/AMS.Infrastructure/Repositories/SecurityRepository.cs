using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Repository implementation for Role entity operations.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the RoleRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RoleRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
        return role;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Repository implementation for Active Directory group entity operations.
/// </summary>
public class ADGroupRepository : IADGroupRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ADGroupRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ADGroupRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ADGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ADGroups
            .Include(g => g.ADGroupRoles)
            .ThenInclude(gr => gr.Role)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ADGroup?> GetByGroupNameAsync(string groupName, CancellationToken cancellationToken = default)
    {
        return await _context.ADGroups
            .FirstOrDefaultAsync(g => g.GroupName == groupName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ADGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ADGroups.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ADGroup> AddAsync(ADGroup adGroup, CancellationToken cancellationToken = default)
    {
        await _context.ADGroups.AddAsync(adGroup, cancellationToken);
        return adGroup;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ADGroup adGroup, CancellationToken cancellationToken = default)
    {
        _context.ADGroups.Update(adGroup);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Repository implementation for Permission entity operations.
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the PermissionRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PermissionRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Permission?> GetByNameAsync(string permissionName, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionName == permissionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Permission> AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Permissions.AddAsync(permission, cancellationToken);
        return permission;
    }
}

/// <summary>
/// Repository implementation for UserRole entity operations.
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRoleRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRoleRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _context.UserRoles.AddAsync(userRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
        }
    }
}

/// <summary>
/// Repository implementation for UserADGroup entity operations.
/// </summary>
public class UserADGroupRepository : IUserADGroupRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserADGroupRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserADGroupRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserADGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserADGroups
            .Where(uag => uag.UserId == userId)
            .Include(uag => uag.ADGroup)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserADGroup userAdGroup, CancellationToken cancellationToken = default)
    {
        await _context.UserADGroups.AddAsync(userAdGroup, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SyncUserADGroupsAsync(Guid userId, IEnumerable<string> adGroupNames, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserADGroups
            .Where(uag => uag.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserADGroups.RemoveRange(existing);

        foreach (var groupName in adGroupNames)
        {
            var adGroup = await _context.ADGroups.FirstOrDefaultAsync(g => g.GroupName == groupName, cancellationToken);
            if (adGroup == null)
            {
                adGroup = new ADGroup
                {
                    Id = Guid.NewGuid(),
                    GroupName = groupName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };
                await _context.ADGroups.AddAsync(adGroup, cancellationToken);
            }

            var userAdGroup = new UserADGroup
            {
                UserId = userId,
                ADGroupId = adGroup.Id,
                SyncedAt = DateTime.UtcNow
            };
            await _context.UserADGroups.AddAsync(userAdGroup, cancellationToken);
        }
    }
}

/// <summary>
/// Repository implementation for ADGroupRole entity operations.
/// </summary>
public class ADGroupRoleRepository : IADGroupRoleRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ADGroupRoleRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ADGroupRoleRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ADGroupRole>> GetByADGroupIdAsync(Guid adGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.ADGroupRoles
            .Where(agr => agr.ADGroupId == adGroupId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ADGroupRole adGroupRole, CancellationToken cancellationToken = default)
    {
        await _context.ADGroupRoles.AddAsync(adGroupRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid adGroupId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var adGroupRole = await _context.ADGroupRoles
            .FirstOrDefaultAsync(agr => agr.ADGroupId == adGroupId && agr.RoleId == roleId, cancellationToken);

        if (adGroupRole != null)
        {
            _context.ADGroupRoles.Remove(adGroupRole);
        }
    }
}

/// <summary>
/// Repository implementation for RolePermission entity operations.
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the RolePermissionRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RolePermissionRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        await _context.RolePermissions.AddAsync(rolePermission, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
        }
    }
}
