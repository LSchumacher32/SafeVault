# SafeVault

A secure software development training project built with ASP.NET Core Razor Pages (.NET 10).

## Overview

SafeVault is an educational application designed to teach secure software development practices. The project implements core security concepts including server-side input validation, parameterized SQL queries, and secure error handling.

Currently, the project includes **Activity 1: Safe Input Exercise**, which demonstrates:
- Server-side input validation for usernames and emails
- Secure data persistence with EF Core and SQL Server
- Protection against common attacks (SQL injection, XSS, control characters)
- Proper error handling and logging practices

## Project Structure

```
SafeVault/
├── SafeVault/                          # Main web application (Razor Pages)
│   ├── Pages/
│   │   ├── SafeInput.cshtml            # Training exercise UI
│   │   └── SafeInput.cshtml.cs         # Page model with validation logic
│   ├── Services/
│   │   ├── InputValidationService.cs   # Core validation logic
│   │   └── DemoUserRepository.cs       # Data access layer with parameterized SQL
│   ├── Models/
│   │   └── DemoUser.cs                 # Demo user entity (separate from Identity)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs     # EF Core DbContext
│   │   └── Migrations/                 # Database migrations
│   └── Program.cs                      # Application startup
│
├── SafeVault.Tests/                    # NUnit test project
│   ├── InputValidationServiceTests.cs  # 27 unit tests
│   ├── DemoUserRepositoryIntegrationTests.cs  # 6 integration tests
│   ├── TestDbContextFactory.cs         # Test database context factory
│   └── SqlCommandCapturingInterceptor.cs     # SQL parameter verification
│
└── TESTS.md                            # Detailed test documentation
```

## Technology Stack

- **Framework:** ASP.NET Core (.NET 10)
- **UI:** Razor Pages
- **Database:** SQL Server
- **ORM:** Entity Framework Core 10.0.14
- **Authentication:** ASP.NET Core Identity (pre-configured, unchanged)
- **Testing:** NUnit 3.14.0 with NUnit3TestAdapter 4.5.0

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- Visual Studio 2022+
- SQL Server (LocalDB)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/LSchumacher32/SafeVault.git
cd SafeVault
```

2. Restore packages:
```bash
dotnet restore
```

3. Apply database migrations:
```bash
cd SafeVault
dotnet ef database update
```

4. Run the application:
```bash
dotnet run --project SafeVault
```

Access at: `https://localhost:7125`

### Quick Start

**Start the application:**
```bash
cd SafeVault
dotnet run
# Then open: https://localhost:7125/SafeInput
```

**Run unit tests (27 validation tests):**
```bash
cd SafeVault
dotnet test SafeVault.Tests\SafeVault.Tests.csproj --filter "InputValidationServiceTests" -c Release
# Expected: 27 passed
```

**Run integration tests (requires test database configuration):**
See [TESTS.md](TESTS.md) for setup instructions.

## Activity 1: Safe Input Exercise Implementation

### Core Components

**InputValidationService** (`Services/InputValidationService.cs`)
- **Username:** 3-100 characters, alphanumeric + underscore and hyphen only
- **Email:** Max 100 characters, valid format, no control characters or angle brackets
- **Method:** `TryValidate()` returns `bool`, outputs normalized values and error messages

**DemoUserRepository** (`Services/DemoUserRepository.cs`)
- **AddAsync(user):** Saves validated DemoUser
- **FindByUsernameAsync(username):** Uses **parameterized SQL query** (`FromSqlInterpolated`)

**SafeInput Page** (`Pages/SafeInput.cshtml`)
- Save form: Add users with validation
- Search form: Query by username (security test)
- Post/Redirect/Get pattern for user feedback

### Security Guarantees

| Threat | Mitigation | Implementation |
|--------|------------|-----------------|
| **SQL Injection** | Parameterized queries | EF Core `FromSqlInterpolated` converts `{username}` to `@p0` parameter |
| **XSS (Input)** | Validation blocks `<>` | `InputValidationService` rejects `<` and `>` characters |
| **XSS (Output)** | HTML encoding | Razor page `@Model.Username` auto-encodes (not `@Html.Raw`) |
| **Control Characters** | Validation blocks them | `char.IsControl()` check in `InputValidationService` |
| **CSRF** | Anti-forgery tokens | `[ValidateAntiForgeryToken]` on POST handlers |

### Test Coverage

- **Unit Tests:** 27 tests covering validation rules, SQL injection attempts, XSS attempts, control characters, and edge cases
- **Integration Tests:** 6 tests with real SQL Server database to verify parameterization (requires optional setup)

See [TESTS.md](TESTS.md) for complete test documentation and running instructions.

## About Copilot Usage

This project was developed with GitHub Copilot as a development aid for:
- Generating boilerplate code (Page handlers, DbContext configuration)
- Creating comprehensive test cases and security-focused test data
- Drafting documentation and inline comments
- Suggesting validation patterns and security best practices

The implementation, security decisions, and test strategy were reviewed and verified manually. All code compiles successfully and test cases document the expected behavior.

## Running Tests

Comprehensive test documentation is available in [TESTS.md](TESTS.md), including:
- How to run unit tests locally (27 tests)
- How to configure and run integration tests (6 tests with real SQL Server)
- Test database setup (SQL Server LocalDB)
- Expected results and known limitations

## Resources

- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [OWASP Top 10](https://owasp.org/Top10/)
- [EF Core Security Best Practices](https://docs.microsoft.com/en-us/ef/core/miscellaneous/security)
- [NUnit 3 Documentation](https://docs.nunit.org/)

---

**Version:** 1.0  
**Updated:** November 25, 2026  
**Activity:** Secure Input Exercise (Activity 1)
