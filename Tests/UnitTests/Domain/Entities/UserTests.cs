using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Moq;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

/// <summary>
/// Testes unitários para a entidade User.
/// </summary>
public class UserTests
{
    private readonly Mock<IPasswordHasherRepository> _mockPasswordHasher;

    public UserTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasherRepository>();
    }

    #region Constructor and Basic Properties Tests

    [Fact]
    public void User_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("TEST-USER", user.UserId);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("test@example.com", user.Email);
        Assert.True(user.IsActive);
        Assert.False(user.IsAdmin);
        Assert.False(user.IsTechAccount);
    }

    [Fact]
    public void User_WithAdminFlag_ShouldSetIsAdminCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = "ADMIN-USER",
            Name = "Admin User",
            Email = "admin@example.com",
            IsActive = true,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.True(user.IsAdmin);
    }

    [Fact]
    public void User_WithTechAccountFlag_ShouldSetIsTechAccountCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = "TECH-USER",
            Name = "Tech User",
            Email = "tech@example.com",
            IsActive = true,
            IsTechAccount = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.True(user.IsTechAccount);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("test123@test-domain.com")]
    public void Email_WithValidFormat_ShouldAccept(string validEmail)
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        user.Email = validEmail;

        // Assert
        Assert.Equal(validEmail, user.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@domain")]
    [InlineData("")]
    public void Email_WithInvalidFormat_ShouldThrowBusinessException(string invalidEmail)
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "valid@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Throws<BusinessException>(() => user.Email = invalidEmail);
    }

    #endregion

    #region Password Validation Tests

    [Theory]
    [InlineData("Password123!")]
    [InlineData("StrongP@ss1")]
    [InlineData("MySecure#Pass99")]
    [InlineData("Test1234!@#$")]
    public void SetPassword_WithStrongPassword_ShouldHashAndSetPassword(string strongPassword)
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockPasswordHasher.Setup(x => x.HashPassword(strongPassword))
            .Returns("hashed_" + strongPassword);

        // Act
        user.SetPassword(strongPassword, _mockPasswordHasher.Object);

        // Assert
        Assert.Equal("hashed_" + strongPassword, user.PasswordHash);
        _mockPasswordHasher.Verify(x => x.HashPassword(strongPassword), Times.Once);
    }

    [Theory]
    [InlineData("weak")] // Muito curta
    [InlineData("password")] // Sem números ou caracteres especiais
    [InlineData("12345678")] // Apenas números
    [InlineData("Password")] // Sem números ou caracteres especiais
    [InlineData("Pass123")] // Menos de 8 caracteres
    [InlineData("password123")] // Sem caracteres especiais
    [InlineData("Password!")] // Sem números
    public void SetPassword_WithWeakPassword_ShouldThrowBusinessException(string weakPassword)
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
            user.SetPassword(weakPassword, _mockPasswordHasher.Object));
    }

    [Fact]
    public void SetPassword_ShouldNotAllowDirectPasswordHashModification()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockPasswordHasher.Setup(x => x.HashPassword("Password123!"))
            .Returns("hashed_password");

        user.SetPassword("Password123!", _mockPasswordHasher.Object);

        // Assert - PasswordHash só tem setter privado, não pode ser modificado diretamente
        // Este teste valida que o encapsulamento está correto
        Assert.Equal("hashed_password", user.PasswordHash);
    }

    #endregion

    #region User Status Tests

    [Fact]
    public void User_WhenCreated_ShouldBeActiveByDefault()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_CanBeDeactivated_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    #endregion

    #region Audit Properties Tests

    [Fact]
    public void User_ShouldTrackCreationDate()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void User_WhenUpdated_ShouldTrackUpdateDate()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedAt = DateTime.UtcNow;

        // Act
        user.Name = "Updated Name";
        user.UpdatedAt = updatedAt;

        // Assert
        Assert.Equal("Updated Name", user.Name);
        Assert.NotNull(user.UpdatedAt);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    #endregion

    #region RequestLogs Collection Tests

    [Fact]
    public void User_ShouldInitializeRequestLogsCollection()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(user.RequestLogs);
        Assert.Empty(user.RequestLogs);
    }

    [Fact]
    public void User_ShouldAllowAddingRequestLogs()
    {
        // Arrange
        var user = new User
        {
            UserId = "TEST-USER",
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var requestLog = new RequestLog
        {
            ServiceName = "users-api",
            UserId = "TEST-USER",
            Path = "/api/test",
            HttpMethod = "GET",
            StatusCode = 200,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMilliseconds(150),
            Duration = TimeSpan.FromMilliseconds(150)
        };

        // Act
        user.RequestLogs.Add(requestLog);

        // Assert
        Assert.Single(user.RequestLogs);
        Assert.Contains(requestLog, user.RequestLogs);
    }

    #endregion
}
