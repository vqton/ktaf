namespace AccountingERP.Domain.Tests.Compliance;

using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class TT99D10ComplianceTests
{
    [Fact]
    public void TT99_D10_SoChungTuGoc_BatBuoc()
    {
        var act = () => ButToanKeToan.TaoMoi(
            "",
            new DateTime(2026, 2, 12),
            new DateTime(2026, 2, 12),
            "Test",
            "admin");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TT99-D10*")
            .WithMessage("*bắt buộc*");
    }

    [Fact]
    public void TT99_D10_NgayChungTuGoc_BatBuoc()
    {
        var act = () => ButToanKeToan.TaoMoi(
            "PT-2026-001",
            default,
            new DateTime(2026, 2, 12),
            "Test",
            "admin");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TT99-D10*")
            .WithMessage("*bắt buộc*");
    }

    [Fact]
    public void TT99_CamTK911()
    {
        var butToan = ButToanKeToan.TaoMoi(
            "PT-2026-001",
            new DateTime(2026, 2, 12),
            new DateTime(2026, 2, 12),
            "Test",
            "admin");

        var act = () => butToan.ThemDong(
            AccountCode.Create("911"),
            AccountCode.Create("111"),
            Money.VND(1000000));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TK 911*")
            .WithMessage("*không được sử dụng*");
    }

    [Fact]
    public void TT99_ButToanKep_TaiKhoanNoKhacTaiKhoanCo()
    {
        var butToan = ButToanKeToan.TaoMoi(
            "PT-2026-001",
            new DateTime(2026, 2, 12),
            new DateTime(2026, 2, 12),
            "Test",
            "admin");

        var tk = AccountCode.Create("111");

        var act = () => butToan.ThemDong(tk, tk, Money.VND(1000000));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*không được giống nhau*");
    }

    [Fact]
    public void TT99_ButToanKep_TongNoBangTongCo()
    {
        var butToan = ButToanKeToan.TaoMoi(
            "PT-2026-001",
            new DateTime(2026, 2, 12),
            new DateTime(2026, 2, 12),
            "Test",
            "admin");

        butToan.ThemDong(AccountCode.Create("111"), AccountCode.Create("511"), Money.VND(1000000));
        butToan.ThemDong(AccountCode.Create("112"), AccountCode.Create("511"), Money.VND(2000000));

        butToan.TongNo.Should().Be(3000000);
        butToan.TongCo.Should().Be(3000000);
        butToan.IsCanBang.Should().BeTrue();
    }
}
