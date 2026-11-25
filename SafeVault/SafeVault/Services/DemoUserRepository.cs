using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Services;

/// <summary>
/// Repository for managing demo users.
/// Provides data access for the training exercise separate from Identity.
/// </summary>
public class DemoUserRepository
{
    private readonly ApplicationDbContext _context;

    public DemoUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new demo user to the database.
    /// </summary>
    /// <param name="demoUser">The validated demo user to save.</param>
    /// <returns>The added demo user with Id populated.</returns>
    public async Task<DemoUser> AddAsync(DemoUser demoUser)
    {
        _context.DemoUsers.Add(demoUser);
        await _context.SaveChangesAsync();
        return demoUser;
    }

    /// <summary>
    /// Finds a demo user by exact username match using a parameterized SQL query.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The demo user if found, otherwise null.</returns>
    public async Task<DemoUser?> FindByUsernameAsync(string username)
    {
        return await _context.DemoUsers
            .FromSqlInterpolated($"SELECT * FROM DemoUsers WHERE Username = {username}")
            .FirstOrDefaultAsync();
    }
}
