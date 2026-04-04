using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class UserServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private UserRepository _userRepository = null!;
    private UserService _userService = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<UserRepository>.Instance;
        _userRepository = new UserRepository(DbContext!, logger);
        _userService = new UserService(_userRepository);
    }

    #region вспомогательные методы

    private async Task<User> CreateValidUserAsync(
        string name = "Test User",
        string phone = "+1000000000",
        Gender gender = Gender.Male,
        UserRole role = UserRole.User,
        string passwordHash = "hash")
    {
        var user = UserFixture.Default()
            .WithName(name)
            .WithPhone(phone)
            .WithGender(gender)
            .WithRole(role)
            .WithPasswordHash(passwordHash)
            .Build();
        return await _userService.CreateAsync(user);
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldPersistUser_AndGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.Empty,
            Name = "New User",
            Phone = "+7777777777",
            Gender = Gender.Female,
            Role = UserRole.User,
            PasswordHash = "hash123"
        };

        // Act
        var result = await _userService.CreateAsync(user);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _userService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New User");
        saved.Phone.Should().Be("+7777777777");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        // Arrange
        var fixedId = Guid.NewGuid();
        var user = new User
        {
            Id = fixedId,
            Name = "Fixed Id User",
            Phone = "+8888888888",
            Gender = Gender.Male,
            Role = UserRole.Admin,
            PasswordHash = "hash"
        };

        // Act
        var result = await _userService.CreateAsync(user);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _userService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Fixed Id User");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowUserCreateException_WhenDuplicateId()
    {
        // Arrange
        var existingUser = await CreateValidUserAsync(phone: "+1111111111");
        var duplicate = new User
        {
            Id = existingUser.Id,
            Name = "Duplicate",
            Phone = "+9999999999",
            Gender = Gender.Male,
            Role = UserRole.User,
            PasswordHash = "hash"
        };

        // Act
        Func<Task> act = async () => await _userService.CreateAsync(duplicate);

        // Assert
        await act.Should().ThrowAsync<UserCreateException>();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = await CreateValidUserAsync(phone: "+1222222222");

        // Act
        var result = await _userService.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Phone.Should().Be(user.Phone);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _userService.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllUsers_WhenNoFilter()
    {
        // Arrange
        var user1 = await CreateValidUserAsync(phone: "+1333333333");
        var user2 = await CreateValidUserAsync(phone: "+1444444444");

        // Act
        var result = await _userService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(u => u.Id).Should().Contain([user1.Id, user2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByNameContains()
    {
        // Arrange
        await CreateValidUserAsync(name: "Alice Smith", phone: "+1555555555");
        await CreateValidUserAsync(name: "Bob Johnson", phone: "+1666666666");
        await CreateValidUserAsync(name: "Charlie Brown", phone: "+1777777777");

        var filter = new UserFilter { NameContains = "ali" };

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Alice Smith");
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByPhone()
    {
        // Arrange
        var targetPhone = "+1888888888";
        await CreateValidUserAsync(phone: targetPhone);
        await CreateValidUserAsync(phone: "+1999999999");

        var filter = new UserFilter { Phone = targetPhone };

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().ContainSingle();
        result.First().Phone.Should().Be(targetPhone);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByRole()
    {
        // Arrange
        await CreateValidUserAsync(role: UserRole.Admin, phone: "+1111111111");
        await CreateValidUserAsync(role: UserRole.User, phone: "+2222222222");

        var filter = new UserFilter { Role = UserRole.Admin };

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().ContainSingle();
        result.First().Role.Should().Be(UserRole.Admin);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByGender()
    {
        // Arrange
        await CreateValidUserAsync(gender: Gender.Male, phone: "+3333333333");
        await CreateValidUserAsync(gender: Gender.Female, phone: "+4444444444");

        var filter = new UserFilter { Gender = Gender.Female };

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().ContainSingle();
        result.First().Gender.Should().Be(Gender.Female);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await CreateValidUserAsync(phone: $"+10000000{i}", name: $"User{i}");
        }

        var filter = new UserFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOrderedByName()
    {
        // Arrange
        await CreateValidUserAsync(name: "Charlie", phone: "+5555555555");
        await CreateValidUserAsync(name: "Alice", phone: "+6666666666");
        await CreateValidUserAsync(name: "Bob", phone: "+7777777777");

        // Act
        var result = await _userService.GetAsync();

        // Assert
        result.Select(u => u.Name).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingUser()
    {
        // Arrange
        var user = await CreateValidUserAsync(phone: "+8888888888");
        var updated = new User
        {
            Id = user.Id,
            Name = "Updated Name",
            Phone = "+9999999999",
            Gender = Gender.Female,
            Role = UserRole.Admin,
            PasswordHash = "newhash"
        };

        // Act
        await _userService.UpdateAsync(updated);

        // Assert
        var result = await _userService.GetByIdAsync(user.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Phone.Should().Be("+9999999999");
        result.Gender.Should().Be(Gender.Female);
        result.Role.Should().Be(UserRole.Admin);
        result.PasswordHash.Should().Be("newhash");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Ghost", Phone = "+0000000000" };

        // Act
        Func<Task> act = async () => await _userService.UpdateAsync(user);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>()
            .WithMessage($"User '{user.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingUser()
    {
        // Arrange
        var user = await CreateValidUserAsync(phone: "+1010101010");

        // Act
        await _userService.DeleteAsync(user.Id);

        // Assert
        var result = await _userService.GetByIdAsync(user.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _userService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>()
            .WithMessage($"User '{id}' was not found.");
    }
}