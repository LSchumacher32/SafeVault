using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SafeVault.Services;

/// <summary>
/// Provides server-side input validation for user registration data.
/// </summary>
public static class InputValidationService
{
    /// <summary>
    /// Validates and normalizes username and email input.
    /// </summary>
    /// <param name="username">The username to validate (may be null or contain whitespace).</param>
    /// <param name="email">The email to validate (may be null or contain whitespace).</param>
    /// <param name="normalizedUsername">The trimmed and validated username, or empty string on failure.</param>
    /// <param name="normalizedEmail">The trimmed and validated email, or empty string on failure.</param>
    /// <param name="errorMessage">An empty string on success, or a descriptive error message on failure.</param>
    /// <returns>True if both inputs are valid, false otherwise.</returns>
    public static bool TryValidate(
        string? username,
        string? email,
        out string normalizedUsername,
        out string normalizedEmail,
        out string errorMessage)
    {
        normalizedUsername = string.Empty;
        normalizedEmail = string.Empty;
        errorMessage = string.Empty;

        // Trim inputs
        var trimmedUsername = username?.Trim();
        var trimmedEmail = email?.Trim();

        // Validate username
        if (string.IsNullOrEmpty(trimmedUsername))
        {
            errorMessage = "Username is required.";
            return false;
        }

        if (trimmedUsername.Length < 3)
        {
            errorMessage = "Username must be at least 3 characters long.";
            return false;
        }

        if (trimmedUsername.Length > 100)
        {
            errorMessage = "Username must not exceed 100 characters.";
            return false;
        }

        // Check for valid username characters (A-Z, a-z, 0-9, _, -)
        if (!Regex.IsMatch(trimmedUsername, @"^[a-zA-Z0-9_-]+$"))
        {
            errorMessage = "Username contains invalid characters. Only letters, digits, underscores, and hyphens are allowed.";
            return false;
        }

        // Validate email
        if (string.IsNullOrEmpty(trimmedEmail))
        {
            errorMessage = "Email is required.";
            return false;
        }

        if (trimmedEmail.Length > 100)
        {
            errorMessage = "Email must not exceed 100 characters.";
            return false;
        }

        // Check for control characters and forbidden characters (< and >)
        if (ContainsInvalidEmailCharacters(trimmedEmail))
        {
            errorMessage = "Email contains invalid characters. Control characters and '<' or '>' are not allowed.";
            return false;
        }

        // Validate email format using EmailAddressAttribute
        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(trimmedEmail))
        {
            errorMessage = "Email format is invalid.";
            return false;
        }

        // All validations passed
        normalizedUsername = trimmedUsername;
        normalizedEmail = trimmedEmail;
        return true;
    }

    /// <summary>
    /// Checks if an email contains control characters or forbidden characters (< and >).
    /// </summary>
    private static bool ContainsInvalidEmailCharacters(string email)
    {
        // Check for control characters (ASCII 0x00-0x1F and 0x7F)
        for (int i = 0; i < email.Length; i++)
        {
            char c = email[i];
            if (char.IsControl(c) || c == '<' || c == '>')
            {
                return true;
            }
        }
        return false;
    }
}
