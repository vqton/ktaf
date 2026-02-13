namespace AccountingERP.Domain.ValueObjects;

using System.Globalization;

/// <summary>
/// Value Object đại diện cho số tiền với đơn vị tiền tệ
/// </summary>
/// <remarks>
/// Đảm bảo tính bất biến (immutable) và validation tại thờ điểm khởi tạo.
/// Theo TT99, số tiền phải luôn >= 0 và có đơn vị tiền tệ xác định.
/// </remarks>
public sealed class Money : IEquatable<Money>
{
    private readonly decimal _amount;
    private readonly Currency _currency;

    /// <summary>
    /// Số tiền
    /// </summary>
    /// <remarks>
    /// Luôn >= 0. Được định dạng với 2 chữ số thập phân cho VND.
    /// </remarks>
    public decimal Amount => _amount;

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    public Currency Currency => _currency;

    /// <summary>
    /// Constructor private - chỉ cho phép tạo qua factory methods
    /// </summary>
    private Money(decimal amount, Currency currency)
    {
        _amount = amount;
        _currency = currency;
    }

    /// <summary>
    /// Tạo đối tượng Money mới với validation
    /// </summary>
    /// <param name="amount">Số tiền (phải >= 0)</param>
    /// <param name="currency">Đơn vị tiền tệ</param>
    /// <returns>Money object</returns>
    /// <exception cref="ArgumentException">Ném ra khi amount &lt; 0</exception>
    /// <remarks>
    /// TT99-Đ10 khoản 2: Số tiền trong chứng từ kế toán phải được ghi bằng số và chữ.
    /// Số tiền âm không được phép trong kế toán doanh nghiệp.
    /// </remarks>
    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Số tiền không được âm. Theo TT99, số tiền trong chứng từ kế toán phải >= 0.", nameof(amount));

