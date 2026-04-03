using System;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Eventor.Services.Exceptions;
using FluentAssertions;
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
        _authService = new AuthService(_userRepository);
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
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var phone = "+1111111111";
        var password = "myPassword";
        await _authService.RegisterAsync("Login User", phone, Gender.Male, password);

        // Act
        var token = await _authService.LoginAsync(phone, password);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Токен — это Base64 от Guid, поэтому проверим, что его можно декодировать
        var bytes = Convert.FromBase64String(token);
        bytes.Length.Should().Be(16); // Guid имеет 16 байт
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