using NUnit.Framework;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Services;

namespace SafeVault.Tests;

/// <summary>
/// Integration tests for DemoUserRepository using real SQL Server database.
/// These tests verify that parameterized queries are actually parameterized
/// and that SQL injection attacks are properly prevented at the database layer.
///
/// Requirements:
/// - Set environment variable SAFEVAULT_TEST_DB_CONNECTION_STRING to a SQL Server connection string
/// - Example: set SAFEVAULT_TEST_DB_CONNECTION_STRING=Server=(localdb)\mssqllocaldb;Database=SafeVault_Tests;Integrated Security=true;
/// - The test database will be created on first run via EF Core migrations
/// </summary>
[TestFixture]
public class DemoUserRepositoryIntegrationTests
{
    private ApplicationDbContext? _context;
    private DemoUserRepository? _repository;
    private SqlCommandCapturingInterceptor? _interceptor;
    private DemoUser? _testUser;

    [SetUp]
    public async Task SetUp()
    {
        // Skip tests if test database is not configured
        if (!TestDbContextFactory.IsTestDatabaseConfigured())
        {
            Assert.Ignore(TestDbContextFactory.GetSkipReasonIfNotConfigured());
        }

        // Create interceptor to capture SQL commands
        _interceptor = new SqlCommandCapturingInterceptor();

        // Create context with interceptor
        _context = TestDbContextFactory.CreateTestContext(_interceptor);
        Assert.That(_context, Is.Not.Null, "Failed to create test database context");

        // Ensure test database is created with current schema
        try
        {
            await _context!.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to ensure test database creation: {ex.Message}");
        }

        _repository = new DemoUserRepository(_context!);

        // Create a test user for search tests
        _testUser = new DemoUser
        {
            Username = "test_user_integration",
            Email = "test@example.com"
        };

        try
        {
            await _repository.AddAsync(_testUser);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to create test user: {ex.Message}");
        }

        // Clear captured commands after setup
        _interceptor?.Clear();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context == null)
        {
            return;
        }

        try
        {
            // Clean up: Delete test data
            if (_testUser != null)
            {
                var demoUsers = _context.DemoUsers.Where(u => u.Username.Contains("test_")).ToList();
                foreach (var user in demoUsers)
                {
                    _context.DemoUsers.Remove(user);
                }
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Warning: Failed to clean up test data: {ex.Message}");
        }
        finally
        {
            _context.Dispose();
        }
    }

    // ============================================================================
    // TEST CASES
    // ============================================================================

    /// <summary>
    /// Test: Finding an existing user by username returns the correct record.
    /// Purpose: Verify that parameterized query works correctly for valid data.
    /// </summary>
    [Test]
    public async Task FindByUsernameAsync_ExistingUsername_ReturnsMatchingUser()
    {
        // Arrange
        var searchUsername = _testUser!.Username;

        // Act
        var result = await _repository!.FindByUsernameAsync(searchUsername);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null for existing user");
        Assert.That(result!.Username, Is.EqualTo(searchUsername));
        Assert.That(result.Email, Is.EqualTo(_testUser.Email));
        Assert.That(result.Id, Is.EqualTo(_testUser.Id));
    }

    /// <summary>
    /// Test: Searching for non-existent username returns null.
    /// Purpose: Verify that parameterized query correctly finds nothing when appropriate.
    /// </summary>
    [Test]
    public async Task FindByUsernameAsync_NonexistentUsername_ReturnsNull()
    {
        // Arrange
        var searchUsername = "this_user_does_not_exist_12345";

        // Act
        var result = await _repository!.FindByUsernameAsync(searchUsername);

        // Assert
        Assert.That(result, Is.Null, "Result should be null for non-existent user");
    }

    /// <summary>
    /// Test: SQL injection attempt in search parameter is treated as literal string.
    /// Purpose: Prove that SQL injection syntax in the parameter does not affect the query logic.
    /// Requirement: Verify via SqlCommandCapturingInterceptor that the injection attempt
    /// is in the DbParameter, not in the SQL command text.
    /// </summary>
    [Test]
    public async Task FindByUsernameAsync_SQLInjectionAttempt_TreatsAsLiteral()
    {
        // Arrange
        var injectionAttempt = "' OR 1=1 --";
        _interceptor?.Clear();

        // Act
        // Note: We bypass InputValidationService here to test the Repository's parameterization directly
        var result = await _repository!.FindByUsernameAsync(injectionAttempt);

        // Assert 1: Query should return no results (no user with this exact username)
        Assert.That(result, Is.Null, 
            "SQL injection attempt should not return all users; injection string is treated as literal");

        // Assert 2: Verify via interceptor that the injection attempt is in parameters, not SQL text
        Assert.That(_interceptor?.CapturedCommands.Count, Is.GreaterThan(0), 
            "Should have captured SQL commands");

        var lastCommand = _interceptor!.CapturedCommands.Last();

        // The injection string should NOT be in the SQL text
        Assert.That(lastCommand.ContainsValueInSqlText(injectionAttempt), Is.False,
            "SQL injection attempt should NOT be found in SQL command text (would indicate string concatenation)");

        // The injection string SHOULD be in a parameter (safe, parameterized)
        Assert.That(lastCommand.ContainsValueInParameters(injectionAttempt), Is.True,
            "SQL injection attempt should be found in DbParameters (indicates safe parameterization)");
    }

