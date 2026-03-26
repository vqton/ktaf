# CODING STANDARDS — .NET Framework ERP Web App (On-Premises)

> Tài liệu này áp dụng cho toàn bộ dự án ERP nội bộ chạy trên .NET Framework.  
> Mọi thành viên trong team cần đọc, hiểu và tuân thủ trước khi bắt đầu code.

---

## Mục lục

1. [Architecture & Structure](#1-architecture--structure)
2. [Naming & Coding Convention](#2-naming--coding-convention)
   2.4. [Tài liệu (Documentation)](#24-tài-liệu-documentation)
3. [Error Handling & Logging](#3-error-handling--logging)
4. [Security & Performance](#4-security--performance)
5. [Development Practices](#5-development-practices)
   5.1. [Quy trình phát triển](#51-quy-trình-phát-triển)
   5.2. [Checklist Setup Dự Án Mới](#52-checklist-setup-dự-án-mới)

---

## 1. Architecture & Structure

### 1.1 Phân lớp chuẩn (Layered Architecture)

```
MyErp.sln
├── MyErp.Web              # Presentation: Controllers, Views, ViewModels
├── MyErp.Application      # Use cases, Services, DTOs, Interfaces
├── MyErp.Domain           # Entities, Business rules, Domain Events
├── MyErp.Infrastructure   # EF DbContext, Repositories, Email, File...
└── MyErp.Common           # Extensions, Constants, Helpers dùng chung
```

**Nguyên tắc dependency:**
- Dependency chỉ đi **một chiều**: `Web → Application → Domain`, `Infrastructure → Application`
- `Domain` **không được** reference bất kỳ layer nào khác
- `Web` **không được** gọi thẳng `Infrastructure` — phải qua `Application`

### 1.2 Cấu trúc folder theo Business Module

```
MyErp.Application/
├── Purchasing/
│   ├── IPurchaseOrderService.cs
│   ├── PurchaseOrderService.cs
│   └── Dtos/
│       ├── CreatePurchaseOrderDto.cs
│       └── PurchaseOrderDto.cs
├── Inventory/
├── Accounting/
└── HR/
```

> ✅ Tổ chức theo **business module** thay vì theo kỹ thuật (`/Services`, `/Repositories`).  
> ERP có nhiều module — làm vậy dễ tìm, dễ onboard người mới.

### 1.3 Patterns bắt buộc

**Repository Pattern** — tách data access khỏi business logic:

```csharp
public interface IPurchaseOrderRepository
{
    PurchaseOrder GetById(int id);
    IEnumerable<PurchaseOrder> GetPendingOrders();
    void Add(PurchaseOrder order);
    void Update(PurchaseOrder order);
}
```

**Unit of Work** — đảm bảo transaction nhất quán:

```csharp
public interface IUnitOfWork : IDisposable
{
    IPurchaseOrderRepository PurchaseOrders { get; }
    IInventoryRepository Inventory { get; }
    int Commit();
}
```

**Controller** chỉ làm: nhận request → gọi service → trả response:

```csharp
public class PurchaseOrderController : BaseController
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrderController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult Create(CreatePurchaseOrderDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = _service.Create(dto);
        return RedirectToAction("Detail", new { id = result.Id });
    }
}
```

---

## 2. Naming & Coding Convention

### 2.1 Quy tắc đặt tên

| Loại | Quy tắc | Ví dụ |
|------|---------|-------|
| Class, Interface | PascalCase | `PurchaseOrderService`, `IInvoiceRepository` |
| Method, Property | PascalCase | `CalculateTotalAmount()`, `OrderDate` |
| Variable, Parameter | camelCase | `purchaseOrder`, `orderId` |
| Private field | `_camelCase` | `_unitOfWork`, `_logger` |
| Constant | PascalCase | `MaxApprovalLevel`, `DefaultCurrency` |
| Interface | Tiền tố `I` | `IOrderService`, `IRepository<T>` |

```csharp
// ✅ Đúng
private readonly IUnitOfWork _unitOfWork;
public const int MaxApprovalLevel = 5;
public decimal CalculateTotalAmount() { }

// ❌ Sai — tên viết tắt, tên kiểu dữ liệu
var ord = ...;       // ❌ → var purchaseOrder = ...;
var strName = ...;   // ❌ → var customerName = ...;
```

### 2.2 Naming theo Business Domain ERP

Luôn dùng thuật ngữ nghiệp vụ rõ ràng, phản ánh đúng domain:

```csharp
// Entities
public class GoodsReceipt { }      // Phiếu nhập kho
public class PaymentVoucher { }    // Phiếu chi
public class JournalEntry { }      // Bút toán kế toán

// Methods
public void PostJournalEntry() { }     // Hạch toán
public void ApproveRequisition() { }   // Duyệt đề nghị
public void ReconcileInventory() { }   // Đối chiếu tồn kho
```

### 2.3 Các quy tắc bổ sung

- Một class chỉ làm **một việc** (Single Responsibility Principle)
- Phương thức không dài quá **50 dòng** — nếu dài hơn, tách ra
- Không dùng **magic number** — đặt hằng số có tên rõ ràng
- Comment giải thích **tại sao**, không giải thích **cái gì** (code tự nói được)

### 2.4 Tài liệu (Documentation)

**Bắt buộc viết docstring bằng tiếng Việt** cho tất cả các thành phần công khai (public class, public method, public property). Mục tiêu là giúp các thành viên trong team dễ dàng đọc hiểu và tra cứu.

**Cấu trúc docstring:**

```csharp
/// <summary>
/// Mô tả ngắn gọn về chức năng của phương thức/class.
/// </summary>
/// <param name="parameterName">Mô tả tham số (nếu có).</param>
/// <returns>Mô tả giá trị trả về (nếu có).</returns>
public class PurchaseOrderService
{
    /// <summary>
    /// Tạo đơn mua hàng mới từ thông tin được cung cấp.
    /// </summary>
    /// <param name="dto">Thông tin đơn mua hàng.</param>
    /// <returns>Kết quả với đơn hàng vừa tạo hoặc lỗi nếu không thành công.</returns>
    public ServiceResult<PurchaseOrder> Create(CreatePurchaseOrderDto dto) { }
}
```

**Quy tắc viết docstring:**

| Thành phần | Yêu cầu |
|------------|---------|
| Class/Interface | Mô tả ngắn gọn (1-2 câu) về mục đích sử dụng |
| Public Method | Mô tả chức năng, tham số, giá trị trả về, ngoại lệ (nếu có) |
| Property | Mô tả ý nghĩa của property |
| DTO | Mô tả mục đích sử dụng DTO này |

**Ví dụ:**

```csharp
/// <summary>
/// Dịch vụ quản lý đơn mua hàng, xử lý các nghiệp vụ liên quan đến tạo, duyệt, hủy đơn.
/// </summary>
public interface IPurchaseOrderService { }

/// <summary>
/// Ngưỡng tiền duyệt tự động. Nếu tổng tiền vượt quá ngưỡng này cần duyệt thủ công.
/// </summary>
public decimal AutoApprovalThreshold { get; set; }
```

> ✅ Docstring tiếng Việt giúp team dễ maintain, onboarding member mới nhanh hơn, và giảm thiểu hiểu nhầm về business logic.

```csharp
// ❌ Magic number
if (order.Amount > 50000000) { ... }

// ✅ Constant có tên rõ ràng
private const decimal DirectorApprovalThreshold = 50_000_000m;
if (order.Amount > DirectorApprovalThreshold) { ... }
```

---

## 3. Error Handling & Logging

### 3.1 Phân tầng xử lý lỗi

**Domain layer** — vi phạm business rule:

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

**Application layer** — kết quả rõ ràng, không throw bừa:

```csharp
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T Data { get; private set; }
    public string ErrorMessage { get; private set; }

    public static ServiceResult<T> Success(T data) =>
        new ServiceResult<T> { IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(string error) =>
        new ServiceResult<T> { IsSuccess = false, ErrorMessage = error };
}
```

**Service** — phân biệt lỗi business và lỗi hệ thống:

```csharp
public ServiceResult<PurchaseOrder> ApprovePurchaseOrder(int id, string approver)
{
    try
    {
        var order = _unitOfWork.PurchaseOrders.GetById(id);

        if (order == null)
            return ServiceResult<PurchaseOrder>.Failure("Không tìm thấy đơn hàng.");

        if (order.Status != OrderStatus.Pending)
            return ServiceResult<PurchaseOrder>.Failure("Chỉ được duyệt đơn ở trạng thái chờ duyệt.");

        order.Approve(approver);
        _unitOfWork.Commit();

        return ServiceResult<PurchaseOrder>.Success(order);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Lỗi khi duyệt đơn hàng {OrderId}", id);
        return ServiceResult<PurchaseOrder>.Failure("Lỗi hệ thống, vui lòng thử lại.");
    }
}
```

### 3.2 Global Exception Handler

```csharp
public class GlobalExceptionFilter : HandleErrorAttribute
{
    private readonly ILogger _logger;

    public override void OnException(ExceptionContext context)
    {
        _logger.Fatal(context.Exception,
            "Unhandled exception at {Url}",
            context.HttpContext.Request.Url);

        context.Result = new ViewResult { ViewName = "Error" };
        context.ExceptionHandled = true;
    }
}
```

### 3.3 Logging — NLog hoặc Serilog

**Cấu hình file rotation (NLog):**
```xml
<target name="file" type="File"
    fileName="logs/erp-${shortdate}.log"
    archiveEvery="Day"
    maxArchiveFiles="30" />
```

**Structured logging — log phải có context:**

```csharp
// ✅ Đúng — có context rõ ràng, dễ trace
_logger.Info("Tạo đơn hàng thành công. OrderId={OrderId}, CreatedBy={User}, Amount={Amount}",
    order.Id, currentUser, order.TotalAmount);

// ❌ Sai — không có context
_logger.Info("Tạo đơn hàng thành công");
```

**Mức độ log:**

| Level | Khi nào dùng |
|-------|-------------|
| `Debug` | Chi tiết kỹ thuật — chỉ bật khi debug |
| `Info` | Sự kiện business bình thường |
| `Warn` | Bất thường nhưng chưa lỗi (retry, fallback) |
| `Error` | Lỗi nhưng hệ thống vẫn chạy |
| `Fatal` | Lỗi khiến hệ thống không thể tiếp tục |

**Không bao giờ log thông tin nhạy cảm:**

```csharp
// ❌ Tuyệt đối không log password, token, thông tin cá nhân
_logger.Info("User {User} login with password {Password}", username, password);

// ✅ Chỉ log thông tin cần thiết
_logger.Info("User {User} logged in successfully", username);
```

---

## 4. Security & Performance

### 4.1 Security

**Bắt buộc dùng parameterized query — không bao giờ concatenate SQL:**

```csharp
// ❌ SQL Injection vulnerability
var sql = "SELECT * FROM Orders WHERE CustomerId = " + customerId;

// ✅ Parameterized query hoặc EF
var orders = db.Orders.Where(o => o.CustomerId == customerId).ToList();
```

**Phân quyền theo module:**

```csharp
[Authorize(Roles = "PurchaseManager,Admin")]
public ActionResult ApprovePurchaseOrder(int id) { ... }
```

**Validate input tại Application layer — không chỉ ở UI:**

```csharp
public ServiceResult<T> Create(CreatePurchaseOrderDto dto)
{
    if (dto.TotalAmount <= 0)
        return ServiceResult<T>.Failure("Tổng tiền phải lớn hơn 0.");
    if (dto.RequiredDate < DateTime.Today)
        return ServiceResult<T>.Failure("Ngày yêu cầu không hợp lệ.");
    // ...
}
```

### 4.2 Performance

**Phân trang bắt buộc cho mọi danh sách — không được load toàn bộ data:**

```csharp
public PagedResult<PurchaseOrderDto> GetOrders(OrderFilterDto filter, int page, int pageSize)
{
    var query = _db.PurchaseOrders
        .Where(o => o.Status == filter.Status)
        .OrderByDescending(o => o.CreatedDate);

    var total = query.Count();
    var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return new PagedResult<PurchaseOrderDto>(items, total, page, pageSize);
}
```

**Dùng `AsNoTracking()` cho query chỉ đọc:**

```csharp
var reportData = _db.Orders
    .AsNoTracking()
    .Where(o => o.CreatedDate >= fromDate)
    .ToList();
```

**Select chỉ những field cần thiết:**

```csharp
var orderSummaries = _db.Orders
    .Select(o => new OrderSummaryDto
    {
        Id = o.Id,
        Code = o.Code,
        TotalAmount = o.TotalAmount,
        Status = o.Status
    })
    .ToList();
```

**Cache dữ liệu danh mục ít thay đổi:**

```csharp
public IEnumerable<Department> GetDepartments()
{
    const string cacheKey = "departments_all";
    var cached = _cache.Get(cacheKey) as IEnumerable<Department>;
    if (cached != null) return cached;

    var data = _db.Departments.AsNoTracking().ToList();
    _cache.Set(cacheKey, data, TimeSpan.FromMinutes(30));
    return data;
}
```

---

## 5. Development Practices

### 5.1 Quy trình phát triển

**Cập nhật trạng thái công việc:**

- Sau khi hoàn thành task, cập nhật trạng thái trong hệ thống quản lý công việc (Jira, Trello, Azure DevOps, v.v.)
- Đánh dấu task đã hoàn thành, thêm thông tin cần thiết (mã commit, thời gian hoàn thành)

**Commit và Push:**

- Commit thường xuyên với message rõ ràng, mô tả những gì đã làm
- Push code lên remote repository ngay sau khi commit
- Tuân thủ branch strategy đã quy định

```bash
# Ví dụ workflow
git add .
git commit -m "Add purchase order approval feature"
git push origin feature/purchase-order-approval
```

### 5.2 Checklist Setup Dự Án Mới

| # | Hạng mục | Việc cần làm | Done |
|---|----------|-------------|------|
| 1 | **Structure** | Tạo 5 project theo layered architecture | ☐ |
| 2 | **Convention** | Tất cả member đọc và ký nhận tài liệu này | ☐ |
| 3 | **Logging** | Cài NLog/Serilog, cấu hình ghi file có rotation 30 ngày | ☐ |
| 4 | **Error handling** | Tạo `ServiceResult<T>`, `GlobalExceptionFilter` | ☐ |
| 5 | **Security** | Chỉ dùng parameterized query, setup role-based auth | ☐ |
| 6 | **Performance** | Quy định bắt buộc phân trang, dùng `AsNoTracking` cho report | ☐ |
| 7 | **Code review** | Dùng pull request, ít nhất 1 người review trước khi merge | ☐ |
| 8 | **Branch strategy** | Quy định nhánh: `main`, `develop`, `feature/*`, `hotfix/*` | ☐ |

---

> **Cập nhật lần cuối:** tháng 3, 2026  
> **Người phụ trách:** Tech Lead  
> Mọi thắc mắc hoặc đề xuất thay đổi, tạo issue hoặc liên hệ trực tiếp Tech Lead.
