namespace AccountingERP.Domain.Tests.ValueObjects;

using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

/// <summary>
/// Unit tests cho Money value object
/// </summary>
public class MoneyTests
{
    #region Factory Methods Tests

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(999999999.99)]
    public void Create_WithValidAmount_ShouldCreateMoney(decimal amount)
    {
        // Act
        var money = Money.Create(amount, Currency.VND);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(Currency.VND);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-1000000)]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException(decimal amount)
    {
        // Act & Assert
        var act = () => Money.Create(amount, Currency.VND);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*không được âm*");
    }

    [Fact]
    public void VND_WithValidAmount_ShouldCreateVNDMoney()
    {
        // Arrange
        const decimal amount = 1000000;

        // Act
        var money = Money.VND(amount);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(Currency.VND);
    }

    [Fact]
    public void USD_WithValidAmount_ShouldCreateUSDMoney()
    {
        // Arrange
        const decimal amount = 100;

        // Act
        var money = Money.USD(amount);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void EUR_WithValidAmount_ShouldCreateEURMoney()
    {
        // Arrange
        const decimal amount = 100;

        // Act
        var money = Money.EUR(amount);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(Currency.EUR);
    }

    #endregion

    #region Operations Tests

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.VND(1000000);
        var money2 = Money.VND(500000);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(1500000);
        result.Currency.Should().Be(Currency.VND);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.VND(1000000);
        var money2 = Money.USD(100);

        // Act & Assert
        var act = () => money1.Add(money2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*không thể cộng*");
    }

    [Fact]
    public void Add_NullMoney_ShouldThrowArgumentNullException()
    {
        // Arrange
        var money = Money.VND(1000000);

        // Act & Assert
        var act = () => money.Add(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.VND(1000000);
        var money2 = Money.VND(300000);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(700000);
        result.Currency.Should().Be(Currency.VND);
    }

    [Fact]
    public void Subtract_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.VND(1000000);
        var money2 = Money.USD(50);

        // Act & Assert
        var act = () => money1.Subtract(money2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*không thể trừ*");
    }

    [Fact]
    public void Subtract_ResultNegative_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.VND(100000);
        var money2 = Money.VND(200000);

        // Act & Assert
        var act = () => money1.Subtract(money2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*không được âm*");
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnProduct()
    {
        // Arrange
        var money = Money.VND(100000);
        const decimal factor = 2.5m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Amount.Should().Be(250000);
        result.Currency.Should().Be(Currency.VND);
    }

    [Fact]
    public void Multiply_WithNegativeFactor_ShouldThrowArgumentException()
    {
        // Arrange
        var money = Money.VND(100000);

        // Act & Assert
        var act = () => money.Multiply(-2);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*không được âm*");
    }

    [Fact]
    public void Multiply_WithZero_ShouldReturnZeroMoney()
    {
        // Arrange
        var money = Money.VND(100000);

        // Act
        var result = money.Multiply(0);

        // Assert
        result.Amount.Should().Be(0);
    }

    #endregion

    #region WithAmount Tests

    [Fact]
    public void WithAmount_WithValidAmount_ShouldReturnNewMoney()
    {
        // Arrange
        var money = Money.VND(100000);
        const decimal newAmount = 200000;

        // Act
        var result = money.WithAmount(newAmount);

        // Assert
        result.Amount.Should().Be(newAmount);
        result.Currency.Should().Be(Currency.VND);
    }

    [Fact]
    public void WithAmount_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var money = Money.VND(100000);

        // Act & Assert
        var act = () => money.WithAmount(-100);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*không được âm*");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.VND(100000);
        var money2 = Money.VND(100000);

        // Act & Assert
        money1.Should().Be(money2);
        money1.Equals(money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.VND(100000);
        var money2 = Money.VND(200000);

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.VND(100000);
        var money2 = Money.USD(100000);

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        // Arrange
        var money = Money.VND(100000);

        // Act & Assert
        money.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var money = Money.VND(100000);
        var notMoney = "100000";

        // Act & Assert
        money.Equals(notMoney).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameAmountAndCurrency_ShouldReturnSameHashCode()
    {
        // Arrange
        var money1 = Money.VND(100000);
        var money2 = Money.VND(100000);

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_VND_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.VND(1000000);

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("VND");
        result.Should().Contain("1,000,000");
    }

    [Fact]
    public void ToString_USD_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.USD(100);

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("USD");
    }

    [Fact]
    public void ToString_WithCulture_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.VND(1000000);
        var culture = new System.Globalization.CultureInfo("vi-VN");

        // Act
        var result = money.ToString(culture);

        // Assert
        result.Should().Be("1.000.000,00");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_ZeroAmount_ShouldAllow()
    {
        // Act
        var money = Money.VND(0);

        // Assert
        money.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_MaxDecimal_ShouldWork()
    {
        // Arrange
        const decimal maxAmount = 79228162514264337593543950335m; // decimal.MaxValue

        // Act
        var money = Money.Create(maxAmount, Currency.VND);

        // Assert
        money.Amount.Should().Be(maxAmount);
    }

    [Fact]
    public void Operations_ShouldNotMutateOriginal()
    {
        // Arrange
        var original = Money.VND(100000);
        var other = Money.VND(50000);

        // Act
        var added = original.Add(other);
        var subtracted = original.Subtract(other);
        var multiplied = original.Multiply(2);

        // Assert
        original.Amount.Should().Be(100000); // Original unchanged
        added.Amount.Should().Be(150000);
        subtracted.Amount.Should().Be(50000);
        multiplied.Amount.Should().Be(200000);
    }

    #endregion
}
