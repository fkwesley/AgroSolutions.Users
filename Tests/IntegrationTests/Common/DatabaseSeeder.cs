using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Moq;

namespace Tests.IntegrationTests.Common;

/// <summary>
/// Helper para popular o banco de dados de teste com dados iniciais.
/// </summary>
public static class DatabaseSeeder
{
    public static void SeedTestData(UsersDbContext context)
    {
        // Limpa dados existentes
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();

        var mockPasswordHasher = new Mock<IPasswordHasherRepository>();
        mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        // Seed Users
        var user1 = new User
        {
            UserId = "TEST-USER-1",
            Name = "Test User 1",
            Email = "user1@test.com",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        user1.SetPassword("Password123!", mockPasswordHasher.Object);

        var user2 = new User
        {
            UserId = "TEST-USER-2",
            Name = "Test User 2",
            Email = "user2@test.com",
            IsActive = true,
            IsAdmin = true,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };
        user2.SetPassword("Password123!", mockPasswordHasher.Object);

        var user3 = new User
        {
            UserId = "TEST-USER-3",
            Name = "Test User 3 (Inactive)",
            Email = "user3@test.com",
            IsActive = false,
            IsAdmin = false,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };
        user3.SetPassword("Password123!", mockPasswordHasher.Object);

        context.Users.AddRange(user1, user2, user3);
        context.SaveChanges();
    }

    public static User CreateTestUser(
        string userId = "TEST-USER", 
        string name = "Test User",
        string email = "test@example.com",
        bool isActive = true,
        bool isAdmin = false)
    {
        var mockPasswordHasher = new Mock<IPasswordHasherRepository>();
        mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        var user = new User
        {
            UserId = userId.ToUpper(),
            Name = name,
            Email = email.ToLower(),
            IsActive = isActive,
            IsAdmin = isAdmin,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        user.SetPassword("Password123!", mockPasswordHasher.Object);

        return user;
    }
}

