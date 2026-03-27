# AGENTS.md - Guidelines for Agentic Coding in glApp Repository

This file provides instructions for AI coding agents working in this repository. It covers build/test commands, code style guidelines, and development practices.

## Table of Contents
1. [Build, Lint, and Test Commands](#1-build-lint-and-test-commands)
2. [Code Style Guidelines](#2-code-style-guidelines)
3. [Development Practices](#3-development-practices)
4. [Project Structure](#4-project-structure)
5. [References](#5-references)

---

## 1. Build, Lint, and Test Commands

### Build Commands
- Build entire solution: `dotnet build E:\glApp\AMS.sln`
- Build specific project: `dotnet build src/<ProjectName>/<ProjectName>.csproj`
- Clean solution: `dotnet clean E:\glApp\AMS.sln`
- Restore packages: `dotnet restore E:\glApp\AMS.sln`

### Test Commands
> Note: This repository currently does not have a test project configured.
> When tests are added, standard .NET test commands will apply:
> - Run all tests: `dotnet test E:\glApp\AMS.sln`
> - Run specific test project: `dotnet test tests/<TestProject>/<TestProject>.csproj`
> - Run single test method: `dotnet test --filter "FullyQualifiedName~Namespace.Class.Method"`
> - Run tests with specific category: `dotnet test --filter "Category=UnitTest"`

### Lint/Formatting Commands
- Format code with dotnet format: `dotnet format`
- Analyze code style violations: `dotnet format --verify-no-changes`
- Run Roslyn analyzers: Built into build process

### Development Server
- Run web application: `dotnet run --project src/AMS.Web/AMS.Web.csproj`
- Run with hot reload: `dotnet watch run --project src/AMS.Web/AMS.Web.csproj`

---

## 2. Code Style Guidelines

This repository follows the coding standards defined in [CODING_STANDARDS.md](CODING_STANDARDS.md). Key guidelines include:

### Naming Conventions
- **Classes, Interfaces**: PascalCase (e.g., `PurchaseOrderService`, `IInvoiceRepository`)
- **Methods, Properties**: PascalCase (e.g., `CalculateTotalAmount()`, `OrderDate`)
- **Variables, Parameters**: camelCase (e.g., `purchaseOrder`, `orderId`)
- **Private Fields**: `_camelCase` (e.g., `_unitOfWork`, `_logger`)
- **Constants**: PascalCase (e.g., `MaxApprovalLevel`, `DefaultCurrency`)
- **Interfaces**: Prefix with `I` (e.g., `IOrderService`, `IRepository<T>`)

### Code Organization
- Follow layered architecture: `Web → Application → Domain`, `Infrastructure → Application`
- Domain layer must not reference any other layers
- Web layer must not call Infrastructure directly - must go through Application
- Organize code by business module, not by technical type (e.g., `/Purchasing/` not `/Services/`)

### Formatting
- Use 4 spaces for indentation (not tabs)
- Place opening braces on same line as declaration
- One statement per line
- Maximum line length: 120 characters
- Use `var` only when type is obvious from right-hand side
- Prefer expression-bodied members for simple methods/properties

### Imports/Usings
- Sort usings alphabetically
- Group System usings first, then third-party, then project-specific
- Remove unused usings
- Use namespace aliases only when necessary to resolve conflicts

### Types
- Prefer immutable types when possible
- Use records for DTOs and data carriers
- Make fields readonly unless they need to be modified after construction
- Use appropriate accessibility modifiers (private by default)

### Error Handling
- Use `ServiceResult<T>` pattern for application layer operations
- Handle business rule violations in Domain layer with `DomainException`
- Log all exceptions with contextual information
- Never expose exception details to users in production
- Use global exception handler for unhandled exceptions

### Logging
- Use structured logging with contextual properties
- Log at appropriate levels (Debug, Info, Warn, Error, Fatal)
- Never log sensitive information (passwords, tokens, personal data)
- Include relevant IDs and contextual information in log messages

### Security
- Always use parameterized queries or EF/LINQ - never concatenate SQL
- Validate all input at application layer boundary
- Use role-based authorization with `[Authorize]` attributes
- Implement proper authentication and session management

### Performance
- Implement pagination for all list queries
- Use `AsNoTracking()` for read-only queries
- Select only required fields in queries
- Cache infrequently changing reference data
- Avoid N+1 query patterns

---

## 3. Development Practices

### Git Workflow
- Main branch: `main` (production-ready)
- Development branch: `develop` (integration branch)
- Feature branches: `feature/*`
- Hotfix branches: `hotfix/*`
- Release branches: `release/*`
- Pull requests required for all changes
- Minimum 1 reviewer for PRs
- Squash merge for feature branches

### Class Design
- Follow SOLID principles
- Single Responsibility Principle: One class should have one reason to change
- Methods should not exceed 50 lines
- Avoid magic numbers - use named constants
- Comments should explain why, not what (code should be self-explanatory)

### Dependency Management
- Use dependency injection for all external dependencies
- Prefer constructor injection over property injection
- Register services with appropriate lifetimes (Scoped, Transient, Singleton)
- Avoid static dependencies and service locator pattern

### Testing (when implemented)
- Write unit tests for business logic
- Use Arrange-Act-Assert pattern
- Test public interface, not private implementation details
- Mock external dependencies
- Aim for high code coverage on complex logic
- Write integration tests for critical workflows

---

## 4. Project Structure

```
AMS.sln
├── src/
│   ├── AMS.Web              # Presentation layer: Controllers, Views, ViewModels
│   ├── AMS.Application      # Application layer: Use cases, Services, DTOs, Interfaces
│   ├── AMS.Domain           # Domain layer: Entities, Business rules, Domain Events
│   ├── AMS.Infrastructure   # Infrastructure layer: EF DbContext, Repositories, Email, File...
│   └── AMS.Common           # Shared: Extensions, Constants, Helpers
└── tools/
    └── dbcreate/            # Database creation scripts and tools
```

### Layer Responsibilities
- **Web**: HTTP concerns only (routing, model binding, action results)
- **Application**: Orchestrates use cases, coordinates between domain and infrastructure
- **Domain**: Pure business logic and rules, no infrastructure concerns
- **Infrastructure**: Technical implementations (database, file system, external services)
- **Common**: Cross-cutting concerns used across layers

---

## 5. References

- [CODING_STANDARDS.md](CODING_STANDARDS.md) - Primary coding standards document
- [TEST_STRATEGY.md](TEST_STRATEGY.md) - Test strategy, patterns, and conventions
- [docs/UI-UX-GUIDE.md](docs/UI-UX-GUIDE.md) - UI/UX design guidelines, design system, component standards
- [docs/USER_PERSONAS.md](docs/USER_PERSONAS.md) - User personas for AMS ERP
- [docs/USER_JOURNEY.md](docs/USER_JOURNEY.md) - User journey maps
- [docs/USE_CASES.md](docs/USE_CASES.md) - Use case definitions
- [AMS.sln](AMS.sln) - Solution file
- Individual project files in src/ directory for specific project configurations

### When in Doubt
1. Follow existing code patterns in the repository
2. Refer to CODING_STANDARDS.md for detailed guidelines
3. Maintain consistency with surrounding code
4. Prioritize readability and maintainability