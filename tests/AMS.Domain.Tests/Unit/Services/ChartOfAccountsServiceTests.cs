using AMS.Application.DTOs;
using AMS.Application.Services;
using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;
using AMS.Domain.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AMS.Domain.Tests.Unit.Services;

public class ChartOfAccountsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IChartOfAccountsRepository> _mockRepository;
    private readonly ChartOfAccountsService _service;

    public ChartOfAccountsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IChartOfAccountsRepository>();
        _service = new ChartOfAccountsService(_mockUnitOfWork.Object, _mockRepository.Object);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsChartOfAccountsDto()
    {
        var id = Guid.NewGuid();
        var entity = TestDataFactory.CreateValidChartOfAccounts(a => a.Id = id);

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Code.Should().Be(entity.Code);
        result.Name.Should().Be(entity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _service.GetByIdAsync(id);

        result.Should().BeNull();
    }

    #endregion

    #region GetByCodeAsync

    [Fact]
    public async Task GetByCodeAsync_WithValidCode_ReturnsChartOfAccountsDto()
    {
        var code = "ACC001";
        var entity = TestDataFactory.CreateValidChartOfAccounts(a => a.Code = code);

        _mockRepository.Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByCodeAsync(code);

        result.Should().NotBeNull();
        result!.Code.Should().Be(code);
    }

    [Fact]
    public async Task GetByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        var code = "INVALID";

        _mockRepository.Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _service.GetByCodeAsync(code);

        result.Should().BeNull();
    }

    #endregion

    #region GetByAccountNumberAsync

    [Fact]
    public async Task GetByAccountNumberAsync_WithValidNumber_ReturnsChartOfAccountsDto()
    {
        var accountNumber = 1001;
        var entity = TestDataFactory.CreateValidChartOfAccounts(a => a.AccountNumber = accountNumber);

        _mockRepository.Setup(r => r.GetByAccountNumberAsync(accountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByAccountNumberAsync(accountNumber);

        result.Should().NotBeNull();
        result!.AccountNumber.Should().Be(accountNumber);
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var page = 1;
        var pageSize = 10;
        var entities = TestDataFactory.CreateChartOfAccountsList(5);
        var totalCount = 5;

        _mockRepository.Setup(r => r.GetAllPagedAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, totalCount));

        var result = await _service.GetAllAsync(page, pageSize);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task GetAllAsync_WithLargePageSize_ClampsToMaxPageSize()
    {
        var page = 1;
        var pageSize = 1000;
        var entities = TestDataFactory.CreateChartOfAccountsList(10);

        _mockRepository.Setup(r => r.GetAllPagedAsync(page, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 10));

        var result = await _service.GetAllAsync(page, pageSize);

        result.PageSize.Should().BeLessOrEqualTo(100);
    }

    #endregion

    #region GetHierarchyAsync

    [Fact]
    public async Task GetHierarchyAsync_ReturnsHierarchicalList()
    {
        var parent = TestDataFactory.CreateValidChartOfAccounts(a =>
        {
            a.Code = "PARENT";
            a.IsDetail = false;
        });
        var child = TestDataFactory.CreateValidChartOfAccounts(a =>
        {
            a.Code = "CHILD";
            a.ParentId = parent.Id;
        });
        var entities = new List<ChartOfAccounts> { parent, child };

        _mockRepository.Setup(r => r.GetHierarchyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var result = await _service.GetHierarchyAsync();

        result.Should().NotBeNull();
    }

    #endregion

    #region GetAllActiveAsync

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveAccounts()
    {
        var entities = TestDataFactory.CreateChartOfAccountsList(3);

        _mockRepository.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var result = await _service.GetAllActiveAsync();

        result.Should().NotBeNull();
        result.Should().AllSatisfy(a => a.IsActive.Should().BeTrue());
    }

    #endregion

    #region GetChildrenAsync

    [Fact]
    public async Task GetChildrenAsync_WithValidParentId_ReturnsChildren()
    {
        var parentId = Guid.NewGuid();
        var children = TestDataFactory.CreateChartOfAccountsList(2);

        _mockRepository.Setup(r => r.GetByParentIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        var result = await _service.GetChildrenAsync(parentId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetChildrenAsync_WithNullParentId_ReturnsRootAccounts()
    {
        var roots = TestDataFactory.CreateChartOfAccountsList(2);

        _mockRepository.Setup(r => r.GetByParentIdAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roots);

        var result = await _service.GetChildrenAsync(null);

        result.Should().NotBeNull();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset,
            IsDetail = true,
            IsActive = true,
            AllowEntry = true
        };

        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByAccountNumberAsync(dto.AccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ChartOfAccounts>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(dto.Code);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyCode_ReturnsFailure()
    {
        var dto = new CreateChartOfAccountsDto
        {
            Code = "",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset
        };

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Mã tài khoản không được để trống.");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFailure()
    {
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "",
            AccountNumber = 1001,
            AccountType = AccountType.Asset
        };

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Tên tài khoản không được để trống.");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAccountNumber_ReturnsFailure()
    {
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "New Account",
            AccountNumber = 0,
            AccountType = AccountType.Asset
        };

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Số tài khoản phải lớn hơn 0.");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCode_ReturnsFailure()
    {
        var existingAccount = TestDataFactory.CreateValidChartOfAccounts(a => a.Code = "EXIST001");
        var dto = new CreateChartOfAccountsDto
        {
            Code = "EXIST001",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset
        };

        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("đã tồn tại");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateAccountNumber_ReturnsFailure()
    {
        var existingAccount = TestDataFactory.CreateValidChartOfAccounts(a => a.AccountNumber = 1001);
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset
        };

        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByAccountNumberAsync(dto.AccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Số tài khoản");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidParentId_ReturnsFailure()
    {
        var parentId = Guid.NewGuid();
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset,
            ParentId = parentId
        };

        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByAccountNumberAsync(dto.AccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cha không tồn tại");
    }

    [Fact]
    public async Task CreateAsync_WithNonDetailParent_ReturnsFailure()
    {
        var parent = TestDataFactory.CreateValidChartOfAccounts(a =>
        {
            a.IsDetail = false;
        });
        var dto = new CreateChartOfAccountsDto
        {
            Code = "NEW001",
            Name = "New Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset,
            ParentId = parent.Id
        };

        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByAccountNumberAsync(dto.AccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parent);

        var result = await _service.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("tổng hợp");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var existingEntity = TestDataFactory.CreateValidChartOfAccounts(a => a.Id = id);
        var dto = new UpdateChartOfAccountsDto
        {
            Code = "UPD001",
            Name = "Updated Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset,
            IsDetail = true,
            IsActive = true,
            AllowEntry = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);
        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.GetByAccountNumberAsync(dto.AccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ChartOfAccounts>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.UpdateAsync(id, dto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateChartOfAccountsDto
        {
            Code = "UPD001",
            Name = "Updated Account",
            AccountNumber = 1001,
            AccountType = AccountType.Asset
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _service.UpdateAsync(id, dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Không tìm thấy");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateCode_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var existingEntity = TestDataFactory.CreateValidChartOfAccounts(a => a.Id = id);
        var otherEntity = TestDataFactory.CreateValidChartOfAccounts(a => a.Code = "OTHER001");
        var dto = new UpdateChartOfAccountsDto
        {
            Code = "OTHER001",
            Name = "Updated Account",
            AccountNumber = existingEntity.AccountNumber,
            AccountType = AccountType.Asset
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);
        _mockRepository.Setup(r => r.GetByCodeAsync(dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherEntity);

        var result = await _service.UpdateAsync(id, dto);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("đã tồn tại");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var entity = TestDataFactory.CreateValidChartOfAccounts(a => a.Id = id);

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mockRepository.Setup(r => r.GetByParentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChartOfAccounts>());
        _mockRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFailure()
    {
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Không tìm thấy");
    }

    [Fact]
    public async Task DeleteAsync_WithChildren_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var entity = TestDataFactory.CreateValidChartOfAccounts(a => a.Id = id);
        var children = TestDataFactory.CreateChartOfAccountsList(2);

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mockRepository.Setup(r => r.GetByParentIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("tài khoản con");
    }

    #endregion
}