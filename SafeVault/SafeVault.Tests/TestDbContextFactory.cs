using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SafeVault.Data;

namespace SafeVault.Tests;

/// <summary>
/// Factory for creating ApplicationDbContext instances configured for integration tests.
/// Uses environment variable for connection string to avoid hardcoding credentials.
/// </summary>
public static class TestDbContextFactory
{
    private const string ConnectionStringEnvVar = "SAFEVAULT_TEST_DB_CONNECTION_STRING";

    /// <summary>
    /// Creates an ApplicationDbContext configured with the test database connection string.
    /// </summary>
    /// <returns>Configured ApplicationDbContext instance, or null if test database is not configured.</returns>
    public static ApplicationDbContext? CreateTestContext(DbCommandInterceptor? interceptor = null)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString);

        if (interceptor != null)
        {
            optionsBuilder.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the environment variable name used for test database configuration.
    /// </summary>
    public static string GetConnectionStringEnvVarName() => ConnectionStringEnvVar;

    /// <summary>
    /// Checks if the test database is configured via environment variable.
    /// </summary>
    public static bool IsTestDatabaseConfigured()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionStringEnvVar));
    }

    /// <summary>
    /// Gets a skip reason if test database is not configured.
    /// </summary>
    public static string GetSkipReasonIfNotConfigured()
    {
        if (IsTestDatabaseConfigured())
        {
            return string.Empty;
        }

        return $"Test database not configured. Set environment variable '{ConnectionStringEnvVar}' " +
               "to a SQL Server connection string to enable integration tests. Example: " +
               "set SAFEVAULT_TEST_DB_CONNECTION_STRING=Server=(localdb)\\mssqllocaldb;Database=SafeVault_Tests;Integrated Security=true;";
    }
}
