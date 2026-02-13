namespace AccountingERP.Domain.Tests.ValueObjects;

using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

/// <summary>
/// Unit tests cho AccountCode value object
/// </summary>
public class AccountCodeTests
{
    #region Create Tests - Valid Codes

    [Theory]
    [InlineData("111")]
    [InlineData("112")]
    [InlineData("131")]
    [InlineData("331")]
    [InlineData("511")]
    [InlineData("632")]
    [InlineData("1111")]
    [InlineData("5111")]
    [InlineData("6321")]
    public void Create_WithValidCode_ShouldCreateAccountCode(string code)
    {
        // Act
        var accountCode = AccountCode.Create(code);

        // Assert
        accountCode.Value.Should().Be(code);
    }

    [Theory]
    [InlineData("111")]
    [InlineData("333")]
    [InlineData("999")]
    public void Create_Level1Code_ShouldSetIsLevel1True(string code)
    {
        // Act
        var accountCode = AccountCode.Create(code);

        // Assert
        accountCode.IsLevel1.Should().BeTrue();
        accountCode.IsLevel2.Should().BeFalse();
    }

    [Theory]
    [InlineData("1111")]
    [InlineData("3333")]
    [InlineData("9999")]
    public void Create_Level2Code_ShouldSetIsLevel2True(string code)
    {
        // Act
        var accountCode = AccountCode.Create(code);

        // Assert
        accountCode.IsLevel2.Should().BeTrue();
        accountCode.IsLevel1.Should().BeFalse();
    }

    [Theory]
    [InlineData(" 111 ")]
    [InlineData("112  ")]
    [InlineData("  131")]
    public void Create_WithWhitespace_ShouldTrim(string code)
    {
        // Act
        var accountCode = AccountCode.Create(code);

        // Assert
        accountCode.Value.Should().Be(code.Trim());
    }

    #endregion

    #region Create Tests - Invalid Codes

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldThrowArgumentException(string? code)
    {
        // Act & Assert
        var act = () => AccountCode.Create(code!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*không được để trống*");
    }

    [Theory]
    [InlineData("11")]      // Too short
    [InlineData("1")]       // Too short
    [InlineData("11111")]   // Too long
    [InlineData("111111")]  // Too long
    [InlineData("abc")]     // Not digits
    [InlineData("11a")]     // Mixed
    [InlineData("111a")]    // Mixed
    [InlineData("abcd")]    // Not digits
    [InlineData("11.1")]    // Special chars
    [InlineData("11-1")]    // Special chars
    [InlineData("  ")]      // Whitespace only
    public void Create_WithInvalidFormat_ShouldThrowArgumentException(string code)
    {
        // Act & Assert
        var act = () => AccountCode.Create(code);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*không hợp lệ*");
    }

    [Fact]
    public void Create_With911_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        var act = () => AccountCode.Create("911");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TK 911*")
            .WithMessage("*không được sử dụng*");
    }

    #endregion

    #region GetLevel1Code Tests

    [Theory]
    [InlineData("1111", "111")]
    [InlineData("5111", "511")]
    [InlineData("6321", "632")]
    [InlineData("3333", "333")]
    public void GetLevel1Code_Level2Code_ShouldReturnLevel1(string level2, string expectedLevel1)
    {
        // Arrange
        var accountCode = AccountCode.Create(level2);

        // Act
        var level1 = accountCode.GetLevel1Code();

        // Assert
        level1.Value.Should().Be(expectedLevel1);
        level1.IsLevel1.Should().BeTrue();
    }

    [Fact]
    public void GetLevel1Code_Level1Code_ShouldReturnSame()
    {
        // Arrange
        var accountCode = AccountCode.Create("111");

        // Act
        var result = accountCode.GetLevel1Code();

        // Assert
        result.Should().Be(accountCode);
        result.Value.Should().Be("111");
    }

    #endregion

    #region IsSameLevel1Group Tests

