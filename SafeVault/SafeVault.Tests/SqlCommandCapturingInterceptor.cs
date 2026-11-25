using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace SafeVault.Tests;

/// <summary>
/// Database command interceptor that captures SQL commands and their parameters.
/// Used to verify that parameterized queries are actually parameterized
/// and not vulnerable to SQL injection via string concatenation.
/// </summary>
public class SqlCommandCapturingInterceptor : DbCommandInterceptor
{
    private readonly List<CapturedCommand> _commands = [];

    /// <summary>
    /// Gets all captured commands during the test.
    /// </summary>
    public IReadOnlyList<CapturedCommand> CapturedCommands => _commands.AsReadOnly();

    /// <summary>
    /// Clears all captured commands.
    /// </summary>
    public void Clear()
    {
        _commands.Clear();
    }

    /// <summary>
    /// Called before a database command is executed.
    /// Captures the SQL text and parameters.
    /// </summary>
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Called before a non-query database command is executed.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Called before a scalar database command is executed.
    /// </summary>
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Captures command details from a DbCommand.
    /// </summary>
    private void CaptureCommand(DbCommand command)
    {
        var parameters = new Dictionary<string, object?>();

        foreach (DbParameter param in command.Parameters)
        {
            parameters[param.ParameterName] = param.Value;
        }

        _commands.Add(new CapturedCommand
        {
            CommandText = command.CommandText,
            Parameters = parameters,
            CapturedAt = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Represents a captured database command with its parameters.
/// </summary>
public class CapturedCommand
{
    /// <summary>
    /// The SQL command text.
    /// </summary>
    public required string CommandText { get; set; }

    /// <summary>
    /// Dictionary of parameter names to values.
    /// </summary>
    public required Dictionary<string, object?> Parameters { get; set; }

    /// <summary>
    /// When the command was captured.
    /// </summary>
    public DateTime CapturedAt { get; set; }

    /// <summary>
    /// Checks if a specific value exists in the SQL command text (not parameters).
    /// Used to verify that values are NOT embedded in the SQL string.
    /// </summary>
    public bool ContainsValueInSqlText(string value)
    {
        // Case-insensitive search for the value in the SQL text
        return CommandText.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a specific value exists in any parameter.
    /// Used to verify that values ARE in parameters (safe).
    /// </summary>
    public bool ContainsValueInParameters(string value)
    {
        return Parameters.Values.Any(v =>
            v is string strValue && strValue.Equals(value, StringComparison.Ordinal));
    }
}