        return new Money(amount, currency);
    }

    /// <summary>
    /// Tạo Money với đơn vị VND
    /// </summary>
    /// <param name="amount">Số tiền VND (phải >= 0)</param>
    /// <returns>Money object với currency VND</returns>
    public static Money VND(decimal amount) => Create(amount, Currency.VND);

    /// <summary>
    /// Tạo Money với đơn vị USD
    /// </summary>
    /// <param name="amount">Số tiền USD (phải >= 0)</param>
    /// <returns>Money object với currency USD</returns>
    public static Money USD(decimal amount) => Create(amount, Currency.USD);

    /// <summary>
    /// Tạo Money với đơn vị EUR
    /// </summary>
    /// <param name="amount">Số tiền EUR (phải >= 0)</param>
    /// <returns>Money object với currency EUR</returns>
    public static Money EUR(decimal amount) => Create(amount, Currency.EUR);

    /// <summary>
    /// Tạo Money với số tiền bằng 0
    /// </summary>
    /// <param name="currency">Đơn vị tiền tệ</param>
    /// <returns>Money object với amount = 0</returns>
    public static Money Zero(Currency currency) => new Money(0, currency);

    /// <summary>
    /// Tạo bản sao với số tiền mới
    /// </summary>
    /// <param name="newAmount">Số tiền mới (phải >= 0)</param>
    /// <returns>Money object mới</returns>
    public Money WithAmount(decimal newAmount) => Create(newAmount, _currency);

    /// <summary>
    /// Cộng hai Money (phải cùng loại tiền tệ)
    /// </summary>
    /// <param name="other">Money cần cộng</param>
    /// <returns>Kết quả Money mới</returns>
    /// <exception cref="InvalidOperationException">Khi khác loại tiền tệ</exception>
    public Money Add(Money other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (_currency != other._currency)
            throw new InvalidOperationException($"Không thể cộng hai loại tiền tệ khác nhau: {_currency} và {other._currency}");
        
        return new Money(_amount + other._amount, _currency);
    }

    /// <summary>
    /// Trừ hai Money (phải cùng loại tiền tệ)
    /// </summary>
    /// <param name="other">Money cần trừ</param>
    /// <returns>Kết quả Money mới</returns>
    /// <exception cref="InvalidOperationException">Khi khác loại tiền tệ hoặc kết quả âm</exception>
    public Money Subtract(Money other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (_currency != other._currency)
            throw new InvalidOperationException($"Không thể trừ hai loại tiền tệ khác nhau: {_currency} và {other._currency}");
        
        var result = _amount - other._amount;
        if (result < 0)
            throw new InvalidOperationException("Kết quả phép trừ không được âm");
        
        return new Money(result, _currency);
    }

    /// <summary>
    /// Nhân với hệ số
    /// </summary>
    /// <param name="factor">Hệ số nhân (phải >= 0)</param>
    /// <returns>Money mới</returns>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Hệ số nhân không được âm", nameof(factor));
        return new Money(_amount * factor, _currency);
    }

    /// <summary>
    /// Kiểm tra bằng nhau
    /// </summary>
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return _amount == other._amount && _currency == other._currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(_amount, _currency);

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    /// <summary>
    /// So sánh lớn hơn
    /// </summary>
    public static bool operator >(Money left, Money right)
    {
        if (left is null || right is null) return false;
        if (left._currency != right._currency)
            throw new InvalidOperationException($"Không thể so sánh hai loại tiền tệ khác nhau: {left._currency} và {right._currency}");
        return left._amount > right._amount;
    }

    /// <summary>
    /// So sánh nhỏ hơn
    /// </summary>
    public static bool operator <(Money left, Money right)
    {
        if (left is null || right is null) return false;
        if (left._currency != right._currency)
            throw new InvalidOperationException($"Không thể so sánh hai loại tiền tệ khác nhau: {left._currency} và {right._currency}");
        return left._amount < right._amount;
    }

    /// <summary>
    /// So sánh lớn hơn hoặc bằng
    /// </summary>
    public static bool operator >=(Money left, Money right)
    {
        if (left is null || right is null) return false;
        if (left._currency != right._currency)
            throw new InvalidOperationException($"Không thể so sánh hai loại tiền tệ khác nhau: {left._currency} và {right._currency}");
        return left._amount >= right._amount;
    }

    /// <summary>
    /// So sánh nhỏ hơn hoặc bằng
    /// </summary>
    public static bool operator <=(Money left, Money right)
    {
        if (left is null || right is null) return false;
        if (left._currency != right._currency)
            throw new InvalidOperationException($"Không thể so sánh hai loại tiền tệ khác nhau: {left._currency} và {right._currency}");
        return left._amount <= right._amount;
    }

    /// <summary>
    /// Chia cho một số
    /// </summary>
    /// <param name="divisor">Số chia (phải > 0)</param>
    /// <returns>Money mới</returns>
    /// <exception cref="ArgumentException">Khi divisor <= 0</exception>
    public Money Divide(decimal divisor)
    {
        if (divisor <= 0)
            throw new ArgumentException("Số chia phải lớn hơn 0", nameof(divisor));
        return new Money(_amount / divisor, _currency);
    }

    /// <summary>
    /// Tính tổng một collection Money
    /// </summary>
    /// <param name="monies">Collection các Money object</param>
    /// <returns>Tổng Money</returns>
    public static Money Sum(IEnumerable<Money> monies)
    {
        if (monies == null || !monies.Any())
            throw new ArgumentException("Collection không được rỗng", nameof(monies));

        var first = monies.First();
        var total = monies.Skip(1).Aggregate(first._amount, (sum, m) =>
        {
            if (m._currency != first._currency)
                throw new InvalidOperationException($"Không thể cộng các loại tiền tệ khác nhau: {first._currency} và {m._currency}");
            return sum + m._amount;
        });

        return new Money(total, first._currency);
    }

    /// <summary>
    /// Định dạng chuỗi hiển thị
    /// </summary>
    public override string ToString() =>
        _currency switch
        {
            Currency.VND => $"{_amount:N2} VND",
            Currency.USD => $"{_amount:C} USD",
            Currency.EUR => $"{_amount:C} EUR",
            _ => $"{_amount} {_currency}"
        };

    /// <summary>
    /// Chuyển đổi số tiền sang định dạng chuỗi theo ngôn ngữ
    /// </summary>
    /// <param name="culture">Culture info (mặc định vi-VN)</param>
    public string ToString(IFormatProvider? culture) =>
        _amount.ToString("N2", culture ?? new CultureInfo("vi-VN"));
}

/// <summary>
/// Đơn vị tiền tệ
/// </summary>
public enum Currency
{
    /// <summary>
    /// Việt Nam Đồng - Đơn vị tiền tệ mặc định
    /// </summary>
    VND = 1,

    /// <summary>
    /// Đô la Mỹ
    /// </summary>
    USD = 2,

    /// <summary>
    /// Euro
    /// </summary>
    EUR = 3
}
