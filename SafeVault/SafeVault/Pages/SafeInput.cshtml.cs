using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Models;
using SafeVault.Services;

namespace SafeVault.Pages;

/// <summary>
/// Razor Page for the safe input training exercise.
/// Demonstrates server-side validation and secure data handling.
/// </summary>
public class SafeInputModel : PageModel
{
    private readonly DemoUserRepository _demoUserRepository;
    private readonly ILogger<SafeInputModel> _logger;

    public SafeInputModel(DemoUserRepository demoUserRepository, ILogger<SafeInputModel> logger)
    {
        _demoUserRepository = demoUserRepository;
        _logger = logger;
    }

    /// <summary>
    /// Error message from validation (save operation).
    /// </summary>
    public string? ValidationError { get; set; }

    /// <summary>
    /// Success message after save.
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Error message from search operation.
    /// </summary>
    public string? SearchError { get; set; }

    /// <summary>
    /// Search result, if any.
    /// </summary>
    public DemoUser? SearchResult { get; set; }

    /// <summary>
    /// Indicates whether a search was performed.
    /// </summary>
    public bool SearchPerformed { get; set; }

    /// <summary>
    /// GET handler - displays the form.
    /// </summary>
    public void OnGet()
    {
        // Check for success message from previous redirect (Post/Redirect/Get)
        if (TempData["SuccessMessage"] is string successMsg)
        {
            SuccessMessage = successMsg;
            TempData.Remove("SuccessMessage");
        }
    }

    /// <summary>
    /// POST handler - processes form submission to save a new demo user.
    /// Uses Post/Redirect/Get pattern to prevent resubmission.
    /// </summary>
    public async Task<IActionResult> OnPost(string? username, string? email)
    {
        // Validate input using InputValidationService
        if (!InputValidationService.TryValidate(
            username,
            email,
            out var normalizedUsername,
            out var normalizedEmail,
            out var errorMessage))
        {
            ValidationError = errorMessage;
            return Page();
        }

        try
        {
            // Create and save demo user with normalized values
            var demoUser = new DemoUser
            {
                Username = normalizedUsername,
                Email = normalizedEmail
            };

            await _demoUserRepository.AddAsync(demoUser);

            // Clear any previous errors
            ValidationError = null;

            // Use Post/Redirect/Get: redirect with success message
            TempData["SuccessMessage"] = $"Demo user '{normalizedUsername}' saved successfully!";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            // Log error without sensitive data
            _logger.LogError(ex, "An error occurred while saving demo user: {ErrorMessage}", ex.Message);

            // Show generic error message to user
            ValidationError = "An unexpected error occurred while saving. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// POST handler for search - finds a demo user by exact username.
    /// </summary>
    public async Task<IActionResult> OnPostSearch(string? searchUsername)
    {
        if (string.IsNullOrWhiteSpace(searchUsername))
        {
            SearchError = "Please enter a username to search.";
            SearchPerformed = true;
            return Page();
        }

        try
        {
            // Search using the repository's parameterized query
            SearchResult = await _demoUserRepository.FindByUsernameAsync(searchUsername);
            SearchPerformed = true;

            if (SearchResult == null)
            {
                SearchError = null; // No error, just no result
            }

            // Check for success message from previous redirect
            if (TempData["SuccessMessage"] is string successMsg)
            {
                SuccessMessage = successMsg;
                TempData.Remove("SuccessMessage");
            }

            return Page();
        }
        catch (Exception ex)
        {
            // Log error without sensitive data
            _logger.LogError(ex, "An error occurred while searching for demo user: {ErrorMessage}", ex.Message);

            // Show generic error message to user
            SearchError = "An unexpected error occurred while searching. Please try again.";
            SearchPerformed = true;
            return Page();
        }
    }
}
