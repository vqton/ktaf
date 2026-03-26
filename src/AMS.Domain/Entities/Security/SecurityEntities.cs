using System.Security.Claims;
using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginDate { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserADGroup> UserADGroups { get; set; } = new List<UserADGroup>();
}

public class Role : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<ADGroupRole> ADGroupRoles { get; set; } = new List<ADGroupRole>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
}

public class ADGroup : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserADGroup> UserADGroups { get; set; } = new List<UserADGroup>();
    public ICollection<ADGroupRole> ADGroupRoles { get; set; } = new List<ADGroupRole>();
}

public class UserADGroup
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ADGroupId { get; set; }
    public ADGroup ADGroup { get; set; } = null!;

    public DateTime SyncedAt { get; set; }
}

public class ADGroupRole
{
    public Guid ADGroupId { get; set; }
    public ADGroup ADGroup { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class Permission : BaseEntity
{
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
