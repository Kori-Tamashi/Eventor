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
public class RegistrationServiceUnitTests
{
    private Mock<IRegistrationRepository> _registrationRepositoryMock = null!;
    private Mock<IRegistrationDayRepository> _registrationDayRepositoryMock = null!;
    private RegistrationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _registrationRepositoryMock = new Mock<IRegistrationRepository>();
        _registrationDayRepositoryMock = new Mock<IRegistrationDayRepository>();
        _service = new RegistrationService(
            _registrationRepositoryMock.Object,
            _registrationDayRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldCallRepositoryAndReturnRegistration()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        var expectedRegistration = new Registration { Id = registrationId };
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, true))
            .ReturnsAsync(expectedRegistration);

        // Act
        var result = await _service.GetByIdAsync(registrationId);

        // Assert
        result.Should().BeSameAs(expectedRegistration);
        _registrationRepositoryMock.Verify(x => x.GetByIdAsync(registrationId, true), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithIncludeDaysFalse_ShouldPassParameterToRepository()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, false))
            .ReturnsAsync((Registration?)null);

        // Act
        await _service.GetByIdAsync(registrationId, false);

        // Assert
        _registrationRepositoryMock.Verify(x => x.GetByIdAsync(registrationId, false), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldCallRepositoryWithFilterAndIncludeDays()
    {
        // Arrange
        var filter = new RegistrationFilter { EventId = Guid.NewGuid() };
        var expectedList = new List<Registration> { new(), new() };
        _registrationRepositoryMock
            .Setup(x => x.GetRegistrationsAsync(filter, true))
            .ReturnsAsync(expectedList);

        // Act
        var result = await _service.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(expectedList);
        _registrationRepositoryMock.Verify(x => x.GetRegistrationsAsync(filter, true), Times.Once);
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ShouldBuildCorrectFilterAndCallRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var paginationFilter = new PaginationFilter { PageNumber = 2, PageSize = 10 };
        var expectedRegistrations = new List<Registration> { new() };
        _registrationRepositoryMock
            .Setup(x => x.GetRegistrationsAsync(
                It.Is<RegistrationFilter>(f => f.UserId == userId && f.PageNumber == 2 && f.PageSize == 10),
                true))
            .ReturnsAsync(expectedRegistrations);

        // Act
        var result = await _service.GetByUserIdAsync(userId, paginationFilter);

        // Assert
        result.Should().BeEquivalentTo(expectedRegistrations);
        _registrationRepositoryMock.Verify(x => x.GetRegistrationsAsync(
            It.Is<RegistrationFilter>(f => f.UserId == userId && f.PageNumber == 2 && f.PageSize == 10),
            true), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateIdIfEmpty_AndAddDays()
    {
        // Arrange
        var registration = new Registration { Id = Guid.Empty };
        var dayIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _registrationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Registration>()))
            .Returns(Task.CompletedTask)
            .Callback<Registration>(r => { });
        _registrationDayRepositoryMock
            .Setup(x => x.AddDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(registration, dayIds);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        _registrationRepositoryMock.Verify(x => x.CreateAsync(It.Is<Registration>(r => r.Id == result.Id)), Times.Once);
        foreach (var dayId in dayIds.Distinct())
        {
            _registrationDayRepositoryMock.Verify(x => x.AddDayAsync(result.Id, dayId), Times.Once);
        }
    }

    [TestMethod]
    public async Task CreateAsync_ShouldUseExistingId_WhenProvided()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var registration = new Registration { Id = existingId };
        var dayIds = new List<Guid> { Guid.NewGuid() };

        _registrationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Registration>()))
            .Returns(Task.CompletedTask);
        _registrationDayRepositoryMock
            .Setup(x => x.AddDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(registration, dayIds);

        // Assert
        result.Id.Should().Be(existingId);
        _registrationRepositoryMock.Verify(x => x.CreateAsync(It.Is<Registration>(r => r.Id == existingId)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldHandleDuplicateDayIds()
    {
        // Arrange
        var registration = new Registration();
        var dayId = Guid.NewGuid();
        var dayIds = new List<Guid> { dayId, dayId }; // duplicate

        _registrationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Registration>()))
            .Returns(Task.CompletedTask);
        _registrationDayRepositoryMock
            .Setup(x => x.AddDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(registration, dayIds);

        // Assert
        _registrationDayRepositoryMock.Verify(x => x.AddDayAsync(It.IsAny<Guid>(), dayId), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldWrapExceptionIntoRegistrationServiceException()
    {
        // Arrange
        var registration = new Registration();
        var dayIds = new List<Guid>();
        _registrationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Registration>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _service.CreateAsync(registration, dayIds);

        // Assert
        var exception = await act.Should().ThrowAsync<RegistrationServiceException>();
        exception.WithMessage("Failed to create registration.");
        exception.WithInnerException<Exception>().WithMessage("DB error");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateRegistration_WhenDaysNotChanged()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        var registration = new Registration { Id = registrationId };
        var existingRegistration = new Registration { Id = registrationId, Days = new List<Day>() };

        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, true))
            .ReturnsAsync(existingRegistration);
        _registrationRepositoryMock
            .Setup(x => x.UpdateAsync(registration))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(registration, dayIds: null);

        // Assert
        _registrationRepositoryMock.Verify(x => x.UpdateAsync(registration), Times.Once);
        _registrationDayRepositoryMock.Verify(x => x.AddDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _registrationDayRepositoryMock.Verify(x => x.RemoveDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSyncDays_WhenDayIdsProvided()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        var existingDay1 = Guid.NewGuid();
        var existingDay2 = Guid.NewGuid();
        var existingRegistration = new Registration
        {
            Id = registrationId,
            Days = new List<Day> { new() { Id = existingDay1 }, new() { Id = existingDay2 } }
        };
        var targetDayIds = new List<Guid> { existingDay1, Guid.NewGuid() }; // keep existingDay1, add new, remove existingDay2

        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, true))
            .ReturnsAsync(existingRegistration);
        _registrationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Registration>()))
            .Returns(Task.CompletedTask);
        _registrationDayRepositoryMock
            .Setup(x => x.AddDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _registrationDayRepositoryMock
            .Setup(x => x.RemoveDayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(existingRegistration, targetDayIds);

        // Assert
        _registrationRepositoryMock.Verify(x => x.UpdateAsync(existingRegistration), Times.Once);
        _registrationDayRepositoryMock.Verify(x => x.AddDayAsync(registrationId, targetDayIds[1]), Times.Once);
        _registrationDayRepositoryMock.Verify(x => x.RemoveDayAsync(registrationId, existingDay2), Times.Once);
        _registrationDayRepositoryMock.Verify(x => x.AddDayAsync(registrationId, existingDay1), Times.Never);
        _registrationDayRepositoryMock.Verify(x => x.RemoveDayAsync(registrationId, existingDay1), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowRegistrationServiceException_WhenRegistrationNotFound()
    {
        // Arrange
        var registration = new Registration { Id = Guid.NewGuid() };
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registration.Id, true))
            .ReturnsAsync((Registration?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(registration);

        // Assert
        var exception = await act.Should().ThrowAsync<RegistrationServiceException>();
        exception.WithMessage($"Registration '{registration.Id}' was not found.");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldWrapExceptionIntoRegistrationServiceException()
    {
        // Arrange
        var registration = new Registration { Id = Guid.NewGuid() };
        var existingRegistration = new Registration { Id = registration.Id, Days = new List<Day>() };
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registration.Id, true))
            .ReturnsAsync(existingRegistration);
        _registrationRepositoryMock
            .Setup(x => x.UpdateAsync(registration))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(registration);

        // Assert
        var exception = await act.Should().ThrowAsync<RegistrationServiceException>();
        exception.WithMessage("Failed to update registration.");
        exception.WithInnerException<Exception>().WithMessage("Update error");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldCallRepositoryDelete_WhenRegistrationExists()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        var existingRegistration = new Registration { Id = registrationId };
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, false))
            .ReturnsAsync(existingRegistration);
        _registrationRepositoryMock
            .Setup(x => x.DeleteAsync(registrationId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(registrationId);

        // Assert
        _registrationRepositoryMock.Verify(x => x.DeleteAsync(registrationId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowRegistrationServiceException_WhenRegistrationNotFound()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, false))
            .ReturnsAsync((Registration?)null);

        // Act
        Func<Task> act = async () => await _service.DeleteAsync(registrationId);

        // Assert
        var exception = await act.Should().ThrowAsync<RegistrationServiceException>();
        exception.WithMessage($"Registration '{registrationId}' was not found.");
        _registrationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldWrapExceptionIntoRegistrationServiceException()
    {
        // Arrange
        var registrationId = Guid.NewGuid();
        var existingRegistration = new Registration { Id = registrationId };
        _registrationRepositoryMock
            .Setup(x => x.GetByIdAsync(registrationId, false))
            .ReturnsAsync(existingRegistration);
        _registrationRepositoryMock
            .Setup(x => x.DeleteAsync(registrationId))
            .ThrowsAsync(new Exception("Delete error"));

        // Act
        Func<Task> act = async () => await _service.DeleteAsync(registrationId);

        // Assert
        var exception = await act.Should().ThrowAsync<RegistrationServiceException>();
        exception.WithMessage("Failed to delete registration.");
        exception.WithInnerException<Exception>().WithMessage("Delete error");
    }
}