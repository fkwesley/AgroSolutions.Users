using Application.DTO.User;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Application.Services;

/// <summary>
/// Testes unitários para UserService.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasherRepository> _mockPasswordHasher;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasherRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserService(null!, _mockPasswordHasher.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullPasswordHasher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserService(_mockUserRepository.Object, null!, _mockLogger.Object));
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WithExistingUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = CreateTestUsers(3);
        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        var user = CreateTestUser("TEST-USER-1");
        _mockUserRepository.Setup(x => x.GetUserByIdAsync("TEST-USER-1"))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync("TEST-USER-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-USER-1", result.UserId);
    }

    #endregion

    #region AddUserAsync Tests

    [Fact]
    public async Task AddUserAsync_WithValidUser_ShouldAddUser()
    {
        // Arrange
        var addUserRequest = new AddUserRequest
        {
            UserId = "NEW-USER",
            Name = "New User",
            Email = "newuser@test.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>();
        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        var addedUser = new User
        {
            UserId = "NEW-USER",
            Name = "New User",
            Email = "newuser@test.com",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        // Use reflection to set PasswordHash (private setter)
        typeof(User).GetProperty("PasswordHash")!.SetValue(addedUser, "hashedpassword");

        _mockUserRepository.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(addedUser);

        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        // Act
        var result = await _userService.AddUserAsync(addUserRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW-USER", result.UserId);
        _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task AddUserAsync_WithDuplicateUserId_ShouldThrowValidationException()
    {
        // Arrange
        var addUserRequest = new AddUserRequest
        {
            UserId = "EXISTING-USER",
            Name = "Test User",
            Email = "test@test.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("EXISTING-USER", isActive: true)
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _userService.AddUserAsync(addUserRequest));
    }

    [Fact]
    public async Task AddUserAsync_WithDuplicateEmail_ShouldThrowValidationException()
    {
        // Arrange
        var addUserRequest = new AddUserRequest
        {
            UserId = "NEW-USER",
            Name = "New User",
            Email = "existing@test.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("EXISTING-USER", email: "existing@test.com", isActive: true)
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _userService.AddUserAsync(addUserRequest));
    }

    [Fact]
    public async Task AddUserAsync_WithInactiveUserWithSameEmail_ShouldAllowCreation()
    {
        // Arrange
        var addUserRequest = new AddUserRequest
        {
            UserId = "NEW-USER",
            Name = "New User",
            Email = "test@test.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("OLD-USER", email: "test@test.com", isActive: false)
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        var addedUser = new User
        {
            UserId = "NEW-USER",
            Name = "New User",
            Email = "test@test.com",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        // Use reflection to set PasswordHash (private setter)
        typeof(User).GetProperty("PasswordHash")!.SetValue(addedUser, "hashedpassword");

        _mockUserRepository.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(addedUser);

        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        // Act
        var result = await _userService.AddUserAsync(addUserRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW-USER", result.UserId);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidUser_ShouldUpdateUser()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            UserId = "TEST-USER",
            Name = "Updated Name",
            Email = "updated@test.com",
            Password = "NewPassword123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("TEST-USER", email: "old@test.com")
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        var updatedUser = new User
        {
            UserId = "TEST-USER",
            Name = "Updated Name",
            Email = "updated@test.com",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        // Use reflection to set PasswordHash (private setter)
        typeof(User).GetProperty("PasswordHash")!.SetValue(updatedUser, "hashedpassword");

        _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        // Act
        var result = await _userService.UpdateUserAsync(updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-USER", result.UserId);
        Assert.Equal("updated@test.com", result.Email);
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            UserId = "NONEXISTENT-USER",
            Name = "Test",
            Email = "test@test.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _userService.UpdateUserAsync(updateRequest));
    }

    [Fact]
    public async Task UpdateUserAsync_WithEmailUsedByAnotherUser_ShouldThrowValidationException()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            UserId = "USER-1",
            Name = "User 1",
            Email = "user2@test.com", // Email já usado por USER-2
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("USER-1", email: "user1@test.com", isActive: true),
            CreateTestUser("USER-2", email: "user2@test.com", isActive: true)
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _userService.UpdateUserAsync(updateRequest));
    }

    [Fact]
    public async Task UpdateUserAsync_DeactivatingUser_ShouldUpdateIsActiveToFalse()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@test.com",
            Password = "Password123!",
            IsActive = false, // Desativando
            IsAdmin = false
        };

        var existingUsers = new List<User>
        {
            CreateTestUser("TEST-USER", isActive: true)
        };

        _mockUserRepository.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(existingUsers);

        var updatedUser = CreateTestUser("TEST-USER", isActive: false);
        updatedUser.UpdatedAt = DateTime.UtcNow;

        _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        // Act
        var result = await _userService.UpdateUserAsync(updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithValidUserId_ShouldDeleteUser()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.DeleteUserAsync("TEST-USER"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync("test-user");

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.DeleteUserAsync("TEST-USER"), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.DeleteUserAsync("NONEXISTENT"))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ValidateCredentialsAsync Tests

    [Fact]
    public async Task ValidateCredentialsAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var user = CreateTestUser("TEST-USER");
        _mockUserRepository.Setup(x => x.GetUserByIdAsync("TEST-USER"))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(x => x.VerifyPassword("correctpassword", user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _userService.ValidateCredentialsAsync("test-user", "correctpassword");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-USER", result.UserId);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser("TEST-USER");
        _mockUserRepository.Setup(x => x.GetUserByIdAsync("TEST-USER"))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(x => x.VerifyPassword("wrongpassword", user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _userService.ValidateCredentialsAsync("test-user", "wrongpassword"));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetUserByIdAsync("NONEXISTENT"))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _userService.ValidateCredentialsAsync("nonexistent", "anypassword"));
    }

    #endregion

    #region Helper Methods

    private List<User> CreateTestUsers(int count)
    {
        var users = new List<User>();
        for (int i = 1; i <= count; i++)
        {
            users.Add(CreateTestUser($"TEST-USER-{i}", email: $"user{i}@test.com"));
        }
        return users;
    }

    private User CreateTestUser(
        string userId,
        string name = "Test User",
        string email = "test@example.com",
        bool isActive = true,
        bool isAdmin = false)
    {
        var user = new User
        {
            UserId = userId.ToUpper(),
            Name = name,
            Email = email.ToLower(),
            IsActive = isActive,
            IsAdmin = isAdmin,
            IsTechAccount = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        // Mock SetPassword para definir o PasswordHash
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        user.SetPassword("Password123!", _mockPasswordHasher.Object);

        return user;
    }

    #endregion
}
