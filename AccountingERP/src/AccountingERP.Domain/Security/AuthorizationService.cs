using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Security
{
    /// <summary>
    /// Domain Service: Quản lý phân quyền và xác thực
    /// TT99-Điều 14: Kiểm soát nội bộ
    /// </summary>
    public interface IAuthorizationService
    {
        bool CanCreateEntry(User user);
        bool CanPostEntry(User user, Money amount);
        bool CanApproveEntry(User user, Money amount, Guid entryCreatorId);
        bool CanClosePeriod(User user, PeriodType periodType);
        bool CanReopenPeriod(User user);
        bool CanVoidEntry(User user, Money amount);
        bool CanCreateInvoice(User user);
        bool CanIssueInvoice(User user);
        bool CanCancelInvoice(User user);
        void ValidateAuthorization(User user, Permission permission, Money? amount = null, Guid? resourceCreatorId = null);
        AuthorizationResult Authorize(User user, Permission permission, Money? amount = null, Guid? resourceCreatorId = null);
    }

    /// <summary>
    /// Implementation of Authorization Service
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationMatrix _matrix;

        public AuthorizationService(AuthorizationMatrix matrix)
        {
            _matrix = matrix ?? new AuthorizationMatrix();
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể tạo bút toán không
        /// </summary>
        public bool CanCreateEntry(User user)
        {
            return user.HasPermission(Permission.CreateEntry) && user.IsActive;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể ghi sổ không
        /// </summary>
        public bool CanPostEntry(User user, Money amount)
        {
            if (!user.HasPermission(Permission.PostEntry) || !user.IsActive)
                return false;

            var limit = _matrix.GetPostingLimit(user.Role);
            return amount.Amount <= limit;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể phê duyệt không
        /// </summary>
        public bool CanApproveEntry(User user, Money amount, Guid entryCreatorId)
        {
            // Separation of duties: Creator cannot approve their own entry
            if (user.Id == entryCreatorId)
                return false;

            if (!user.HasPermission(Permission.ApproveEntry) || !user.IsActive)
                return false;

            var limit = _matrix.GetApprovalLimit(user.Role);
            return amount.Amount <= limit;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể đóng kỳ không
        /// </summary>
        public bool CanClosePeriod(User user, PeriodType periodType)
        {
            if (!user.IsActive)
                return false;

            var requiredRole = _matrix.GetPeriodClosingAuthority(periodType);
            return HasRequiredRole(user.Role, requiredRole);
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể mở lại kỳ không
        /// </summary>
        public bool CanReopenPeriod(User user)
        {
            // Mở lại kỳ yêu cầu CFO hoặc CEO
            return user.IsActive && 
                   (user.Role == UserRole.CFO || user.Role == UserRole.CEO);
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể hủy bút toán không
        /// </summary>
        public bool CanVoidEntry(User user, Money amount)
        {
            if (!user.HasPermission(Permission.VoidEntry) || !user.IsActive)
                return false;

            var limit = _matrix.GetVoidLimit(user.Role);
            return amount.Amount <= limit;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể tạo hóa đơn không
        /// </summary>
        public bool CanCreateInvoice(User user)
        {
            return user.HasPermission(Permission.CreateInvoice) && user.IsActive;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể phát hành hóa đơn không
        /// </summary>
        public bool CanIssueInvoice(User user)
        {
            return user.HasPermission(Permission.IssueInvoice) && user.IsActive;
        }

        /// <summary>
        /// Kiểm tra ngườii dùng có thể hủy hóa đơn không
        /// </summary>
        public bool CanCancelInvoice(User user)
        {
            return user.HasPermission(Permission.CancelInvoice) && user.IsActive;
        }

        /// <summary>
        /// Validate và throw exception nếu không được phép
        /// </summary>
        public void ValidateAuthorization(User user, Permission permission, Money? amount = null, Guid? resourceCreatorId = null)
        {
            var result = Authorize(user, permission, amount, resourceCreatorId);
            
            if (!result.IsAuthorized)
            {
                throw new InsufficientPermissionException(result.RequiredRole.ToString(), user.Role.ToString());
            }
        }

        /// <summary>
        /// Thực hiện kiểm tra phân quyền chi tiết
        /// </summary>
        public AuthorizationResult Authorize(User user, Permission permission, Money? amount = null, Guid? resourceCreatorId = null)
        {
            // Check 1: User active
            if (!user.IsActive)
            {
                return AuthorizationResult.Failure("Tài khoản đã bị khóa");
            }

            // Check 2: Separation of duties
            if (resourceCreatorId.HasValue && user.Id == resourceCreatorId.Value)
            {
                return AuthorizationResult.Failure("Ngườii tạo không được phê duyệt chính bút toán của mình (Phân quyền)");
            }

            // Check 3: Specific permission
            if (!user.HasPermission(permission))
            {
                return AuthorizationResult.Failure($"Không có quyền {permission}");
            }

            // Check 4: Amount limit
            if (amount != null)
            {
                var limit = _matrix.GetLimitForPermission(user.Role, permission);
                if (amount.Amount > limit)
                {
                    return AuthorizationResult.Failure(
                        $"Số tiền {amount.Amount:N0} VND vượt quá hạn mức {limit:N0} VND cho vai trò {user.Role}");
                }
            }

            // Check 5: Dual authorization for high amounts
            if (amount != null && amount.Amount > 500_000_000m)
            {
                return AuthorizationResult.RequireDualAuth(
                    "Giao dịch > 500 triệu đồng yêu cầu phê duyệt kép");
            }

            return AuthorizationResult.Success();
        }

        private bool HasRequiredRole(UserRole userRole, UserRole requiredRole)
        {
            var roleHierarchy = new Dictionary<UserRole, int>
            {
                [UserRole.Accountant] = 1,
                [UserRole.SeniorAccountant] = 2,
                [UserRole.ChiefAccountant] = 3,
                [UserRole.CFO] = 4,
                [UserRole.CEO] = 5
            };

            return roleHierarchy[userRole] >= roleHierarchy[requiredRole];
        }
    }

    /// <summary>
    /// Ngườii dùng hệ thống
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public List<Permission> Permissions { get; set; } = new();

        public bool HasPermission(Permission permission)
        {
            return Permissions.Contains(permission) || RoleHasPermission(Role, permission);
        }

        private bool RoleHasPermission(UserRole role, Permission permission)
        {
            // Define default permissions for each role
            var rolePermissions = new Dictionary<UserRole, List<Permission>>
            {
                [UserRole.Accountant] = new()
                {
                    Permission.ViewEntries,
                    Permission.CreateEntry,
                    Permission.PostEntry
                },
                [UserRole.SeniorAccountant] = new()
                {
                    Permission.ViewEntries,
                    Permission.CreateEntry,
                    Permission.PostEntry,
                    Permission.ApproveEntry,
                    Permission.CreateInvoice,
                    Permission.IssueInvoice
                },
                [UserRole.ChiefAccountant] = new()
                {
                    Permission.ViewEntries,
                    Permission.CreateEntry,
                    Permission.PostEntry,
                    Permission.ApproveEntry,
                    Permission.VoidEntry,
                    Permission.CreateInvoice,
                    Permission.IssueInvoice,
                    Permission.CancelInvoice,
                    Permission.ClosePeriod
                },
                [UserRole.CFO] = new()
                {
                    Permission.ViewEntries,
                    Permission.CreateEntry,
                    Permission.PostEntry,
                    Permission.ApproveEntry,
                    Permission.VoidEntry,
                    Permission.CreateInvoice,
                    Permission.IssueInvoice,
                    Permission.CancelInvoice,
                    Permission.ClosePeriod,
                    Permission.ReopenPeriod,
                    Permission.ViewReports
                },
                [UserRole.CEO] = new()
                {
                    Permission.ViewEntries,
                    Permission.CreateEntry,
                    Permission.PostEntry,
                    Permission.ApproveEntry,
                    Permission.VoidEntry,
                    Permission.CreateInvoice,
                    Permission.IssueInvoice,
                    Permission.CancelInvoice,
                    Permission.ClosePeriod,
                    Permission.ReopenPeriod,
                    Permission.ViewReports,
                    Permission.AdminAccess
                }
            };

            return rolePermissions.ContainsKey(role) && rolePermissions[role].Contains(permission);
        }
    }

    /// <summary>
    /// Vai trò ngườii dùng
    /// </summary>
    public enum UserRole
    {
        Accountant = 1,         // Kế toán viên
        SeniorAccountant = 2,   // Kế toán viên cao cấp
        ChiefAccountant = 3,    // Kế toán trưởng
        CFO = 4,                // Giám đốc tài chính
        CEO = 5                 // Tổng giám đốc
    }

    /// <summary>
    /// Quyền hạn
    /// </summary>
    public enum Permission
    {
        ViewEntries,
        CreateEntry,
        PostEntry,
        ApproveEntry,
        VoidEntry,
        CreateInvoice,
        IssueInvoice,
        CancelInvoice,
        ClosePeriod,
        ReopenPeriod,
        ViewReports,
        AdminAccess
    }

    /// <summary>
    /// Ma trận phân quyền
    /// </summary>
    public class AuthorizationMatrix
    {
        // Hạn mức ghi sổ (VND)
        private readonly Dictionary<UserRole, decimal> _postingLimits = new()
        {
            [UserRole.Accountant] = 50_000_000m,
            [UserRole.SeniorAccountant] = 200_000_000m,
            [UserRole.ChiefAccountant] = 500_000_000m,
            [UserRole.CFO] = 1_000_000_000m,
            [UserRole.CEO] = decimal.MaxValue
        };

        // Hạn mức phê duyệt (VND)
        private readonly Dictionary<UserRole, decimal> _approvalLimits = new()
        {
            [UserRole.Accountant] = 0m, // Accountants cannot approve
            [UserRole.SeniorAccountant] = 200_000_000m,
            [UserRole.ChiefAccountant] = 500_000_000m,
            [UserRole.CFO] = 1_000_000_000m,
            [UserRole.CEO] = decimal.MaxValue
        };

        // Hạn mức hủy bỏ (VND)
        private readonly Dictionary<UserRole, decimal> _voidLimits = new()
        {
            [UserRole.Accountant] = 0m,
            [UserRole.SeniorAccountant] = 50_000_000m,
            [UserRole.ChiefAccountant] = 200_000_000m,
            [UserRole.CFO] = 500_000_000m,
            [UserRole.CEO] = decimal.MaxValue
        };

        // Quyền đóng kỳ
        private readonly Dictionary<PeriodType, UserRole> _periodClosingAuthorities = new()
        {
            [PeriodType.Month] = UserRole.ChiefAccountant,
            [PeriodType.Quarter] = UserRole.CFO,
            [PeriodType.Year] = UserRole.CEO
        };

        public decimal GetPostingLimit(UserRole role) => _postingLimits.GetValueOrDefault(role, 0);
        public decimal GetApprovalLimit(UserRole role) => _approvalLimits.GetValueOrDefault(role, 0);
        public decimal GetVoidLimit(UserRole role) => _voidLimits.GetValueOrDefault(role, 0);
        public UserRole GetPeriodClosingAuthority(PeriodType periodType) => _periodClosingAuthorities[periodType];

        public decimal GetLimitForPermission(UserRole role, Permission permission)
        {
            return permission switch
            {
                Permission.PostEntry => GetPostingLimit(role),
                Permission.ApproveEntry => GetApprovalLimit(role),
                Permission.VoidEntry => GetVoidLimit(role),
                _ => decimal.MaxValue
            };
        }
    }

    /// <summary>
    /// Kết quả phân quyền
    /// </summary>
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public bool RequiresDualAuthorization { get; private set; }
        public UserRole RequiredRole { get; private set; }

        public static AuthorizationResult Success()
        {
            return new AuthorizationResult { IsAuthorized = true };
        }

        public static AuthorizationResult Failure(string reason)
        {
            return new AuthorizationResult { IsAuthorized = false, Reason = reason };
        }

        public static AuthorizationResult RequireDualAuth(string reason)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = reason,
                RequiresDualAuthorization = true
            };
        }
    }

    public enum PeriodType
    {
        Month,
        Quarter,
        Year
    }
}
