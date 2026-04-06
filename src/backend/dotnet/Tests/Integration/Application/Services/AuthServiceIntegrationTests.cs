using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Configuration;
using Application.Services;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class AuthServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private UserRepository _userRepository = null!;
    private AuthService _authService = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<UserRepository>.Instance;
        _userRepository = new UserRepository(DbContext!, logger);

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "Eventor.Tests",
            Audience = "Eventor.Tests.Client",
            Key = "IntegrationTests_SuperSecretKey_1234567890",
            ExpirationMinutes = 60
        });

        _authService = new AuthService(_userRepository, jwtOptions);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        var name = "Test User";
        var phone = "+1234567890";
        var gender = Gender.Male;
        var password = "securePassword123";

        // Act
        var result = await _authService.RegisterAsync(name, phone, gender, password);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(name);
        result.Phone.Should().Be(phone);
        result.Gender.Should().Be(gender);
        result.Role.Should().Be(UserRole.User);
        result.PasswordHash.Should().NotBe(password);
        result.PasswordHash.Should().NotBeNullOrEmpty();

        // Проверка, что пользователь действительно сохранён в БД
        var savedUser = await _userRepository.GetByIdAsync(result.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Phone.Should().Be(phone);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldThrow_WhenPhoneAlreadyExists()
    {
        // Arrange
        var phone = "+9999999999";
        var existingUser = UserFixture.Default()
            .WithPhone(phone)
            .Build();
        await _userRepository.CreateAsync(existingUser);

        // Act
        Func<Task> act = async () =>
            await _authService.RegisterAsync("New User", phone, Gender.Male, "password");

        // Assert
        await act.Should().ThrowAsync<UserLoginAlreadyExistsException>()
            .WithMessage($"User with phone '{phone}' already exists.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldReturnJwtToken_WhenCredentialsValid()
    {
        // Arrange
        var phone = "+1111111111";
        var password = "myPassword";
        await _authService.RegisterAsync("Login User", phone, Gender.Male, password);

        // Act
        var token = await _authService.LoginAsync(phone, password);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(token).Should().BeTrue();

        var jwt = tokenHandler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role);
        jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be("Login User");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var phone = "+0000000000";
        var password = "anyPassword";

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(phone, password);

        // Assert
        await act.Should().ThrowAsync<UserLoginNotFoundException>()
            .WithMessage($"User with phone '{phone}' not found.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrow_WhenIncorrectPassword()
    {
        // Arrange
        var phone = "+5555555555";
        var correctPassword = "correctPassword";
        await _authService.RegisterAsync("User", phone, Gender.Male, correctPassword);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(phone, "wrongPassword");

        // Assert
        await act.Should().ThrowAsync<IncorrectPasswordException>()
            .WithMessage("Incorrect password.");
    }
}