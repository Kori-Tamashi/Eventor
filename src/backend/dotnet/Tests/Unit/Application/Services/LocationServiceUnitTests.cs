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
using Tests.Core.Fixtures;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class LocationServiceUnitTests
{
    private Mock<ILocationRepository> _locationRepositoryMock = null!;
    private LocationService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _locationRepositoryMock = new Mock<ILocationRepository>();
        _sut = new LocationService(_locationRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnLocation_WhenExists()
    {
        // Arrange
        var expectedLocation = LocationFixture.Default().Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(expectedLocation.Id))
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _sut.GetByIdAsync(expectedLocation.Id);

        // Assert
        result.Should().BeEquivalentTo(expectedLocation);
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(expectedLocation.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnListOfLocations()
    {
        // Arrange
        var locations = new List<Location>
        {
            LocationFixture.Default().Build(),
            LocationFixture.Default().Build()
        };
        var filter = new LocationFilter { TitleContains = "test" };
        _locationRepositoryMock
            .Setup(x => x.GetAsync(filter))
            .ReturnsAsync(locations);

        // Act
        var result = await _sut.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(locations);
        _locationRepositoryMock.Verify(x => x.GetAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenEmpty()
    {
        // Arrange
        var location = LocationFixture.Default().WithId(Guid.Empty).Build();
        Location? capturedLocation = null;
        _locationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Location>()))
            .Callback<Location>(l => capturedLocation = l)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(location);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        capturedLocation.Should().NotBeNull();
        capturedLocation!.Id.Should().Be(result.Id);
        _locationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Location>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenNotEmpty()
    {
        // Arrange
        var location = LocationFixture.Default().WithId(Guid.NewGuid()).Build();
        Location? capturedLocation = null;
        _locationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Location>()))
            .Callback<Location>(l => capturedLocation = l)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(location);

        // Assert
        result.Id.Should().Be(location.Id);
        capturedLocation.Should().NotBeNull();
        capturedLocation!.Id.Should().Be(location.Id);
        _locationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Location>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowLocationCreateException_WhenRepositoryFails()
    {
        // Arrange
        var location = LocationFixture.Default().Build();
        _locationRepositoryMock
            .Setup(x => x.CreateAsync(location))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(location);

        // Assert
        await act.Should().ThrowAsync<LocationCreateException>()
            .WithMessage("Failed to create location.");
        _locationRepositoryMock.Verify(x => x.CreateAsync(location), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdate_WhenLocationExists()
    {
        // Arrange
        var location = LocationFixture.Default().Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(location.Id))
            .ReturnsAsync(location);
        _locationRepositoryMock
            .Setup(x => x.UpdateAsync(location))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(location);

        // Assert
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(location.Id), Times.Once);
        _locationRepositoryMock.Verify(x => x.UpdateAsync(location), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowLocationNotFoundException_WhenLocationDoesNotExist()
    {
        // Arrange
        var location = LocationFixture.Default().Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(location.Id))
            .ReturnsAsync((Location?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(location);

        // Assert
        await act.Should().ThrowAsync<LocationNotFoundException>()
            .WithMessage($"Location '{location.Id}' was not found.");
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(location.Id), Times.Once);
        _locationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Location>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowLocationUpdateException_WhenRepositoryUpdateFails()
    {
        // Arrange
        var location = LocationFixture.Default().Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(location.Id))
            .ReturnsAsync(location);
        _locationRepositoryMock
            .Setup(x => x.UpdateAsync(location))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(location);

        // Assert
        await act.Should().ThrowAsync<LocationUpdateException>()
            .WithMessage("Failed to update location.");
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(location.Id), Times.Once);
        _locationRepositoryMock.Verify(x => x.UpdateAsync(location), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDelete_WhenLocationExists()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var existingLocation = LocationFixture.Default().WithId(locationId).Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId))
            .ReturnsAsync(existingLocation);
        _locationRepositoryMock
            .Setup(x => x.DeleteAsync(locationId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(locationId);

        // Assert
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(locationId), Times.Once);
        _locationRepositoryMock.Verify(x => x.DeleteAsync(locationId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowLocationNotFoundException_WhenLocationDoesNotExist()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId))
            .ReturnsAsync((Location?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(locationId);

        // Assert
        await act.Should().ThrowAsync<LocationNotFoundException>()
            .WithMessage($"Location '{locationId}' was not found.");
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(locationId), Times.Once);
        _locationRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowLocationDeleteException_WhenRepositoryDeleteFails()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var existingLocation = LocationFixture.Default().WithId(locationId).Build();
        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId))
            .ReturnsAsync(existingLocation);
        _locationRepositoryMock
            .Setup(x => x.DeleteAsync(locationId))
            .ThrowsAsync(new Exception("Delete error"));

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(locationId);

        // Assert
        await act.Should().ThrowAsync<LocationDeleteException>()
            .WithMessage("Failed to delete location.");
        _locationRepositoryMock.Verify(x => x.GetByIdAsync(locationId), Times.Once);
        _locationRepositoryMock.Verify(x => x.DeleteAsync(locationId), Times.Once);
    }
}