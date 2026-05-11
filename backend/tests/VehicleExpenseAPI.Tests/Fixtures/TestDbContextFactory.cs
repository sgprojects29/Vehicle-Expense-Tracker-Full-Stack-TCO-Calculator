using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Tests.Fixtures;

/// <summary>
/// Factory for creating in-memory database contexts for testing.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new ApplicationDbContext with an in-memory database.
    /// </summary>
    public static ApplicationDbContext Create(string databaseName = "")
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            databaseName = Guid.NewGuid().ToString();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Seeds the database with a test user for authentication tests.
    /// </summary>
    public static User SeedTestUser(ApplicationDbContext context, string userId = "test-user-id")
    {
        var user = new User
        {
            Id = userId,
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        
        return user;
    }
}
