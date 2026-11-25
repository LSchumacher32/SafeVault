using NUnit.Framework;
using SafeVault.Services;

namespace SafeVault.Tests;

/// <summary>
/// Comprehensive test suite for InputValidationService.
/// Tests valid inputs, invalid inputs, edge cases, and attack vectors.
/// Note: These tests demonstrate validation behavior, not complete security proofs.
/// </summary>
[TestFixture]
public class InputValidationServiceTests
{
    // ============================================================================
    // VALID INPUT TESTS
    // ============================================================================

    [Test]
    public void TryValidate_ValidUsernameAndEmail_ReturnsTrue()
    {
        // Arrange
        string username = "john_doe";
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("john_doe"));
        Assert.That(normalizedEmail, Is.EqualTo("john@example.com"));
        Assert.That(errorMessage, Is.Empty);
    }

    [Test]
    public void TryValidate_UsernameWithUnderscoreAndHyphen_ReturnsTrue()
    {
        // Arrange
        string username = "valid_user-123";
        string email = "user@domain.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("valid_user-123"));
        Assert.That(normalizedEmail, Is.EqualTo("user@domain.com"));
        Assert.That(errorMessage, Is.Empty);
    }

    [Test]
    public void TryValidate_InputWithLeadingAndTrailingWhitespace_TrimmedAndValid()
    {
        // Arrange
        string username = "  john_doe  ";
        string email = "  john@example.com  ";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("john_doe"));
        Assert.That(normalizedEmail, Is.EqualTo("john@example.com"));
        Assert.That(errorMessage, Is.Empty);
    }

    // ============================================================================
    // INVALID / MISSING INPUT TESTS
    // ============================================================================

    [Test]
    public void TryValidate_NullUsername_ReturnsFalse()
    {
        // Arrange
        string? username = null;
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_EmptyUsername_ReturnsFalse()
    {
        // Arrange
        string username = "";
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_UsernameOnlyWhitespace_ReturnsFalse()
    {
        // Arrange
        string username = "   ";
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_NullEmail_ReturnsFalse()
    {
        // Arrange
        string username = "john_doe";
        string? email = null;

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_EmptyEmail_ReturnsFalse()
    {
        // Arrange
        string username = "john_doe";
        string email = "";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    // ============================================================================
    // LENGTH VIOLATION TESTS
    // ============================================================================

    [Test]
    public void TryValidate_UsernameShorterThan3Characters_ReturnsFalse()
    {
        // Arrange
        string username = "ab";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        Assert.That(errorMessage, Contains.Substring("at least 3"));
    }

    [Test]
    public void TryValidate_UsernameLongerThan100Characters_ReturnsFalse()
    {
        // Arrange
        string username = new string('a', 101);
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        Assert.That(errorMessage, Contains.Substring("exceed 100"));
    }

    [Test]
    public void TryValidate_EmailLongerThan100Characters_ReturnsFalse()
    {
        // Arrange
        string username = "valid_user";
        string email = "a" + new string('a', 90) + "@example.com"; // Over 100 chars

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    // ============================================================================
    // CHARACTER VIOLATION TESTS
    // ============================================================================

    [Test]
    public void TryValidate_UsernameWithSpecialCharacter_ReturnsFalse()
    {
        // Arrange
        string username = "john@doe";
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        Assert.That(errorMessage, Contains.Substring("invalid characters"));
    }

    [Test]
    public void TryValidate_UsernameWithSpace_ReturnsFalse()
    {
        // Arrange
        string username = "john doe";
        string email = "john@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    // ============================================================================
    // EMAIL VALIDATION TESTS
    // ============================================================================

    [Test]
    public void TryValidate_InvalidEmailFormat_ReturnsFalse()
    {
        // Arrange
        string username = "valid_user";
        string email = "not-an-email";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        Assert.That(errorMessage, Contains.Substring("format"));
    }

    [Test]
    public void TryValidate_ValidEmailWithSubdomain_ReturnsTrue()
    {
        // Arrange
        string username = "valid_user";
        string email = "user@mail.example.co.uk";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("valid_user"));
        Assert.That(normalizedEmail, Is.EqualTo("user@mail.example.co.uk"));
        Assert.That(errorMessage, Is.Empty);
    }

    // ============================================================================
    // ATTACK INPUT TESTS (SQL INJECTION, XSS, CONTROL CHARACTERS, ETC.)
    // ============================================================================

    [Test]
    public void TryValidate_UsernameWithSQLInjectionAttempt_ReturnsFalse()
    {
        // Arrange - Attack: ' OR 1=1 --
        string username = "' OR 1=1 --";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        // The validation should reject this due to invalid characters (', space)
    }

    [Test]
    public void TryValidate_UsernameWithDropTableAttempt_ReturnsFalse()
    {
        // Arrange - Attack: '; DROP TABLE DemoUsers;--
        string username = "'; DROP TABLE DemoUsers;--";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_UsernameWithXSSAttempt_ReturnsFalse()
    {
        // Arrange - Attack: <script>alert('xss')</script>
        string username = "<script>alert('xss')</script>";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_UsernameWithEventHandlerXSS_ReturnsFalse()
    {
        // Arrange - Attack: <img src=x onerror=alert(1)>
        string username = "<img src=x onerror=alert(1)>";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_EmailWithControlCharacter_ReturnsFalse()
    {
        // Arrange - Email with embedded null/tab character
        string username = "valid_user";
        string email = "user\0@example.com"; // Null character

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
        Assert.That(errorMessage, Contains.Substring("Control characters"));
    }

    [Test]
    public void TryValidate_EmailWithLessThanCharacter_ReturnsFalse()
    {
        // Arrange - Email with < character
        string username = "valid_user";
        string email = "<user@example.com>";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    [Test]
    public void TryValidate_EmailWithGreaterThanCharacter_ReturnsFalse()
    {
        // Arrange - Email with > character
        string username = "valid_user";
        string email = "user@example.com>";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(normalizedUsername, Is.Empty);
        Assert.That(normalizedEmail, Is.Empty);
        Assert.That(errorMessage, Is.Not.Empty);
    }

    // ============================================================================
    // EDGE CASE TESTS
    // ============================================================================

    [Test]
    public void TryValidate_UsernameExactly3Characters_ReturnsTrue()
    {
        // Arrange - Minimum valid length
        string username = "abc";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("abc"));
    }

    [Test]
    public void TryValidate_UsernameExactly100Characters_ReturnsTrue()
    {
        // Arrange - Maximum valid length
        string username = new string('a', 100);
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo(username));
    }

    [Test]
    public void TryValidate_EmailExactly100Characters_ReturnsTrue()
    {
        // Arrange - Maximum valid email length
        string username = "valid_user";
        string email = "a" + new string('b', 86) + "@example.com"; // Exactly 100 chars

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedEmail, Is.EqualTo(email));
    }

    [Test]
    public void TryValidate_AllNumericUsername_ReturnsTrue()
    {
        // Arrange - Digits are allowed
        string username = "123456";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("123456"));
    }

    [Test]
    public void TryValidate_UsernameWithConsecutiveHyphensAndUnderscores_ReturnsTrue()
    {
        // Arrange - Multiple special allowed characters
        string username = "user--__--name";
        string email = "user@example.com";

        // Act
        bool result = InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(normalizedUsername, Is.EqualTo("user--__--name"));
    }
}
