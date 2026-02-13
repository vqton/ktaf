namespace AccountingERP.Domain.ValueObjects;

using System.Text.RegularExpressions;

/// <summary>
/// Value Object đại diện cho mã tài khoản kế toán
/// </summary>
/// <remarks>
/// Đảm bảo mã tài khoản tuân thủ chuẩn TT99/2025/TT-BTC.
/// Format: 3-4 chữ số (ví dụ: 111, 112, 131, 5111, 6321)
/// </remarks>
public sealed class AccountCode : IEquatable<AccountCode>
{
    private static readonly Regex CodePattern = new Regex("^[0-9]{3,4}$", RegexOptions.Compiled);
    private readonly string _value;

    /// <summary>
    /// Giá trị mã tài khoản
    /// </summary>
    /// <remarks>
    /// Luôn có định dạng hợp lệ: 3-4 chữ số
    /// </remarks>
    public string Value => _value;

    /// <summary>
    /// Constructor private - chỉ cho phép tạo qua factory method
    /// </summary>
    private AccountCode(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Tạo AccountCode mới với validation
    /// </summary>
    /// <param name="code">Mã tài khoản (3-4 chữ số)</param>
    /// <returns>AccountCode object</returns>
    /// <exception cref="ArgumentException">Ném ra khi format không hợp lệ</exception>
    /// <remarks>
    /// TT99-Đ10: Mã tài khoản theo hệ thống tài khoản do Bộ Tài chính ban hành.
    /// - TK cấp 1: 3 chữ số (ví dụ: 111, 112, 131)
    /// - TK cấp 2: 4 chữ số (ví dụ: 5111, 5112, 6321)
    /// </remarks>
    public static AccountCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Mã tài khoản không được để trống.", nameof(code));

        var normalizedCode = code.Trim();

        if (!CodePattern.IsMatch(normalizedCode))
            throw new ArgumentException(
                $"Mã tài khoản '{code}' không hợp lệ. Mã TK phải có 3-4 chữ số (ví dụ: 111, 112, 131, 5111).",
                nameof(code));

        // Validate cấm TK 911
        if (normalizedCode == "911")
            throw new InvalidOperationException(
                "TK 911 (Xác định kết quả kinh doanh) không được sử dụng trong bút toán thông thường. " +
                "TK này chỉ dùng để kết chuyển cuối kỳ.");

        return new AccountCode(normalizedCode);
    }

    /// <summary>
    /// Kiểm tra xem mã có phải TK cấp 1 (3 chữ số)
    /// </summary>
    public bool IsLevel1 => _value.Length == 3;

    /// <summary>
    /// Kiểm tra xem mã có phải TK cấp 2 (4 chữ số)
    /// </summary>
    public bool IsLevel2 => _value.Length == 4;

    /// <summary>
    /// Lấy mã tài khoản cấp 1 (3 chữ số đầu)
    /// </summary>
    public AccountCode GetLevel1Code() =>
        IsLevel1 ? this : new AccountCode(_value.Substring(0, 3));

    /// <summary>
    /// Kiểm tra xem hai mã có cùng nhóm TK cấp 1
    /// </summary>
    /// <param name="other">Mã TK khác</param>
    public bool IsSameLevel1Group(AccountCode other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return GetLevel1Code().Value == other.GetLevel1Code().Value;
    }

    /// <summary>
    /// Kiểm tra xem có phải TK tài sản (bắt đầu bằng 1)
    /// </summary>
    public bool IsAssetAccount => _value.StartsWith("1");

    /// <summary>
    /// Kiểm tra xem có phải TK nợ phải trả (bắt đầu bằng 3)
    /// </summary>
    public bool IsLiabilityAccount => _value.StartsWith("3");

    /// <summary>
    /// Kiểm tra xem có phải TK doanh thu (bắt đầu bằng 5)
    /// </summary>
    public bool IsRevenueAccount => _value.StartsWith("5");

    /// <summary>
    /// Kiểm tra xem có phải TK chi phí (bắt đầu bằng 6)
    /// </summary>
    public bool IsExpenseAccount => _value.StartsWith("6");

    /// <summary>
    /// Kiểm tra bằng nhau
    /// </summary>
    public bool Equals(AccountCode? other)
    {
        if (other is null) return false;
        return _value == other._value;
    }

    public override bool Equals(object? obj) => Equals(obj as AccountCode);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(AccountCode? left, AccountCode? right) => Equals(left, right);
    public static bool operator !=(AccountCode? left, AccountCode? right) => !Equals(left, right);

    /// <summary>
    /// Implicit conversion sang string
    /// </summary>
    public static implicit operator string(AccountCode accountCode) => accountCode._value;

    /// <summary>
    /// Explicit conversion từ string
    /// </summary>
    public static explicit operator AccountCode(string code) => Create(code);

    public override string ToString() => _value;
}