    /// <summary>
    /// Test: SQL injection attempt with DROP TABLE syntax is treated as literal.
    /// Purpose: Verify that destructive SQL commands cannot be injected via parameters.
    /// Requirement: Verify that the attack string is in parameters, not SQL text.
    /// </summary>
    [Test]
    public async Task FindByUsernameAsync_DropTableAttempt_TreatsAsLiteral()
    {
        // Arrange
        var dropTableAttempt = "'; DROP TABLE DemoUsers; --";
        _interceptor?.Clear();

        // Act
        var result = await _repository!.FindByUsernameAsync(dropTableAttempt);

        // Assert 1: Query should return no results
        Assert.That(result, Is.Null, 
            "DROP TABLE injection should not execute; treated as literal username");

        // Assert 2: Verify that DemoUsers table still exists (wasn't dropped)
        var existingUsers = _context!.DemoUsers.ToList();
        Assert.That(existingUsers.Count, Is.GreaterThan(0),
            "DemoUsers table should still exist and contain data; DROP TABLE was not executed");

        // Assert 3: Verify parameterization via interceptor
        Assert.That(_interceptor?.CapturedCommands.Count, Is.GreaterThan(0),
            "Should have captured SQL commands");

        var lastCommand = _interceptor!.CapturedCommands.Last();

        // The DROP TABLE attempt should NOT be in the SQL text
        Assert.That(lastCommand.ContainsValueInSqlText("DROP"), Is.False,
            "DROP TABLE keyword should NOT appear in SQL text (indicates injection attempt was prevented)");

        // The DROP TABLE attempt SHOULD be in parameters
        Assert.That(lastCommand.ContainsValueInParameters(dropTableAttempt), Is.True,
            "Injection attempt should be in parameters (safe parameterization)");
    }

    /// <summary>
    /// Test: Adding a valid user works correctly.
    /// Purpose: Verify that the AddAsync method functions properly with parameterized inserts.
    /// </summary>
    [Test]
    public async Task AddAsync_ValidDemoUser_SavesSuccessfully()
    {
        // Arrange
        var newUser = new DemoUser
        {
            Username = "new_test_user_add",
            Email = "newuser@example.com"
        };
        _interceptor?.Clear();

        // Act
        var result = await _repository!.AddAsync(newUser);

        // Assert 1: User should be saved with an ID
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0), "User should have been assigned an ID");
        Assert.That(result.Username, Is.EqualTo(newUser.Username));
        Assert.That(result.Email, Is.EqualTo(newUser.Email));

        // Assert 2: User should be retrievable from database
        var retrievedUser = await _repository.FindByUsernameAsync(newUser.Username);
        Assert.That(retrievedUser, Is.Not.Null, "Added user should be retrievable");
        Assert.That(retrievedUser!.Email, Is.EqualTo(newUser.Email));

        // Assert 3: Verify via interceptor that INSERT used parameters
        Assert.That(_interceptor?.CapturedCommands.Count, Is.GreaterThan(0),
            "Should have captured SQL commands");

        var insertCommand = _interceptor!.CapturedCommands.FirstOrDefault(c => c.CommandText.Contains("INSERT", StringComparison.OrdinalIgnoreCase));
        Assert.That(insertCommand, Is.Not.Null, "Should have executed an INSERT command");

        // Values should be in parameters, not in SQL text
        Assert.That(insertCommand!.Parameters.Values.Cast<object>()
            .Any(v => v is string s && s.Equals(newUser.Username)),
            Is.True, "Username should be in parameters");
        Assert.That(insertCommand.Parameters.Values.Cast<object>()
            .Any(v => v is string s && s.Equals(newUser.Email)),
            Is.True, "Email should be in parameters");
    }

    /// <summary>
    /// Test: SQL injection via email field parameter (bypass validation).
    /// Purpose: Demonstrate that parameterization protects even if validation is bypassed.
    /// </summary>
    [Test]
    public async Task AddAsync_EmailWithSQLInjectionBypassingValidation_SafeParameterization()
    {
        // Arrange
        // Note: This bypasses InputValidationService, testing the Repository directly
        var injectionEmail = "user'; DROP TABLE DemoUsers; --@example.com";
        var userWithInjection = new DemoUser
        {
            Username = "injection_test_user",
            Email = injectionEmail
        };
        _interceptor?.Clear();

        // Act
        var result = await _repository!.AddAsync(userWithInjection);

        // Assert 1: User should be saved despite injection attempt in email
        Assert.That(result.Id, Is.GreaterThan(0), 
            "User with injection in email should be saved (parameterization protects)");

        // Assert 2: Verify that DemoUsers table still exists (wasn't dropped)
        var allUsers = _context!.DemoUsers.ToList();
        Assert.That(allUsers.Count, Is.GreaterThan(0),
            "DemoUsers table should still exist; DROP TABLE was not executed");

        // Assert 3: Email should be stored exactly as provided (not truncated by injection)
        var savedUser = await _repository.FindByUsernameAsync(userWithInjection.Username);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.Email, Is.EqualTo(injectionEmail),
            "Email should be stored exactly, with injection string intact but harmless");

        // Assert 4: Verify parameterization via interceptor
        var insertCommand = _interceptor!.CapturedCommands.FirstOrDefault(c => 
            c.CommandText.Contains("INSERT", StringComparison.OrdinalIgnoreCase));
        Assert.That(insertCommand, Is.Not.Null);
        Assert.That(insertCommand!.ContainsValueInSqlText("DROP"), Is.False,
            "DROP keyword should not appear in SQL text");
        Assert.That(insertCommand.ContainsValueInParameters(injectionEmail), Is.True,
            "Injection attempt should be safely in parameters");
    }
}
