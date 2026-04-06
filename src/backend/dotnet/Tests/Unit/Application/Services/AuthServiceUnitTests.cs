using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Application.Configuration;
using Application.Services;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class AuthServiceUnitTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private AuthService _authService;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "Eventor.Tests",
            Audience = "Eventor.Tests.Client",
            Key = "UnitTests_SuperSecretKey_1234567890",
            ExpirationMinutes = 60
        });

        _authService = new AuthService(_userRepositoryMock.Object, jwtOptions);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldCreateUser_WhenPhoneIsUnique()
    {
        // Arrange
        var phone = "+1234567890";
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.Is<UserFilter>(f => f.Phone == phone)))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _authService.RegisterAsync("Test User", phone, Gender.Male, "password123");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test User");
        result.Phone.Should().Be(phone);
        result.Gender.Should().Be(Gender.Male);
        result.Role.Should().Be(UserRole.User);
        result.PasswordHash.Should().Be(HashPassword("password123"));
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldThrowUserLoginAlreadyExistsException_WhenPhoneAlreadyExists()
    {
        // Arrange
        var phone = "+1234567890";
        var existingUser = new User { Phone = phone };
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.Is<UserFilter>(f => f.Phone == phone)))
            .ReturnsAsync(new List<User> { existingUser });

        // Act & Assert
        await _authService
            .Invoking(s => s.RegisterAsync("Test User", phone, Gender.Male, "password"))
            .Should().ThrowAsync<UserLoginAlreadyExistsException>();
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldThrowAuthServiceException_WhenRepositoryThrows()
    {
        // Arrange
        var phone = "+1234567890";
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.IsAny<UserFilter>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var ex = await _authService
            .Invoking(s => s.RegisterAsync("Test User", phone, Gender.Male, "password"))
            .Should().ThrowAsync<AuthServiceException>();
        ex.Which.Message.Should().Be("Failed to register user.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var phone = "+1234567890";
        var password = "correctPassword";
        var hashedPassword = HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Login User",
            Role = UserRole.User,
            Phone = phone,
            PasswordHash = hashedPassword
        };
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.Is<UserFilter>(f => f.Phone == phone)))
            .ReturnsAsync(new List<User> { user });

        // Act
        var token = await _authService.LoginAsync(phone, password);

        // Assert
        token.Should().NotBeNullOrEmpty();
        new JwtSecurityTokenHandler().CanReadToken(token).Should().BeTrue();
        _userRepositoryMock.Verify(x => x.GetUsersAsync(It.IsAny<UserFilter>()), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrowUserLoginNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var phone = "+1234567890";
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.Is<UserFilter>(f => f.Phone == phone)))
            .ReturnsAsync(new List<User>());

        // Act & Assert
        await _authService
            .Invoking(s => s.LoginAsync(phone, "any"))
            .Should().ThrowAsync<UserLoginNotFoundException>();
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrowIncorrectPasswordException_WhenPasswordIsWrong()
    {
        // Arrange
        var phone = "+1234567890";
        var correctHash = HashPassword("correct");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Login User",
            Role = UserRole.User,
            Phone = phone,
            PasswordHash = correctHash
        };
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.Is<UserFilter>(f => f.Phone == phone)))
            .ReturnsAsync(new List<User> { user });

        // Act & Assert
        await _authService
            .Invoking(s => s.LoginAsync(phone, "wrong"))
            .Should().ThrowAsync<IncorrectPasswordException>();
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrowAuthServiceException_WhenRepositoryThrows()
    {
        // Arrange
        var phone = "+1234567890";
        _userRepositoryMock
            .Setup(x => x.GetUsersAsync(It.IsAny<UserFilter>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        var ex = await _authService
            .Invoking(s => s.LoginAsync(phone, "password"))
            .Should().ThrowAsync<AuthServiceException>();
        ex.Which.Message.Should().Be("Failed to login user.");
    }
}