# TEST STRATEGY — ERP Web App

> Tài liệu định nghĩa chiến lược kiểm thử cho dự án ERP nội bộ.
> Mọi thành viên trong team cần tuân thủ để đảm bảo chất lượng code.

---

## Mục lục

1. [Test Pyramid](#1-test-pyramid)
2. [Test Types](#2-test-types)
3. [Test Naming Convention](#3-test-naming-convention)
4. [Test Patterns](#4-test-patterns)
5. [Test Data Management](#5-test-data-management)
6. [Coverage Goals](#6-coverage-goals)
7. [Test Quality](#7-test-quality)
8. [Test Infrastructure](#8-test-infrastructure)
9. [CI/CD Integration](#9-cicd-integration)
10. [PR Checklist](#10-pr-checklist)

---

## 1. Test Pyramid

```
                    /\
                   /  \      E2E Tests (ít - 5%)
                  /----\     - Critical user flows
                 /      \    - Cross-browser testing
                /--------\  Integration Tests (vừa - 20%)
               /          \ - API endpoints
              /------------\ - Database interactions
             /              \ Unit Tests (nhiều - 75%)
            /________________\ - Business logic
```

**Nguyên tắc:**
- **75% Unit Test**: Test nhanh (<100ms), chi phí thấp, viết dễ
- **20% Integration Test**: Test tích hợp giữa các module
- **5% E2E Test**: Chậm, tốn kém, khó maintain - chỉ critical paths

---

## 2. Test Types

### 2.1 Unit Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Test logic nghiệp vụ riêng lẻ |
| **Ai viết** | Developer |
| **Khi nào chạy** | Mỗi lần build/commit |
| **Framework** | xUnit |
| **Thời gian tối đa** | 100ms/test |

**Phạm vi test:**
- Domain entities (business rules)
- Application services (use cases)
- Value Objects, DTOs validation
- Utility functions

```csharp
[Fact]
public void CalculateTotalAmount_WithMultipleItems_ReturnsSum()
{
    var order = new PurchaseOrder();
    order.AddItem(new OrderItem { Amount = 100 });
    order.AddItem(new OrderItem { Amount = 200 });

    var result = order.CalculateTotalAmount();

    Assert.Equal(300, result);
}
```

### 2.2 Integration Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Test tích hợp giữa các module |
| **Ai viết** | Developer |
| **Khi nào chạy** | Pull request |
| **Framework** | xUnit + TestHost + TestContainers |
| **Thời gian tối đa** | 5s/test |

**Phạm vi test:**
- API endpoints
- Repository với database thật (TestContainers SQL Server/PostgreSQL)
- Service tích hợp nhiều dependency

```csharp
[Fact]
public async Task CreateOrder_ReturnsCreatedResult()
{
    using var container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();
    await container.StartAsync();
    
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(container.GetConnectionString())
        .Options;
        
    using var context = new AppDbContext(options);
    var service = new OrderService(new EFRepository(context));
    
    var result = await service.Create(TestDataFactory.CreateValidOrder());
    
    Assert.True(result.IsSuccess);
    Assert.NotEqual(0, result.Data.Id);
}
```

### 2.3 E2E Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Test luồng nghiệp vụ hoàn chỉnh |
| **Ai viết** | QA / Automation Engineer |
| **Khi nào chạy** | Release candidate |
| **Framework** | Playwright |

**Phạm vi test:**
- Critical business flows (tạo đơn → duyệt → xuất hóa đơn)
- Authentication & Authorization
- Cross-browser compatibility (Chrome, Firefox, Edge)

```csharp
[Fact]
public async Task CreateOrderFlow_SmokeTest()
{
    var page = await Browser.NewPageAsync();
    
    await page.GotoAsync("/login");
    await page.FillAsync("#username", "admin");
    await page.FillAsync("#password", "admin123");
    await page.ClickAsync("#btn-login");
    
    await page.GotoAsync("/orders/new");
    await page.FillAsync("#customer-name", "Test Customer");
    await page.ClickAsync("#btn-submit");
    
    ScreenshotHelper.CaptureOnFailure(page, "CreateOrderFailed");
    
    await AssertLocator(page.Locator(".alert-success")).IsVisibleAsync();
}
```

### 2.4 Performance Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Validate performance requirements |
| **Ai viết** | Tech Lead / Senior Dev |
| **Khi nào chạy** | Weekly / Pre-release |
| **Framework** | k6, BenchmarkDotNet |

**Phạm vi test:**
- API response time (p95 < 500ms)
- Database query performance
- Load testing (100 concurrent users)
- Stress testing (peak 10x normal load)

```csharp
[Benchmark]
public void QueryOrders_ByDateRange_Benchmark()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(_connectionString)
        .Options;
        
    using var context = new AppDbContext(options);
    
    var result = context.Orders
        .Where(o => o.OrderDate >= start && o.OrderDate <= end)
        .Include(o => o.Items)
        .ToList();
}
```

### 2.5 Security Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Phát hiện lỗ hổng bảo mật |
| **Ai viết** | Security Lead |
| **Khi nào chạy** | Pre-release / Monthly |
| **Framework** | OWASP ZAP, SAST (SonarQube) |

**Phạm vi test:**
- SQL Injection
- XSS, CSRF
- Authentication bypass
- Authorization escalation
- Sensitive data exposure

---

## 3. Test Naming Convention

### 3.1 Quy tắc đặt tên

`[MethodName]_[Scenario]_[ExpectedResult]`

```csharp
[Fact]
public void CreateOrder_WithValidData_ReturnsSuccess() { }

[Fact]
public void CreateOrder_WithInvalidAmount_ThrowsArgumentException() { }

[Fact]
public void ApproveOrder_WhenStatusIsPending_SetsApproved() { }

[Theory]
[InlineData(0, false)]
[InlineData(-1, false)]
[InlineData(100, true)]
public void ValidateAmount_WithVariousValues_ReturnsExpected(bool amount, bool expected) { }
```

### 3.2 Class Naming

`[ClassName]Tests`

```csharp
public class PurchaseOrderServiceTests { }
public class OrderCalculationTests { }
public class OrderControllerTests { }
```

---

## 4. Test Patterns

### 4.1 Arrange-Act-Assert

```csharp
[Fact]
public void CalculateDiscount_WithVIPCustomer_ReturnsTenPercent()
{
    // Arrange
    var customer = new Customer { Type = CustomerType.VIP };
    var order = new PurchaseOrder { Customer = customer, TotalAmount = 1000 };

    // Act
    var discount = order.CalculateDiscount();

    // Assert
    Assert.Equal(100, discount);
}
```

### 4.2 Factory Pattern cho Test Data

```csharp
public static class TestDataFactory
{
    private static int _counter = 0;
    
    public static PurchaseOrder CreateValidOrder(Action<PurchaseOrder>? configure = null)
    {
        var order = new PurchaseOrder
        {
            Id = ++_counter,
            CustomerId = 1,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1, Amount = 100 }
            }
        };
        
        configure?.Invoke(order);
        return order;
    }

    public static Customer CreateVIPCustomer() => new Customer
    {
        Id = ++_counter,
        Type = CustomerType.VIP,
        Name = "Test VIP"
    };

    public static OrderItem CreateOrderItem(decimal amount = 100) => new OrderItem
    {
        Id = ++_counter,
        ProductId = 1,
        Quantity = 1,
        Amount = amount
    };
}
```

### 4.3 Object Mother Pattern

```csharp
public static class OrderMother
{
    public static PurchaseOrder Default() => TestDataFactory.CreateValidOrder();
    
    public static PurchaseOrder WithItems(int count)
    {
        var order = TestDataFactory.CreateValidOrder();
        order.Items = Enumerable.Range(1, count)
            .Select(i => TestDataFactory.CreateOrderItem(i * 100))
            .ToList();
        return order;
    }
    
    public static PurchaseOrder Approved() => TestDataFactory.CreateValidOrder(o => o.Status = OrderStatus.Approved);
    
    public static PurchaseOrder Cancelled() => TestDataFactory.CreateValidOrder(o => o.Status = OrderStatus.Cancelled);
}
```

### 4.4 Mocking Dependencies

```csharp
[Fact]
public void CreateOrder_WithValidData_CallsRepository()
{
    // Arrange
    var mockRepo = new Mock<IOrderRepository>();
    var mockLogger = new Mock<ILogger<OrderService>>();
    var service = new OrderService(mockRepo.Object, mockLogger.Object);

    // Act
    var result = service.Create(TestDataFactory.CreateValidOrder());

    // Assert
    mockRepo.Verify(r => r.Add(It.IsAny<PurchaseOrder>()), Times.Once);
    mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

### 4.5 Parameterized Tests (Theory)

```csharp
[Theory]
[InlineData(0, "Amount must be greater than 0")]
[InlineData(-10, "Amount must be greater than 0")]
[InlineData(-0.01, "Amount must be greater than 0")]
public void ValidateAmount_InvalidValues_ThrowsException(decimal amount, string expectedMessage)
{
    var order = TestDataFactory.CreateValidOrder();
    
    var ex = Assert.Throws<ArgumentException>(() => order.SetAmount(amount));
    
    Assert.Equal(expectedMessage, ex.Message);
}

[Theory]
[MemberData(nameof(DiscountTestData))]
public void CalculateDiscount_CustomerTypes_ReturnsExpected(CustomerType type, decimal total, decimal expected)
{
    var customer = new Customer { Type = type };
    var order = new PurchaseOrder { Customer = customer, TotalAmount = total };
    
    var discount = order.CalculateDiscount();
    
    Assert.Equal(expected, discount);
}

public static IEnumerable<object[]> DiscountTestData()
{
    yield return new object[] { CustomerType.VIP, 1000m, 100m };
    yield return new object[] { CustomerType.Regular, 1000m, 50m };
    yield return new object[] { CustomerType.New, 1000m, 0m };
}
```

### 4.6 Snapshot Testing

```csharp
[Fact]
public void SerializeOrder_ToJson_MatchesSnapshot()
{
    var order = TestDataFactory.CreateValidOrder();
    var json = JsonSerializer.Serialize(order, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    
    Snapshots.MatchOrCreateSnapshot("OrderDefault", json);
}
```

---

## 5. Test Data Management

### 5.1 Test Database Strategy

**Unit Tests:**
- Sử dụng InMemoryProvider hoặc mock
- Không cần database thật

**Integration Tests:**
- **TestContainers** cho database thật (SQL Server, PostgreSQL)
- Mỗi test chạy trong isolated container
- Database được recreate cho mỗi test class

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    protected MsSqlContainer? _database;
    
    public async Task InitializeAsync()
    {
        _database = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "TestPassword123!")
            .Build();
            
        await _database.StartAsync();
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _database?.DisposeAsync()!;
    }
}
```

### 5.2 Data Cleanup

```csharp
public class OrderRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateOrder_ThenDelete_DataIsCleaned()
    {
        var order = TestDataFactory.CreateValidOrder();
        await _repository.Create(order);
        
        await _repository.Delete(order.Id);
        
        var result = await _repository.GetById(order.Id);
        Assert.Null(result);
    }
}
```

### 5.3 Test Data Builder

```csharp
public class OrderBuilder
{
    private PurchaseOrder _order = new();
    
    public OrderBuilder WithCustomer(Customer customer)
    {
        _order.Customer = customer;
        _order.CustomerId = customer.Id;
        return this;
    }
    
    public OrderBuilder WithStatus(OrderStatus status)
    {
        _order.Status = status;
        return this;
    }
    
    public OrderBuilder WithItem(Action<OrderItemBuilder> configure)
    {
        var builder = new OrderItemBuilder();
        configure(builder);
        _order.Items.Add(builder.Build());
        return this;
    }
    
    public PurchaseOrder Build() => _order;
}

public class OrderItemBuilder
{
    private OrderItem _item = new();
    
    public OrderItemBuilder WithAmount(decimal amount)
    {
        _item.Amount = amount;
        return this;
    }
    
    public OrderItem Build() => _item;
}

// Usage
var order = new OrderBuilder()
    .WithCustomer(customer)
    .WithStatus(OrderStatus.Pending)
    .WithItem(i => i.WithAmount(100))
    .WithItem(i => i.WithAmount(200))
    .Build();
```

---

## 6. Coverage Goals

| Layer | Line Coverage | Branch Coverage | Priority |
|-------|---------------|-----------------|----------|
| **Domain** | 90%+ | 85%+ | Cao nhất - logic nghiệp vụ |
| **Application** | 80%+ | 75%+ | Cao - use cases |
| **Infrastructure | 70%+ | 60%+ | Trung bình |
| **Web** | 60%+ | 50%+ | Thấp - Manual + E2E |

### 6.1 Coverage Metrics

- **Line Coverage**: Tỷ lệ dòng code được thực thi
- **Branch Coverage**: Tất cả các nhánh (if/else, switch) được test
- **Method Coverage**: Tất cả public methods được gọi

### 6.2 Exclusions (KHÔNG cần test)

```csharp
// *.g.cs - Generated files
// *Dto.cs - Simple DTOs không có logic
// *Configuration.cs - EF configurations
// *Resource*.cs - Resource files
// *Extensions.cs - Simple extension methods
```

---

## 7. Test Quality

### 7.1 Mutation Testing

Sử dụng **Stryker.NET** để validate test effectiveness:

```bash
dotnet tool install --global dotnet-stryker
dotnet stryker --project-file tests/AMS.Domain.Tests/AMS.Domain.Tests.csproj
```

**Thresholds:**
- Mutation Score >= 80%
- Killed Mutations > 70%

### 7.2 Test Execution Time

| Test Type | Max Time | Reason |
|-----------|----------|--------|
| Unit Test | 100ms | Fast feedback loop |
| Integration Test | 5s | Database operations |
| E2E Test | 30s | Browser operations |

### 7.3 Flaky Test Handling

```csharp
[Fact]
[Retry(3)] // Retry up to 3 times
public async Task PaymentGateway_ProcessPayment_EventuallyConsistent()
{
    var result = await _paymentGateway.ProcessPayment(order);
    
    // Allow eventual consistency
    await Task.Delay(500);
    
    var status = await _paymentGateway.GetStatus(result.TransactionId);
    Assert.Equal(PaymentStatus.Completed, status);
}
```

### 7.4 Test Isolation Rules

1. **Mỗi test độc lập** - không phụ thuộc thứ tự chạy
2. **Clean state** - database được cleanup sau mỗi test
3. **No shared mutable state** - không dùng static variables
4. **Deterministic** - kết quả phải consistent

---

## 8. Test Infrastructure

### 8.1 Project Structure

```
src/
  AMS.Domain/
  AMS.Application/
  AMS.Infrastructure/
  AMS.Web/
tests/
  AMS.Domain.Tests/
    ├── AMS.Domain.Tests.csproj
    ├── Unit/
    │   ├── Entities/
    │   ├── Services/
    │   └── ValueObjects/
    └── Snapshots/
  AMS.Application.Tests/
    ├── Unit/
    └── Integration/
  AMS.Infrastructure.Tests/
    ├── Repositories/
    └── Integration/
  AMS.Web.Tests/
    ├── Controllers/
    └── Middleware/
  AMS.E2E.Tests/
    ├── Flows/
    └── Playwright/
  AMS.Performance.Tests/
    ├── Benchmarks/
    └── Load/
```

### 8.2 Test Projects Configuration

```xml
<!-- Domain.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\AMS.Domain\AMS.Domain.csproj" />
  </ItemGroup>
</Project>

<!-- Infrastructure.Tests.csproj (Integration) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="Testcontainers.MsSql" Version="3.6.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
  </ItemGroup>
</Project>

<!-- E2E.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Playwright" Version="1.40.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  </ItemGroup>
</Project>
```

### 8.3 Test Helpers

```csharp
public static class ScreenshotHelper
{
    public static void CaptureOnFailure(IPage page, string testName)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var path = Path.Combine("screenshots", $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            page.ScreenshotAsync(path).Wait();
        };
    }
}

public static class TestLogger
{
    public static ILogger<T> CreateLogger<T>()
    {
        return new XUnitLogger<T>(TestOutputHelper);
    }
}
```

---

## 9. CI/CD Integration

### 9.1 Pipeline Stages

```yaml
# azure-pipelines.yml
stages:
  - stage: Build
    jobs:
      - job: BuildAndTest
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: '8.0.x'
          
          - script: dotnet build --configuration Release
            displayName: 'Build Solution'
          
          - script: dotnet restore
            displayName: 'Restore Packages'

  - stage: Unit_Tests
    dependsOn: Build
    jobs:
      - job: RunUnitTests
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'test'
              projects: 'tests/AMS.Domain.Tests/**/*.csproj'
              arguments: '--configuration Release --collect:"XPlat Code Coverage" --logger "trx;LogFileName=results.trx"'
            displayName: 'Run Domain Unit Tests'
          
          - task: PublishTestResults@2
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/results.trx'
            displayName: 'Publish Test Results'
          
          - task: PublishCodeCoverage@1
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
            displayName: 'Publish Code Coverage'

  - stage: Integration_Tests
    dependsOn: Unit_Tests
    jobs:
      - job: RunIntegrationTests
        services:
          sqlserver:
            container: mcr.microsoft.com/mssql/server:2022-latest
        steps:
          - script: dotnet test tests/AMS.Infrastructure.Tests --configuration Release
            displayName: 'Run Integration Tests'
            env:
              ConnectionStrings__DefaultConnection: 'Server=sqlserver;Database=TestDB;User=sa;Password=TestPassword123!'

  - stage: Quality_Gates
    dependsOn: Integration_Tests
    jobs:
      - job: QualityGate
        steps:
          - task: SonarQubePrepare@5
            inputs:
              SonarQube: 'SonarQube'
              scannerMode: 'MSBuild'
          
          - script: dotnet build /p:SonarQubeTreatWarningsAsErrors=true
          
          - task: SonarQubeAnalyze@2
          
          - task: QualityGate@0
            inputs:
              SonarQube: 'SonarQube'

  - stage: E2E_Tests
    dependsOn: Quality_Gates
    condition: succeeded()
    jobs:
      - job: RunE2E
        pool:
          vmImage: 'windows-latest'
        steps:
          - task: Playwright@0
            inputs:
              version: '1.40.0'
          
          - script: npx playwright install --with-deps
          
          - script: dotnet test tests/AMS.E2E.Tests --configuration Release
            displayName: 'Run E2E Tests'

  - stage: Performance_Tests
    dependsOn: E2E_Tests
    condition: succeeded()
    jobs:
      - job: RunPerformanceTests
        steps:
          - script: dotnet test tests/AMS.Performance.Tests --configuration Release
            displayName: 'Run Performance Tests'
```

### 9.2 Quality Gates

| Metric | Threshold | Fail Stage |
|--------|-----------|------------|
| Code Coverage (Line) | >= 70% | Unit Tests |
| Code Coverage (Branch) | >= 65% | Unit Tests |
| Mutation Score | >= 80% | Unit Tests |
| Unit Tests Pass | 100% | Unit Tests |
| Integration Tests Pass | 100% | Integration Tests |
| SonarQube Quality Gate | Passed | Quality Gates |
| E2E Tests Pass | 100% | E2E Tests |
| p95 Response Time | < 500ms | Performance Tests |

### 9.3 Test Reports

- **Unit/Integration**: TRX + HTML report
- **Coverage**: Cobertura XML + HTML
- **E2E**: HTML report + screenshots on failure
- **Performance**: k6 HTML report

---

## 10. PR Checklist

### Trước khi tạo PR

- [ ] Tất cả unit tests pass
- [ ] Code coverage đạt ngưỡng quy định (70% line, 65% branch)
- [ ] Không có new warnings từ test run
- [ ] Integration tests pass (nếu có thay đổi API)
- [ ] Review code đã cover các edge cases
- [ ] Tests có tên rõ ràng theo convention
- [ ] Không có flaky tests (retry không thường xuyên)
- [ ] Test execution time < 100ms/unit test

### Reviewer Checklist

- [ ] Test cover happy path và edge cases
- [ ] Assertions đủ chi tiết (không chỉ assert true)
- [ ] Test isolation - không phụ thuộc thứ tự
- [ ] Mock đúng dependencies (không over-mock)
- [ ] Test data realistic
- [ ] Không có magic numbers - dùng constants

---

## Appendix A: Package Versions

| Package | Version |
|---------|---------|
| Microsoft.NET.Test.Sdk | 17.8.0 |
| xunit | 2.6.2 |
| xunit.runner.visualstudio | 2.5.4 |
| Moq | 4.20.70 |
| FluentAssertions | 6.12.0 |
| Testcontainers.MsSql | 3.6.0 |
| Microsoft.Playwright | 1.40.0 |
| coverlet.collector | 6.0.0 |

---

> **Cập nhật lần cuối:** tháng 3, 2026  
> **Người phụ trách:** Tech Lead  
> **Phiên bản:** 2.0  
> Mọi thắc mắc hoặc đề xuất thay đổi, tạo issue hoặc liên hệ Tech Lead.
