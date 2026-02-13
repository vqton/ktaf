using AccountingERP.Domain.Enums;

namespace AccountingERP.Domain.Entities;

/// <summary>
/// Hệ thống tài khoản (Chart of Accounts) - Tuân thủ TT99
/// </summary>
public class Account : BaseEntity
{
    public string Code { get; private set; } = string.Empty;           // Mã tài khoản (111, 112, 131...)
    public string Name { get; private set; } = string.Empty;           // Tên tài khoản
    public string? EnglishName { get; private set; }                   // Tên tiếng Anh
    public AccountType Type { get; private set; }                      // Loại tài khoản
    public string? ParentCode { get; private set; }                    // Mã tài khoản cha (cấp trên)
    public int Level { get; private set; }                             // Cấp độ (1, 2, 3)
    public bool IsActive { get; private set; }
    public bool IsDetail { get; private set; }                         // Là TK chi tiết (cấp cuối)?
    public string? Description { get; private set; }                   // Mô tả/Diễn giải
    
    // Các yếu tố phân tích bắt buộc theo TT99
    public bool RequiresCostCenter { get; private set; }               // Yêu cầu mã bộ phận
    public bool RequiresProject { get; private set; }                  // Yêu cầu mã dự án
    public bool RequiresCustomer { get; private set; }                 // Yêu cầu mã khách hàng
    public bool RequiresSupplier { get; private set; }                 // Yêu cầu mã nhà cung cấp

    private Account() : base() { }

    public static Account Create(
        string code,
        string name,
        AccountType type,
        int level,
        bool isDetail,
        string? parentCode = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Mã tài khoản không được để trống", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên tài khoản không được để trống", nameof(name));

        if (level < 1 || level > 3)
            throw new ArgumentException("Cấp độ tài khoản phải từ 1 đến 3", nameof(level));

        return new Account
        {
            Code = code,
            Name = name,
            Type = type,
            Level = level,
            IsDetail = isDetail,
            ParentCode = parentCode,
            IsActive = true
        };
    }

    public void MarkAsRequiringCostCenter() => RequiresCostCenter = true;
    public void MarkAsRequiringProject() => RequiresProject = true;
    public void MarkAsRequiringCustomer() => RequiresCustomer = true;
    public void MarkAsRequiringSupplier() => RequiresSupplier = true;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
