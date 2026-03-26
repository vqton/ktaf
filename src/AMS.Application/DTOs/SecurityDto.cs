namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object representing a user in the system.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the department the user belongs to.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the roles assigned to the user.
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the permissions granted to the user.
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating a new user.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the department the user belongs to.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the linked employee ID.
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the roles to assign to the user upon creation.
    /// </summary>
    public List<Guid> RoleIds { get; set; } = new();
}

/// <summary>
/// Data transfer object representing a role in the system.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the display order for the role.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the permissions associated with the role.
    /// </summary>
    public List<PermissionDto> Permissions { get; set; } = new();
}

/// <summary>
/// Data transfer object representing a permission in the system.
/// </summary>
public class PermissionDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the permission.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the permission.
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the permission.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the module or feature the permission belongs to.
    /// </summary>
    public string Module { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object representing an Active Directory group mapping.
/// </summary>
public class ADGroupDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the AD group.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the AD group.
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the AD group.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the description of the AD group.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the AD group mapping is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the roles mapped to this AD group.
    /// </summary>
    public List<RoleDto> MappedRoles { get; set; } = new();
}

/// <summary>
/// Data transfer object for assigning a role to a user.
/// </summary>
public class AssignRoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// </summary>
    public Guid RoleId { get; set; }
}

/// <summary>
/// Data transfer object for mapping an AD group to a role.
/// </summary>
public class MapADGroupToRoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the AD group.
    /// </summary>
    public Guid ADGroupId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// </summary>
    public Guid RoleId { get; set; }
}