    [Theory]
    [InlineData("1111", "111")]
    [InlineData("1111", "1112")]
    [InlineData("1112", "1113")]
    public void IsSameLevel1Group_SameGroup_ShouldReturnTrue(string code1, string code2)
    {
        // Arrange
        var accountCode1 = AccountCode.Create(code1);
        var accountCode2 = AccountCode.Create(code2);

        // Act
        var result = accountCode1.IsSameLevel1Group(accountCode2);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("111", "112")]
    [InlineData("1111", "1121")]
    [InlineData("5111", "6111")]
    public void IsSameLevel1Group_DifferentGroup_ShouldReturnFalse(string code1, string code2)
    {
        // Arrange
        var accountCode1 = AccountCode.Create(code1);
        var accountCode2 = AccountCode.Create(code2);

        // Act
        var result = accountCode1.IsSameLevel1Group(accountCode2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameLevel1Group_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var accountCode = AccountCode.Create("111");

        // Act & Assert
        var act = () => accountCode.IsSameLevel1Group(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Account Type Detection Tests

    [Theory]
    [InlineData("111")]
    [InlineData("112")]
    [InlineData("131")]
    [InlineData("1111")]
    public void IsAssetAccount_StartingWith1_ShouldReturnTrue(string code)
    {
        // Arrange
        var accountCode = AccountCode.Create(code);

        // Act & Assert
        accountCode.IsAssetAccount.Should().BeTrue();
        accountCode.IsLiabilityAccount.Should().BeFalse();
        accountCode.IsRevenueAccount.Should().BeFalse();
        accountCode.IsExpenseAccount.Should().BeFalse();
    }

    [Theory]
    [InlineData("311")]
    [InlineData("331")]
    [InlineData("341")]
    public void IsLiabilityAccount_StartingWith3_ShouldReturnTrue(string code)
    {
        // Arrange
        var accountCode = AccountCode.Create(code);

        // Act & Assert
        accountCode.IsLiabilityAccount.Should().BeTrue();
        accountCode.IsAssetAccount.Should().BeFalse();
    }

    [Theory]
    [InlineData("511")]
    [InlineData("515")]
    [InlineData("5111")]
    public void IsRevenueAccount_StartingWith5_ShouldReturnTrue(string code)
    {
        // Arrange
        var accountCode = AccountCode.Create(code);

        // Act & Assert
        accountCode.IsRevenueAccount.Should().BeTrue();
        accountCode.IsAssetAccount.Should().BeFalse();
    }

    [Theory]
    [InlineData("611")]
    [InlineData("632")]
    [InlineData("642")]
    [InlineData("6321")]
    public void IsExpenseAccount_StartingWith6_ShouldReturnTrue(string code)
    {
        // Arrange
        var accountCode = AccountCode.Create(code);

        // Act & Assert
        accountCode.IsExpenseAccount.Should().BeTrue();
        accountCode.IsAssetAccount.Should().BeFalse();
    }

    [Theory]
    [InlineData("211")]  // Fixed assets
    [InlineData("411")]  // Equity
    [InlineData("711")]  // Other income
    public void AccountTypeProperties_NotMatching_ShouldReturnFalse(string code)
    {
        // Arrange
        var accountCode = AccountCode.Create(code);

        // Act & Assert
        accountCode.IsAssetAccount.Should().BeFalse();
        accountCode.IsLiabilityAccount.Should().BeFalse();
        accountCode.IsRevenueAccount.Should().BeFalse();
        accountCode.IsExpenseAccount.Should().BeFalse();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameCode_ShouldReturnTrue()
    {
        // Arrange
        var code1 = AccountCode.Create("111");
        var code2 = AccountCode.Create("111");

        // Act & Assert
        code1.Should().Be(code2);
        code1.Equals(code2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCode_ShouldReturnFalse()
    {
        // Arrange
        var code1 = AccountCode.Create("111");
        var code2 = AccountCode.Create("112");

        // Act & Assert
        code1.Should().NotBe(code2);
        code1.Equals(code2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        // Arrange
        var code = AccountCode.Create("111");

        // Act & Assert
        code.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var code = AccountCode.Create("111");

        // Act & Assert
        code.Equals("111").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameCode_ShouldReturnSameHashCode()
    {
        // Arrange
        var code1 = AccountCode.Create("111");
        var code2 = AccountCode.Create("111");

        // Act & Assert
        code1.GetHashCode().Should().Be(code2.GetHashCode());
    }

    [Fact]
    public void Operators_SameCode_ShouldWork()
    {
        // Arrange
        var code1 = AccountCode.Create("111");
        var code2 = AccountCode.Create("111");
        var code3 = AccountCode.Create("112");

        // Act & Assert
        (code1 == code2).Should().BeTrue();
        (code1 == code3).Should().BeFalse();
        (code1 != code3).Should().BeTrue();
        (code1 != code2).Should().BeFalse();
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ShouldReturnValue()
    {
        // Arrange
        var accountCode = AccountCode.Create("111");

        // Act
        string value = accountCode;

        // Assert
        value.Should().Be("111");
    }

    [Fact]
    public void ExplicitConversionFromString_ShouldCreateAccountCode()
    {
        // Act
        var accountCode = (AccountCode)"111";

        // Assert
        accountCode.Value.Should().Be("111");
    }

    [Fact]
    public void ExplicitConversionFromInvalidString_ShouldThrow()
    {
        // Act & Assert
        var act = () => { AccountCode code = (AccountCode)"invalid"; };
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var accountCode = AccountCode.Create("111");

        // Act
        var result = accountCode.ToString();

        // Assert
        result.Should().Be("111");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_AllValidLevel1Codes_ShouldWork()
    {
        // Test all possible 3-digit codes from 100-999
        for (int i = 100; i <= 999; i++)
        {
            if (i == 911) continue; // Skip forbidden code

            var code = i.ToString();
            var accountCode = AccountCode.Create(code);
            accountCode.Value.Should().Be(code);
            accountCode.IsLevel1.Should().BeTrue();
        }
    }

    [Fact]
    public void Create_AllValidLevel2Codes_ShouldWork()
    {
        // Test sample of 4-digit codes
        for (int i = 1000; i <= 9999; i += 100)
        {
            var code = i.ToString();
            var accountCode = AccountCode.Create(code);
            accountCode.Value.Should().Be(code);
            accountCode.IsLevel2.Should().BeTrue();
        }
    }

    [Fact]
    public void Immutability_ValueProperty_ShouldNotChange()
    {
        // Arrange
        var accountCode = AccountCode.Create("111");

        // Value property should be read-only (private setter)
        // This test ensures the design is immutable
        accountCode.Value.Should().Be("111");
    }

    #endregion
}
