using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service implementation for handling authorization, role-based access control, and Active Directory group mapping.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IADGroupRepository _adGroupRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserADGroupRepository _userADGroupRepository;
    private readonly IADGroupRoleRepository _adGroupRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;

    /// <summary>
    /// Initializes a new instance of the AuthorizationService class.
    /// </summary>
    /// <param name="userRepository">The user repository for data access.</param>
    /// <param name="roleRepository">The role repository for data access.</param>
    /// <param name="adGroupRepository">The Active Directory group repository for data access.</param>
    /// <param name="permissionRepository">The permission repository for data access.</param>
    /// <param name="userRoleRepository">The user-role repository for data access.</param>
    /// <param name="userADGroupRepository">The user-AD group repository for data access.</param>
    /// <param name="adGroupRoleRepository">The AD group-role repository for data access.</param>
    /// <param name="rolePermissionRepository">The role-permission repository for data access.</param>
    public AuthorizationService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IADGroupRepository adGroupRepository,
        IPermissionRepository permissionRepository,
        IUserRoleRepository userRoleRepository,
        IUserADGroupRepository userADGroupRepository,
        IADGroupRoleRepository adGroupRoleRepository,
        IRolePermissionRepository rolePermissionRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _adGroupRepository = adGroupRepository;
        _permissionRepository = permissionRepository;
        _userRoleRepository = userRoleRepository;
        _userADGroupRepository = userADGroupRepository;
        _adGroupRoleRepository = adGroupRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);
        var roles = new List<RoleDto>();

        foreach (var ur in userRoles)
        {
            var role = await _roleRepository.GetByIdAsync(ur.RoleId, cancellationToken);
            if (role != null)
            {
                roles.Add(MapToRoleDto(role));
            }
        }

        return roles;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);
        var permissions = new HashSet<string>();

        foreach (var ur in userRoles)
        {
            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(ur.RoleId, cancellationToken);
            foreach (var rp in rolePermissions)
            {
                var permission = await _permissionRepository.GetByIdAsync(rp.PermissionId, cancellationToken);
                if (permission != null)
                {
                    permissions.Add(permission.PermissionName);
                }
            }

            var adGroups = await _userADGroupRepository.GetByUserIdAsync(userId, cancellationToken);
            foreach (var uag in adGroups)
            {
                var adGroupRoles = await _adGroupRoleRepository.GetByADGroupIdAsync(uag.ADGroupId, cancellationToken);
                foreach (var agr in adGroupRoles)
                {
                    var rpList = await _rolePermissionRepository.GetByRoleIdAsync(agr.RoleId, cancellationToken);
                    foreach (var rp in rpList)
                    {
                        var permission = await _permissionRepository.GetByIdAsync(rp.PermissionId, cancellationToken);
                        if (permission != null)
                        {
                            permissions.Add(permission.PermissionName);
                        }
                    }
                }
            }
        }

        return permissions;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Username))
            errors.Add("Username is required.");

        var existing = await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken);
        if (existing != null)
            errors.Add("Username already exists.");

        if (errors.Any())
            return ServiceResult<UserDto>.Failure(errors);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            DisplayName = dto.DisplayName,
            Email = dto.Email,
            Department = dto.Department,
            EmployeeId = dto.EmployeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _userRepository.AddAsync(user, cancellationToken);

        foreach (var roleId in dto.RoleIds)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "system"
            };
            await _userRoleRepository.AddAsync(userRole, cancellationToken);
        }

        return ServiceResult<UserDto>.Success(MapToUserDto(user));
    }

    /// <inheritdoc />
    public async Task<ServiceResult> AssignRoleAsync(Guid userId, Guid roleId, string assignedBy, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return ServiceResult.Failure("User not found.");

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
            return ServiceResult.Failure("Role not found.");

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };

        await _userRoleRepository.AddAsync(userRole, cancellationToken);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SyncADGroupsAsync(string username, IEnumerable<string> adGroups, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user == null)
            return ServiceResult.Failure("User not found.");

        await _userADGroupRepository.SyncUserADGroupsAsync(user.Id, adGroups, cancellationToken);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ADGroupDto>> GetMappedADGroupsAsync(CancellationToken cancellationToken = default)
    {
        var adGroups = await _adGroupRepository.GetAllAsync(cancellationToken);
        var result = new List<ADGroupDto>();

        foreach (var ag in adGroups)
        {
            var dto = new ADGroupDto
            {
                Id = ag.Id,
                GroupName = ag.GroupName,
                DisplayName = ag.DisplayName,
                Description = ag.Description,
                IsActive = ag.IsActive
            };

            var adGroupRoles = await _adGroupRoleRepository.GetByADGroupIdAsync(ag.Id, cancellationToken);
            foreach (var agr in adGroupRoles)
            {
                var role = await _roleRepository.GetByIdAsync(agr.RoleId, cancellationToken);
                if (role != null)
                {
                    dto.MappedRoles.Add(MapToRoleDto(role));
                }
            }

            result.Add(dto);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ServiceResult> MapADGroupToRoleAsync(Guid adGroupId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var adGroup = await _adGroupRepository.GetByIdAsync(adGroupId, cancellationToken);
        if (adGroup == null)
            return ServiceResult.Failure("AD Group not found.");

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
            return ServiceResult.Failure("Role not found.");

        var adGroupRole = new ADGroupRole
        {
            ADGroupId = adGroupId,
            RoleId = roleId
        };

        await _adGroupRoleRepository.AddAsync(adGroupRole, cancellationToken);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Maps a User entity to a UserDto.
    /// </summary>
    /// <param name="entity">The user entity to map.</param>
    /// <returns>The mapped UserDto.</returns>
    private static UserDto MapToUserDto(User entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username,
        DisplayName = entity.DisplayName,
        Email = entity.Email,
        Department = entity.Department,
        IsActive = entity.IsActive,
        LastLoginDate = entity.LastLoginDate
    };

    /// <summary>
    /// Maps a Role entity to a RoleDto.
    /// </summary>
    /// <param name="entity">The role entity to map.</param>
    /// <returns>The mapped RoleDto.</returns>
    private static RoleDto MapToRoleDto(Role entity) => new()
    {
        Id = entity.Id,
        RoleName = entity.RoleName,
        Description = entity.Description,
        IsActive = entity.IsActive,
        SortOrder = entity.SortOrder
    };
}
