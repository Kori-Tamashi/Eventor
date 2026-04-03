using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private UserService _userService = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Name = "Test User" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnListOfUsers_WhenFilterIsNull()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "User1" },
            new() { Id = Guid.NewGuid(), Name = "User2" }
        };
        _userRepositoryMock.Setup(r => r.GetUsersAsync(null))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(users);
        _userRepositoryMock.Verify(r => r.GetUsersAsync(null), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredUsers_WhenFilterProvided()
    {
        // Arrange
        var filter = new UserFilter { NameContains = "John" };
        var filteredUsers = new List<User> { new() { Id = Guid.NewGuid(), Name = "John Doe" } };
        _userRepositoryMock.Setup(r => r.GetUsersAsync(filter))
            .ReturnsAsync(filteredUsers);

        // Act
        var result = await _userService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("John Doe");
        _userRepositoryMock.Verify(r => r.GetUsersAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateNewId_WhenIdIsEmpty()
    {
        // Arrange
        var user = new User { Id = Guid.Empty, Name = "New User" };
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateAsync(user);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New User");
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.Id != Guid.Empty)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPreserveExistingId_WhenIdIsNotEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Existing Id User" };
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateAsync(user);

        // Assert
        result.Id.Should().Be(userId);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.Id == userId)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowUserCreateException_WhenRepositoryFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test" };
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _userService.CreateAsync(user);

        // Assert
        await act.Should().ThrowAsync<UserCreateException>()
            .WithMessage("Failed to create user.");
        _userRepositoryMock.Verify(r => r.CreateAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdate_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Name = "Old Name" };
        var updatedUser = new User { Id = userId, Name = "New Name" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.UpdateAsync(updatedUser))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateAsync(updatedUser);

        // Assert
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(updatedUser), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedUser = new User { Id = userId, Name = "Ghost" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _userService.UpdateAsync(updatedUser);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>()
            .WithMessage($"User '{userId}' was not found.");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowUserUpdateException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Name = "Existing" };
        var updatedUser = new User { Id = userId, Name = "Updated" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.UpdateAsync(updatedUser))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _userService.UpdateAsync(updatedUser);

        // Assert
        await act.Should().ThrowAsync<UserUpdateException>()
            .WithMessage("Failed to update user.");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(updatedUser), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDelete_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.DeleteAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.DeleteAsync(userId);

        // Assert
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _userService.DeleteAsync(userId);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>()
            .WithMessage($"User '{userId}' was not found.");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowUserDeleteException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.DeleteAsync(userId))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _userService.DeleteAsync(userId);

        // Assert
        await act.Should().ThrowAsync<UserDeleteException>()
            .WithMessage("Failed to delete user.");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
    }
}