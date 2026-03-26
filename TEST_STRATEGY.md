# TEST STRATEGY — ERP Web App

> Tài liệu này định nghĩa chiến lược kiểm thử cho dự án ERP nội bộ.
> Mọi thành viên trong team cần tuân thủ để đảm bảo chất lượng code.

---

## Mục lục

1. [Test Pyramid](#1-test-pyramid)
2. [Test Types](#2-test-types)
3. [Test Naming Convention](#3-test-naming-convention)
4. [Test Patterns](#4-test-patterns)
5. [Coverage Goals](#5-coverage-goals)
6. [Test Infrastructure](#6-test-infrastructure)
7. [CI/CD Integration](#7-cicd-integration)

---

## 1. Test Pyramid

```
                    /\
                   /  \      E2E Tests (ít)
                  /----\     - Critical user flows
                 /      \    - Cross-browser testing
                /--------\  Integration Tests (vừa)
               /          \ - API endpoints
              /------------\ - Database interactions
             /              \ Unit Tests (nhiều)
            /________________\ - Business logic
```

**Nguyên tắc:**
- **Nhiều Unit Test**: Test nhanh, chi phí thấp, viết dễ
- **Vừa Integration Test**: Test tích hợp giữa các module
- **Ít E2E Test**: Chậm, tốn kém, khó maintain

---

## 2. Test Types

### 2.1 Unit Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Test logic nghiệp vụ riêng lẻ |
| **Ai viết** | Developer |
| **Khi nào chạy** | Mỗi lần build/commit |
| **Framework** | xUnit, NUnit |

**Phạm vi test:**
- Domain entities (business rules)
- Application services (use cases)
- DTOs validation

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
| **Framework** | xUnit + TestHost |

**Phạm vi test:**
- API endpoints
- Repository với database thật (hoặc in-memory)
- Service tích hợp nhiều dependency

```csharp
[Fact]
public async Task CreateOrder_ReturnsCreatedResult()
{
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync("/api/orders", new 
    {
        CustomerId = 1,
        Items = new[] { new { ProductId = 1, Quantity = 2, Amount = 100 } }
    });

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

### 2.3 E2E Test

| Thông tin | Chi tiết |
|-----------|----------|
| **Mục đích** | Test luồng nghiệp vụ hoàn chỉnh |
| **Ai viết** | QA / Automation Engineer |
| **Khi nào chạy** | Release candidate |
| **Framework** | Playwright, Selenium |

**Phạm vi test:**
- Critical business flows (tạo đơn → duyệt → xuất hóa đơn)
- Authentication & Authorization
- Cross-browser compatibility

---

## 3. Test Naming Convention

### 3.1 Quy tắc đặt tên

`[MethodName]_[Scenario]_[ExpectedResult]`

```csharp
[Fact]
public void CreateOrder_WithValidData_ReturnsSuccess() { }

[Fact]
public void CreateOrder_WithInvalidAmount_ThrowsException() { }

[Fact]
public void ApproveOrder_WhenStatusIsPending_SetsApproved() { }
```

### 3.2 Class Naming

`[ClassName]Tests`

```csharp
public class PurchaseOrderServiceTests { }
public class OrderCalculationTests { }
```

---

## 4. Test Patterns

### 4.1 Arrange-Act-Assert

```csharp
[Fact]
public void CalculateDiscount_WithVIPCustomer_ReturnsTenPercent()
{
    // Arrange - Chuẩn bị dữ liệu
    var customer = new Customer { Type = CustomerType.VIP };
    var order = new PurchaseOrder { Customer = customer, TotalAmount = 1000 };

    // Act - Thực hiện hành động
    var discount = order.CalculateDiscount();

    // Assert - Kiểm tra kết quả
    Assert.Equal(100, discount);
}
```

### 4.2 Factory Pattern cho Test Data

```csharp
public static class TestDataFactory
{
    public static PurchaseOrder CreateValidOrder()
    {
        return new PurchaseOrder
        {
            CustomerId = 1,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1, Amount = 100 }
            }
        };
    }

    public static Customer CreateVIPCustomer()
    {
        return new Customer { Id = 1, Type = CustomerType.VIP, Name = "Test VIP" };
    }
}
```

### 4.3 Mocking Dependencies

```csharp
[Fact]
public void CreateOrder_WithValidData_CallsRepository()
{
    // Arrange
    var mockRepo = new Mock<IOrderRepository>();
    var service = new OrderService(mockRepo.Object);

    // Act
    service.Create(TestDataFactory.CreateValidOrder());

    // Assert
    mockRepo.Verify(r => r.Add(It.IsAny<PurchaseOrder>()), Times.Once);
}
```

---

## 5. Coverage Goals

| Layer | Target | Priority |
|-------|--------|----------|
| **Domain** | 90%+ | Cao nhất - logic nghiệp vụ |
| **Application** | 80%+ | Cao - use cases |
| **Infrastructure | 70%+ | Trung bình |
| **Web** | Manual + E2E | Thấp |

**Những gì KHÔNG cần test:**
- Simple DTOs (không có logic)
- Configuration classes
- Third-party integrations (đã được thư viện test)

---

## 6. Test Infrastructure

### 6.1 Project Structure

```
src/
  AMS.Domain/
  AMS.Application/
  AMS.Infrastructure/
  AMS.Web/
tests/
  AMS.Domain.Tests/          # Unit tests cho Domain
  AMS.Application.Tests/     # Unit tests cho Application
  AMS.Infrastructure.Tests/  # Integration tests
  AMS.Web.Tests/             # Controller tests
  AMS.E2E.Tests/             # E2E tests
```

### 6.2 Test Projects Configuration

```xml
<!-- Domain.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\AMS.Domain\AMS.Domain.csproj" />
  </ItemGroup>
</Project>
```

---

## 7. CI/CD Integration

### 7.1 Chạy test trong CI Pipeline

```yaml
# azure-pipelines.yml
steps:
  - script: dotnet test --configuration Release --collect:"XPlat Code Coverage"
    displayName: 'Run Unit Tests'

  - script: dotnet test tests/AMS.Domain.Tests --configuration Release
    displayName: 'Run Domain Tests'

  - script: dotnet test tests/AMS.Application.Tests --configuration Release
    displayName: 'Run Application Tests'
```

### 7.2 Quality Gates

| Metric | Threshold |
|--------|-----------|
| Code Coverage | >= 70% |
| Unit Tests Pass | 100% |
| Integration Tests Pass | 100% |

---

## Checklist Trước Khi Merge PR

- [ ] Tất cả unit tests pass
- [ ] Code coverage đạt ngưỡng quy định
- [ ] Không có new warnings từ test run
- [ ] Integration tests pass (nếu có thay đổi API)
- [ ] Review code đã cover các edge cases

---

> **Cập nhật lần cuối:** tháng 3, 2026  
> **Người phụ trách:** Tech Lead  
> Mọi thắc mắc hoặc đề xuất thay đổi, tạo issue hoặc liên hệ trực tiếp Tech Lead.
