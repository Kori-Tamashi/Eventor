using System;
using System.Collections.Generic;
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
public class DayServiceUnitTests
{
    private Mock<IDayRepository> _dayRepositoryMock = null!;
    private DayService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _dayRepositoryMock = new Mock<IDayRepository>();
        _sut = new DayService(_dayRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnDay_WhenExists()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var expectedDay = new Day { Id = dayId, Title = "Test Day" };
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync(expectedDay);

        // Act
        var result = await _sut.GetByIdAsync(dayId);

        // Assert
        result.Should().BeEquivalentTo(expectedDay);
        _dayRepositoryMock.Verify(x => x.GetByIdAsync(dayId), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync((Day?)null);

        // Act
        var result = await _sut.GetByIdAsync(dayId);

        // Assert
        result.Should().BeNull();
        _dayRepositoryMock.Verify(x => x.GetByIdAsync(dayId), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnListOfDays_WhenNoFilter()
    {
        // Arrange
        var days = new List<Day>
        {
            new() { Id = Guid.NewGuid(), Title = "Day1" },
            new() { Id = Guid.NewGuid(), Title = "Day2" }
        };
        _dayRepositoryMock.Setup(x => x.GetAsync(null))
            .ReturnsAsync(days);

        // Act
        var result = await _sut.GetAsync();

        // Assert
        result.Should().BeEquivalentTo(days);
        _dayRepositoryMock.Verify(x => x.GetAsync(null), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredDays_WhenFilterProvided()
    {
        // Arrange
        var filter = new DayFilter { EventId = Guid.NewGuid() };
        var days = new List<Day> { new() { Id = Guid.NewGuid(), EventId = filter.EventId.Value } };
        _dayRepositoryMock.Setup(x => x.GetAsync(filter))
            .ReturnsAsync(days);

        // Act
        var result = await _sut.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(days);
        _dayRepositoryMock.Verify(x => x.GetAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var day = new Day { Id = Guid.Empty, Title = "New Day" };

        // Act
        var result = await _sut.CreateAsync(day);

        // Assert
        result.Id.Should().NotBeEmpty();
        _dayRepositoryMock.Verify(x => x.CreateAsync(It.Is<Day>(d => d.Id == result.Id)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldNotGenerateId_WhenIdIsProvided()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, Title = "Existing Day" };

        // Act
        var result = await _sut.CreateAsync(day);

        // Assert
        result.Id.Should().Be(dayId);
        _dayRepositoryMock.Verify(x => x.CreateAsync(It.Is<Day>(d => d.Id == dayId)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowDayCreateException_WhenRepositoryThrows()
    {
        // Arrange
        var day = new Day { Id = Guid.Empty, Title = "Failing Day" };
        _dayRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Day>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(day);

        // Assert
        await act.Should().ThrowAsync<DayCreateException>()
            .WithMessage("Failed to create day.*");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldCallRepository_WhenDayExists()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var existingDay = new Day { Id = dayId, Title = "Old" };
        var updatedDay = new Day { Id = dayId, Title = "New" };

        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync(existingDay);
        _dayRepositoryMock.Setup(x => x.UpdateAsync(updatedDay))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(updatedDay);

        // Assert
        _dayRepositoryMock.Verify(x => x.UpdateAsync(updatedDay), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowDayNotFoundException_WhenDayNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, Title = "Ghost" };
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync((Day?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(day);

        // Assert
        await act.Should().ThrowAsync<DayNotFoundException>()
            .WithMessage($"Day '{dayId}' was not found.");
        _dayRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Day>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowDayUpdateException_WhenRepositoryThrows()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, Title = "Failing" };
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync(day);
        _dayRepositoryMock.Setup(x => x.UpdateAsync(day))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(day);

        // Assert
        await act.Should().ThrowAsync<DayUpdateException>()
            .WithMessage("Failed to update day.*");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldCallRepository_WhenDayExists()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId };
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync(day);
        _dayRepositoryMock.Setup(x => x.DeleteAsync(dayId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(dayId);

        // Assert
        _dayRepositoryMock.Verify(x => x.DeleteAsync(dayId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowDayNotFoundException_WhenDayNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync((Day?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(dayId);

        // Assert
        await act.Should().ThrowAsync<DayNotFoundException>()
            .WithMessage($"Day '{dayId}' was not found.");
        _dayRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowDayDeleteException_WhenRepositoryThrows()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId };
        _dayRepositoryMock.Setup(x => x.GetByIdAsync(dayId))
            .ReturnsAsync(day);
        _dayRepositoryMock.Setup(x => x.DeleteAsync(dayId))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(dayId);

        // Assert
        await act.Should().ThrowAsync<DayDeleteException>()
            .WithMessage("Failed to delete day.*");
    }
}