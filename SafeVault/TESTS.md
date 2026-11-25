# Test Documentation

## Test Environment

| Property | Value |
|----------|-------|
| **Framework** | NUnit 3.14.0 |
| **Test Adapter** | NUnit3TestAdapter 4.5.0 |
| **Target Framework** | .NET 10 |
| **Last Test Run** | 2026-11-25 |

## Test Results Summary

**Command:**
```powershell
cd SafeVault\SafeVault
dotnet test SafeVault.Tests\SafeVault.Tests.csproj -c Release
```

**Results (reported 2026-11-25):**
- **Total Tests:** 33
- **Passed:** 27
- **Failed:** 0
- **Skipped:** 6

---

## Unit Tests: InputValidationService

**Count:** 27 tests

**Purpose:** Verify server-side input validation for username and email fields.

**Coverage:**
- Valid inputs (accepted entries within constraints)
- Missing/null inputs
- Length violations (< 3 or > 100 characters)
- Invalid characters (rejected: special chars, spaces, control characters, `<` and `>`)
- Email format validation using `EmailAddressAttribute`
- Edge cases (boundary lengths, whitespace trimming)

**Note:** Unit tests validate input rules only. They do not verify SQL parameter binding (requires integration tests with real database) or final HTML rendering.

---

## Integration Tests: DemoUserRepository

**Count:** 6 tests

**Purpose:** Verify parameterized SQL query execution and SQL injection resistance using a real SQL Server database.

**Tests (skipped if database not configured):**
1. `FindByUsernameAsync_ExistingUsername_ReturnsMatchingUser`
2. `FindByUsernameAsync_NonexistentUsername_ReturnsNull`
3. `FindByUsernameAsync_SQLInjectionAttempt_TreatsAsLiteral` – Attempts `' OR 1=1 --`; verifies injection remains in parameter, not SQL text
4. `FindByUsernameAsync_DropTableAttempt_TreatsAsLiteral` – Attempts `'; DROP TABLE DemoUsers; --`; verifies table integrity
5. `AddAsync_ValidDemoUser_SavesSuccessfully`
6. `AddAsync_EmailWithSQLInjectionBypassingValidation_SafeParameterization` – Verifies parameterization protects even if validation is bypassed

**Current Status:** All 6 tests **skipped** (environment variable `SAFEVAULT_TEST_DB_CONNECTION_STRING` not configured).

---

## Running Tests

### Unit Tests Only
```powershell
cd SafeVault\SafeVault
dotnet test SafeVault.Tests\SafeVault.Tests.csproj --filter "InputValidationServiceTests" -c Release
```

### Integration Tests (Requires Setup)

**1. Create test database:**
```sql
CREATE DATABASE SafeVault_Tests;
```

**2. Configure environment variable (PowerShell):**
```powershell
$env:SAFEVAULT_TEST_DB_CONNECTION_STRING = "Server=(localdb)\mssqllocaldb;Database=SafeVault_Tests;Integrated Security=true;"
```

**3. Run all tests:**
```powershell
cd SafeVault\SafeVault
dotnet test SafeVault.Tests\SafeVault.Tests.csproj -c Release
```

**Important:** Use a **separate, disposable test database**. Never point to production or development databases.

---

## Test Database Notes

- Tests use `ApplicationDbContext.Database.EnsureCreatedAsync()` to initialize schema
- Each test run with database configured creates/reuses test database
- Teardown removes only test-prefixed records; database persists between runs
- **Data isolation:** Integration tests use separate connection string; application and tests do not share data

---

## Known Gaps

1. **XSS Output Encoding:** No automated verification that usernames are HTML-encoded in rendered pages (`@Model.Username` auto-encodes in Razor, but not tested end-to-end)
2. **Email Format Strictness:** `EmailAddressAttribute` validation may differ from RFC 5322; edge cases (international addresses, rare formats) not explicitly tested
3. **End-to-End Web UI:** No Selenium/browser tests for full page rendering or CSP (Content Security Policy) behavior
4. **SQL Injection Integration:** Requires separate database setup to prove parameterization; unit tests cannot verify parameter binding

---

**Version:** 1.0  
**Last Updated:** 2026-11-25
