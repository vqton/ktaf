# AccountingERP - Hệ thống Kế toán ERP

Hệ thống quản lý kế toán tuân thủ **TT99/2025/TT-BTC** - Thông tư quy định chế độ kế toán doanh nghiệp nhỏ và vừa tại Việt Nam.

## 🎯 Tính năng chính

- ✅ **Bút toán kế toán** với đầy đủ thông tin chứng từ gốc (bắt buộc TT99)
- ✅ **Hệ thống tài khoản** 56 tài khoản theo chuẩn TT99
- ✅ **Báo cáo tài chính**: B01, B02, B03-DN
- ✅ **Multi-database**: SQLite (dev) → SQL Server (production)
- ✅ **On-premises deployment** (không cloud)

## 🏗️ Kiến trúc

```
AccountingERP/
├── src/
│   ├── AccountingERP.Domain/        # Entities, Value Objects, Enums
│   ├── AccountingERP.Application/   # CQRS (Commands, Queries), DTOs
│   ├── AccountingERP.Infrastructure/# EF Core, Repositories, Migrations
│   └── AccountingERP.Web/           # Blazor Server UI
├── tests/                           # Unit & Integration Tests
└── docker/                          # Docker Compose cho SQL Server
```

**Kiến trúc**: Clean Architecture + Domain-Driven Design (DDD) + CQRS

## 🚀 Quick Start

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio Code](https://code.visualstudio.com/) hoặc Visual Studio 2022
- SQL Server (tùy chọn, cho production)

### 2. Clone & Build

```bash
git clone <repository-url>
cd AccountingERP
dotnet restore
dotnet build
```

### 3. Database Setup

**Option A: SQLite (Development - mặc định)**
```bash
cd src/AccountingERP.Web
dotnet ef database update --project ../AccountingERP.Infrastructure
```

**Option B: SQL Server**
```bash
# Chỉnh sửa appsettings.Development.json:
# "Database:Provider": "sqlserver"
# "ConnectionStrings:DefaultConnection": "Server=..."

dotnet ef database update --project ../AccountingERP.Infrastructure
```

### 4. Run Application

```bash
cd src/AccountingERP.Web
dotnet run
```

Truy cập: http://localhost:5000

## ⚙️ Cấu hình Multi-Database

| Môi trường | Database Provider | Connection String |
|-----------|------------------|-------------------|
| Development | SQLite | `Data Source=accounting_dev.db` |
| Staging | SQL Server Express | `Server=localhost\\SQLEXPRESS;Database=...` |
| Production | SQL Server | `Server=prod-server;Database=...` |

Chỉnh sửa file `appsettings.<Environment>.json`:
```json
{
  "Database": {
    "Provider": "sqlite"  // hoặc "sqlserver"
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Project Structure

### Domain Layer (`AccountingERP.Domain`)
- **Entities**: `JournalEntry`, `JournalEntryLine`, `Account`
- **Value Objects**: `Money`, `Currency`
- **Enums**: `JournalEntryStatus`, `AccountType`, `FinancialReportType`
- **Exceptions**: Domain exceptions

### Application Layer (`AccountingERP.Application`)
- **Commands**: `CreateJournalEntryCommand`, `PostJournalEntryCommand`
- **Queries**: `GetJournalEntryByIdQuery`, `GetJournalEntriesQuery`
- **Validators**: FluentValidation rules
- **DTOs**: Data Transfer Objects

### Infrastructure Layer (`AccountingERP.Infrastructure`)
- **DbContext**: `AccountingDbContext`
- **Repositories**: `JournalEntryRepository`, `AccountRepository`
- **Migrations**: EF Core migrations

### Web Layer (`AccountingERP.Web`)
- **Blazor Server**: Interactive UI
- **Fluent UI**: Microsoft Fluent UI components
- **Bootstrap Icons**: Icon library

## 📋 TT99 Compliance

Hệ thống tuân thủ các yêu cầu của TT99/2025/TT-BTC:

- ✅ Bắt buộc nhập **số chứng từ gốc** và **ngày chứng từ gốc**
- ✅ Hệ thống tài khoản 56 TK đầy đủ
- ✅ Báo cáo tài chính chuẩn: B01, B02, B03-DN
- ✅ Phân quyền và audit trail

## 🔧 Development

### VS Code Extensions (recommended)
- C# Dev Kit
- Blazor WASM Companion
- .NET Test Explorer
- GitLens

### Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run web app
dotnet run --project src/AccountingERP.Web

# Add migration
dotnet ef migrations add <MigrationName> --project src/AccountingERP.Infrastructure --startup-project src/AccountingERP.Web

# Update database
dotnet ef database update --project src/AccountingERP.Infrastructure --startup-project src/AccountingERP.Web
```

## 📄 License

MIT License - Copyright (c) 2026

## 🤝 Contributing

1. Fork repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## 📞 Support

Nếu có vấn đề, vui lòng tạo GitHub Issue hoặc liên hệ team phát triển.

---

**Built with ❤️ using .NET 8, Blazor Server, and EF Core**
