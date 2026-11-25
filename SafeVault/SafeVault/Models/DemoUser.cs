namespace SafeVault.Models;

/// <summary>
/// Represents a demo user for training purposes.
/// Not connected to ASP.NET Core Identity.
/// </summary>
public class DemoUser
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username, required, max 100 characters.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Email, required, max 100 characters.
    /// </summary>
    public required string Email { get; set; }
}
