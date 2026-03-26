using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;
using AMS.Domain.Entities.Tax;
using AMS.Domain.Enums;

namespace AMS.Domain.Tests.Helpers;

public static class TestDataFactory
{
    private static int _counter = 0;

    public static ChartOfAccounts CreateValidChartOfAccounts(Action<ChartOfAccounts>? configure = null)
    {
        var account = new ChartOfAccounts
        {
            Id = Guid.NewGuid(),
            Code = $"ACC{++_counter:D3}",
            Name = $"Test Account {++_counter}",
            AccountNumber = 100 + _counter,
            AccountType = AccountType.Asset,
            IsDetail = true,
            IsActive = true,
            AllowEntry = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };

        configure?.Invoke(account);
        return account;
    }

    public static ChartOfAccounts CreateAssetAccount() => CreateValidChartOfAccounts(a => a.AccountType = AccountType.Asset);

    public static ChartOfAccounts CreateLiabilityAccount() => CreateValidChartOfAccounts(a => a.AccountType = AccountType.Liability);

    public static ChartOfAccounts CreateEquityAccount() => CreateValidChartOfAccounts(a => a.AccountType = AccountType.Equity);

    public static ChartOfAccounts CreateRevenueAccount() => CreateValidChartOfAccounts(a => a.AccountType = AccountType.Revenue);

    public static ChartOfAccounts CreateExpenseAccount() => CreateValidChartOfAccounts(a => a.AccountType = AccountType.Expense);

    public static ChartOfAccounts CreateBankAccount() => CreateValidChartOfAccounts(a =>
    {
        a.IsBankAccount = true;
        a.BankAccount = "1234567890";
        a.BankName = "Test Bank";
    });

    public static ChartOfAccounts CreateInactiveAccount() => CreateValidChartOfAccounts(a => a.IsActive = false);

    public static ChartOfAccounts CreateParentAccount() => CreateValidChartOfAccounts(a =>
    {
        a.IsDetail = false;
        a.ParentId = null;
    });

    public static List<ChartOfAccounts> CreateChartOfAccountsList(int count, Action<ChartOfAccounts>? configure = null)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateValidChartOfAccounts(configure))
            .ToList();
    }

    public static CostCenter CreateValidCostCenter(Action<CostCenter>? configure = null)
    {
        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(),
            CostCenterCode = $"CC{++_counter:D3}",
            CostCenterName = $"Test Cost Center {++_counter}",
            CostCenterType = "Production",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(costCenter);
        return costCenter;
    }

    public static CashBook CreateValidCashBook(Action<CashBook>? configure = null)
    {
        var cashBook = new CashBook
        {
            Id = Guid.NewGuid(),
            Code = $"CB{++_counter:D3}",
            Name = $"Test Cash Book {++_counter}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(cashBook);
        return cashBook;
    }

    public static BankAccount CreateValidBankAccount(Action<BankAccount>? configure = null)
    {
        var bankAccount = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = $"123456{++_counter:D4}",
            AccountName = $"Test Bank Account {++_counter}",
            BankId = Guid.NewGuid(),
            IsActive = true,
            OpeningBalance = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(bankAccount);
        return bankAccount;
    }

    public static Receivable CreateValidReceivable(Action<Receivable>? configure = null)
    {
        var receivable = new Receivable
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            VoucherNo = $"RC{++_counter:D3}",
            VoucherDate = DateTime.UtcNow,
            ReceivableType = ReceivableType.TradeReceivable,
            Amount = 1000000,
            PaidAmount = 0,
            CurrencyCode = "VND",
            ExchangeRate = 1,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(receivable);
        return receivable;
    }

    public static Payable CreateValidPayable(Action<Payable>? configure = null)
    {
        var payable = new Payable
        {
            Id = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            VoucherNo = $"PY{++_counter:D3}",
            VoucherDate = DateTime.UtcNow,
            PayableType = PayableType.TradePayable,
            Amount = 1000000,
            PaidAmount = 0,
            CurrencyCode = "VND",
            ExchangeRate = 1,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(payable);
        return payable;
    }

    public static User CreateValidUser(Action<User>? configure = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = $"user{++_counter}",
            DisplayName = $"Test User {++_counter}",
            Email = $"user{_counter}@test.com",
            Department = "IT",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(user);
        return user;
    }

    public static Role CreateValidRole(Action<Role>? configure = null)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleName = $"ROLE{++_counter}",
            Description = $"Test Role {++_counter}",
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(role);
        return role;
    }

    public static ADGroup CreateValidADGroup(Action<ADGroup>? configure = null)
    {
        var adGroup = new ADGroup
        {
            Id = Guid.NewGuid(),
            GroupName = $"AD_GROUP{++_counter}",
            DisplayName = $"Test AD Group {++_counter}",
            Description = "Test AD Group Description",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(adGroup);
        return adGroup;
    }

    public static Permission CreateValidPermission(Action<Permission>? configure = null)
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            PermissionName = $"PERMISSION{++_counter}",
            Description = $"Test Permission {++_counter}",
            Module = "TEST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };
        configure?.Invoke(permission);
        return permission;
    }

    public static UserRole CreateValidUserRole(Action<UserRole>? configure = null)
    {
        var userRole = new UserRole
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            AssignedAt = DateTime.UtcNow,
            AssignedBy = "testuser"
        };
        configure?.Invoke(userRole);
        return userRole;
    }

    public static UserADGroup CreateValidUserADGroup(Action<UserADGroup>? configure = null)
    {
        var userAdGroup = new UserADGroup
        {
            UserId = Guid.NewGuid(),
            ADGroupId = Guid.NewGuid(),
            SyncedAt = DateTime.UtcNow
        };
        configure?.Invoke(userAdGroup);
        return userAdGroup;
    }

    public static ADGroupRole CreateValidADGroupRole(Action<ADGroupRole>? configure = null)
    {
        var adGroupRole = new ADGroupRole
        {
            ADGroupId = Guid.NewGuid(),
            RoleId = Guid.NewGuid()
        };
        configure?.Invoke(adGroupRole);
        return adGroupRole;
    }

    public static RolePermission CreateValidRolePermission(Action<RolePermission>? configure = null)
    {
        var rolePermission = new RolePermission
        {
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid()
        };
        configure?.Invoke(rolePermission);
        return rolePermission;
    }
}