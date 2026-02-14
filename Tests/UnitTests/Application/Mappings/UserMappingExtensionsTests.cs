using Application.DTO.User;
using Application.Mappings;
using Domain.Entities;
using Domain.Repositories;
using Moq;
using Xunit;

namespace Tests.UnitTests.Application.Mappings;

/// <summary>
/// Testes unitários para UserMappingExtensions.
/// </summary>
public class UserMappingExtensionsTests
{
    #region AddUserRequest to User Entity Tests

    [Fact]
    public void ToEntity_AddUserRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new AddUserRequest
        {
            UserId = "test-user",
            Name = "Test User",
            Email = "TEST@EXAMPLE.COM",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.Equal("TEST-USER", entity.UserId); // Deve estar em maiúsculas
        Assert.Equal("TEST USER", entity.Name); // Deve estar em maiúsculas
        Assert.Equal("test@example.com", entity.Email); // Deve estar em minúsculas
        Assert.True(entity.IsActive);
        Assert.False(entity.IsAdmin);
        Assert.False(entity.IsTechAccount);
    }

    [Fact]
    public void ToEntity_AddUserRequest_WithAdminTrue_ShouldSetIsAdminCorrectly()
    {
        // Arrange
        var request = new AddUserRequest
        {
            UserId = "admin-user",
            Name = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = true,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.True(entity.IsAdmin);
        Assert.Equal("ADMIN-USER", entity.UserId);
    }

    [Fact]
    public void ToEntity_AddUserRequest_WithTechAccountTrue_ShouldSetIsTechAccountCorrectly()
    {
        // Arrange
        var request = new AddUserRequest
        {
            UserId = "tech-user",
            Name = "Tech User",
            Email = "tech@example.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = true
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.True(entity.IsTechAccount);
    }

    #endregion

    #region UpdateUserRequest to User Entity Tests

    [Fact]
    public void ToEntity_UpdateUserRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            UserId = "existing-user",
            Name = "Updated Name",
            Email = "UPDATED@EXAMPLE.COM",
            Password = "NewPassword123!",
            IsActive = false,
            IsAdmin = true,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.Equal("EXISTING-USER", entity.UserId); // Deve estar em maiúsculas
        Assert.Equal("UPDATED NAME", entity.Name); // Deve estar em maiúsculas
        Assert.Equal("updated@example.com", entity.Email); // Deve estar em minúsculas
        Assert.False(entity.IsActive);
        Assert.True(entity.IsAdmin);
        Assert.False(entity.IsTechAccount);
    }

    [Fact]
    public void ToEntity_UpdateUserRequest_DeactivatingUser_ShouldSetIsActiveFalse()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            UserId = "user-to-deactivate",
            Name = "User",
            Email = "user@example.com",
            Password = "Password123!",
            IsActive = false, // Desativando
            IsAdmin = false,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.False(entity.IsActive);
    }

    #endregion

    #region User Entity to UserResponse Tests

    [Fact]
    public void ToResponse_UserEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "TEST-USER",
            "TEST USER",
            "test@example.com",
            isActive: true,
            isAdmin: false,
            isTechAccount: false,
            updatedAt: new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        );

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.Equal("TEST-USER", response.UserId);
        Assert.Equal("TEST USER", response.Name);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("hashed_password", response.PasswordHash);
        Assert.True(response.IsActive);
        Assert.False(response.IsAdmin);
        Assert.False(response.IsTechAccount);
        Assert.NotNull(response.CreatedAt);
        Assert.NotNull(response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_UserEntity_WithNullUpdatedAt_ShouldMapCorrectly()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "NEW-USER",
            "NEW USER",
            "new@example.com",
            updatedAt: null
        );

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.Equal("NEW-USER", response.UserId);
        Assert.NotNull(response.CreatedAt);
        Assert.Null(response.UpdatedAt); // Deve ser null
    }

    [Fact]
    public void ToResponse_AdminUser_ShouldMapIsAdminCorrectly()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "ADMIN-USER",
            "ADMIN USER",
            "admin@example.com",
            isAdmin: true
        );

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.True(response.IsAdmin);
    }

    [Fact]
    public void ToResponse_TechAccountUser_ShouldMapIsTechAccountCorrectly()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "TECH-USER",
            "TECH USER",
            "tech@example.com",
            isTechAccount: true
        );

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.True(response.IsTechAccount);
    }

    [Fact]
    public void ToResponse_InactiveUser_ShouldMapIsActiveFalse()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "INACTIVE-USER",
            "INACTIVE USER",
            "inactive@example.com",
            isActive: false,
            updatedAt: DateTime.UtcNow
        );
        entity.CreatedAt = DateTime.UtcNow.AddDays(-60);

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.False(response.IsActive);
        Assert.NotNull(response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_ShouldInitializeLinksCollection()
    {
        // Arrange
        var entity = CreateUserWithPassword(
            "TEST-USER",
            "TEST USER",
            "test@example.com"
        );

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response.Links);
        Assert.Empty(response.Links); // Deve ser uma lista vazia inicialmente
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("lowercase-user")]
    [InlineData("MixedCase-User")]
    [InlineData("UPPERCASE-USER")]
    public void ToEntity_AddUserRequest_ShouldAlwaysConvertUserIdToUpperCase(string userId)
    {
        // Arrange
        var request = new AddUserRequest
        {
            UserId = userId,
            Name = "Test",
            Email = "test@example.com",
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.Equal(userId.ToUpper(), entity.UserId);
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.Com")]
    [InlineData("test@example.com")]
    public void ToEntity_AddUserRequest_ShouldAlwaysConvertEmailToLowerCase(string email)
    {
        // Arrange
        var request = new AddUserRequest
        {
            UserId = "TEST-USER",
            Name = "Test",
            Email = email,
            Password = "Password123!",
            IsActive = true,
            IsAdmin = false,
            IsTechAccount = false
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        Assert.Equal(email.ToLower(), entity.Email);
    }

    #endregion

    #region Helper Methods

    private User CreateUserWithPassword(
        string userId,
        string name,
        string email,
        bool isActive = true,
        bool isAdmin = false,
        bool isTechAccount = false,
        DateTime? updatedAt = null)
    {
        var mockPasswordHasher = new Mock<IPasswordHasherRepository>();
        mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        var user = new User
        {
            UserId = userId,
            Name = name,
            Email = email,
            IsActive = isActive,
            IsAdmin = isAdmin,
            IsTechAccount = isTechAccount,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = updatedAt
        };

        user.SetPassword("Password123!", mockPasswordHasher.Object);

        return user;
    }

    #endregion
}
