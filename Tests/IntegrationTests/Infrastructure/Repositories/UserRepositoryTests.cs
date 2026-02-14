using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Tests.IntegrationTests.Infrastructure.Repositories;

/// <summary>
/// Testes de integração para UserRepository.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly UsersDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsersDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WithExistingUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser("USER-1", "user1@test.com"),
            CreateTestUser("USER-2", "user2@test.com"),
            CreateTestUser("USER-3", "user3@test.com")
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldIncludeActiveAndInactiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser("ACTIVE-USER", "active@test.com", isActive: true),
            CreateTestUser("INACTIVE-USER", "inactive@test.com", isActive: false)
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllUsersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, u => u.UserId == "ACTIVE-USER" && u.IsActive);
        Assert.Contains(result, u => u.UserId == "INACTIVE-USER" && !u.IsActive);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = CreateTestUser("EXISTING-USER", "existing@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByIdAsync("EXISTING-USER");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EXISTING-USER", result.UserId);
        Assert.Equal("existing@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetUserByIdAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = CreateTestUser("TEST-USER", "test@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var resultUpper = await _repository.GetUserByIdAsync("TEST-USER");
        var resultLower = await _repository.GetUserByIdAsync("test-user");

        // Assert
        Assert.NotNull(resultUpper);
        Assert.NotNull(resultLower);
        Assert.Equal(resultUpper.UserId, resultLower.UserId);
    }

    #endregion

    #region AddUserAsync Tests

    [Fact]
    public async Task AddUserAsync_WithValidUser_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = CreateTestUser("NEW-USER", "new@test.com");

        // Act
        var result = await _repository.AddUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW-USER", result.UserId);

        var userInDb = await _context.Users.FindAsync("NEW-USER");
        Assert.NotNull(userInDb);
        Assert.Equal("new@test.com", userInDb.Email);
    }

    [Fact]
    public async Task AddUserAsync_ShouldSetCreatedAtDate()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var user = CreateTestUser("NEW-USER", "new@test.com");
        user.CreatedAt = createdAt;

        // Act
        var result = await _repository.AddUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdAt.Date, result.CreatedAt.Date);
    }

    [Fact]
    public async Task AddUserAsync_WithAdminUser_ShouldSetIsAdminCorrectly()
    {
        // Arrange
        var user = CreateTestUser("ADMIN-USER", "admin@test.com", isAdmin: true);

        // Act
        var result = await _repository.AddUserAsync(user);

        // Assert
        Assert.True(result.IsAdmin);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithExistingUser_ShouldUpdateUser()
    {
        // Arrange
        var user = CreateTestUser("UPDATE-USER", "old@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Detach para simular uma atualização externa
        _context.Entry(user).State = EntityState.Detached;

        var updatedUser = CreateTestUser("UPDATE-USER", "new@test.com");
        updatedUser.Name = "UPDATED NAME";
        updatedUser.UpdatedAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateUserAsync(updatedUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UPDATED NAME", result.Name);
        Assert.Equal("new@test.com", result.Email);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateUserAsync_DeactivatingUser_ShouldSetIsActiveFalse()
    {
        // Arrange
        var user = CreateTestUser("ACTIVE-USER", "user@test.com", isActive: true);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _context.Entry(user).State = EntityState.Detached;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateUserAsync(user);

        // Assert
        Assert.False(result.IsActive);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateUserAsync_PromotingToAdmin_ShouldSetIsAdminTrue()
    {
        // Arrange
        var user = CreateTestUser("USER", "user@test.com", isAdmin: false);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _context.Entry(user).State = EntityState.Detached;

        user.IsAdmin = true;
        user.UpdatedAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateUserAsync(user);

        // Assert
        Assert.True(result.IsAdmin);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_ShouldDeleteUser()
    {
        // Arrange
        var user = CreateTestUser("DELETE-USER", "delete@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteUserAsync("DELETE-USER");

        // Assert
        Assert.True(result);

        var userInDb = await _context.Users.FindAsync("DELETE-USER");
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteUserAsync("NONEXISTENT");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = CreateTestUser("DELETE-USER", "delete@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteUserAsync("delete-user");

        // Assert
        Assert.True(result);

        var userInDb = await _context.Users.FindAsync("DELETE-USER");
        Assert.Null(userInDb);
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser(
        string userId,
        string email,
        bool isActive = true,
        bool isAdmin = false)
    {
        var mockPasswordHasher = new Mock<IPasswordHasherRepository>();
        mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        var user = new User
        {
            UserId = userId.ToUpper(),
            Name = $"Test User {userId}",
            Email = email.ToLower(),
            IsActive = isActive,
            IsAdmin = isAdmin,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        user.SetPassword("Password123!", mockPasswordHasher.Object);

        return user;
    }

    #endregion
}
