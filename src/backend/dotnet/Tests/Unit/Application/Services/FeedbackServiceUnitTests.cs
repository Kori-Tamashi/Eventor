using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Application.Services;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class FeedbackServiceUnitTests
{
    private Mock<IFeedbackRepository> _feedbackRepositoryMock;
    private Mock<IRegistrationRepository> _registrationRepositoryMock;
    private FeedbackService _sut;

    [TestInitialize]
    public void Setup()
    {
        _feedbackRepositoryMock = new Mock<IFeedbackRepository>();
        _registrationRepositoryMock = new Mock<IRegistrationRepository>();
        _sut = new FeedbackService(_feedbackRepositoryMock.Object, _registrationRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnFeedback_WhenExists()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var expectedFeedback = new Feedback { Id = feedbackId, Comment = "Test" };
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedbackId))
            .ReturnsAsync(expectedFeedback);

        // Act
        var result = await _sut.GetByIdAsync(feedbackId);

        // Assert
        result.Should().Be(expectedFeedback);
        _feedbackRepositoryMock.Verify(x => x.GetByIdAsync(feedbackId), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedbackId))
            .ReturnsAsync((Feedback?)null);

        // Act
        var result = await _sut.GetByIdAsync(feedbackId);

        // Assert
        result.Should().BeNull();
        _feedbackRepositoryMock.Verify(x => x.GetByIdAsync(feedbackId), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllFeedbacks_WhenNoFilter()
    {
        // Arrange
        var feedbacks = new List<Feedback>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };
        _feedbackRepositoryMock.Setup(x => x.GetAsync(null))
            .ReturnsAsync(feedbacks);

        // Act
        var result = await _sut.GetAsync();

        // Assert
        result.Should().HaveCount(2);
        _feedbackRepositoryMock.Verify(x => x.GetAsync(null), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyFilter_WhenFilterProvided()
    {
        // Arrange
        var filter = new FeedbackFilter { RegistrationId = Guid.NewGuid() };
        var expectedFeedbacks = new List<Feedback> { new() { Id = Guid.NewGuid() } };
        _feedbackRepositoryMock.Setup(x => x.GetAsync(filter))
            .ReturnsAsync(expectedFeedbacks);

        // Act
        var result = await _sut.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(expectedFeedbacks);
        _feedbackRepositoryMock.Verify(x => x.GetAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task GetByEventIdAsync_ShouldReturnFeedbacksForEvent_WithPagination()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var registration1 = new Registration { Id = Guid.NewGuid() };
        var registration2 = new Registration { Id = Guid.NewGuid() };
        var registrations = new List<Registration> { registration1, registration2 };

        _registrationRepositoryMock.Setup(x => x.GetRegistrationsAsync(
                It.Is<RegistrationFilter>(f => f.EventId == eventId), false))
            .ReturnsAsync(registrations);

        var allFeedbacks = new List<Feedback>
        {
            new() { Id = Guid.NewGuid(), RegistrationId = registration1.Id },
            new() { Id = Guid.NewGuid(), RegistrationId = registration2.Id },
            new() { Id = Guid.NewGuid(), RegistrationId = Guid.NewGuid() } // не принадлежит событию
        };
        _feedbackRepositoryMock.Setup(x => x.GetAsync(null))
            .ReturnsAsync(allFeedbacks);

        // Act
        var result = await _sut.GetByEventIdAsync(eventId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.RegistrationId == registration1.Id || f.RegistrationId == registration2.Id);
    }

    [TestMethod]
    public async Task GetByEventIdAsync_ShouldApplyPagination_WhenFilterProvided()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var registration = new Registration { Id = Guid.NewGuid() };
        _registrationRepositoryMock.Setup(x => x.GetRegistrationsAsync(
                It.IsAny<RegistrationFilter>(), false))
            .ReturnsAsync(new List<Registration> { registration });

        var feedbacks = Enumerable.Range(1, 10).Select(i => new Feedback
        {
            Id = Guid.NewGuid(),
            RegistrationId = registration.Id
        }).ToList();
        _feedbackRepositoryMock.Setup(x => x.GetAsync(null))
            .ReturnsAsync(feedbacks);

        var pagination = new PaginationFilter { PageNumber = 2, PageSize = 3 };

        // Act
        var result = await _sut.GetByEventIdAsync(eventId, pagination);

        // Assert
        result.Should().HaveCount(3); // items 4,5,6
        result.Should().BeEquivalentTo(feedbacks.Skip(3).Take(3));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenEmpty()
    {
        // Arrange
        var feedback = new Feedback { Id = Guid.Empty, Comment = "New" };
        _feedbackRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Feedback>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _sut.CreateAsync(feedback);

        // Assert
        result.Id.Should().NotBeEmpty();
        _feedbackRepositoryMock.Verify(x => x.CreateAsync(It.Is<Feedback>(f => f.Id != Guid.Empty && f.Comment == "New")), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPreserveId_WhenNotEmpty()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var feedback = new Feedback { Id = existingId, Comment = "Existing" };
        _feedbackRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Feedback>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(feedback);

        // Assert
        result.Id.Should().Be(existingId);
        _feedbackRepositoryMock.Verify(x => x.CreateAsync(feedback), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowFeedbackCreateException_WhenRepositoryFails()
    {
        // Arrange
        var feedback = new Feedback { Id = Guid.NewGuid() };
        _feedbackRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Feedback>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(feedback);

        // Assert
        await act.Should().ThrowAsync<FeedbackCreateException>()
            .WithMessage("Failed to create feedback.");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdate_WhenFeedbackExists()
    {
        // Arrange
        var feedback = new Feedback { Id = Guid.NewGuid(), Comment = "Updated" };
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedback.Id))
            .ReturnsAsync(new Feedback());
        _feedbackRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Feedback>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(feedback);

        // Assert
        _feedbackRepositoryMock.Verify(x => x.UpdateAsync(feedback), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowFeedbackNotFoundException_WhenFeedbackNotExists()
    {
        // Arrange
        var feedback = new Feedback { Id = Guid.NewGuid() };
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedback.Id))
            .ReturnsAsync((Feedback?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(feedback);

        // Assert
        await act.Should().ThrowAsync<FeedbackNotFoundException>()
            .WithMessage($"Feedback '{feedback.Id}' was not found.");
        _feedbackRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Feedback>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowFeedbackUpdateException_WhenRepositoryFails()
    {
        // Arrange
        var feedback = new Feedback { Id = Guid.NewGuid() };
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedback.Id))
            .ReturnsAsync(new Feedback());
        _feedbackRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Feedback>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(feedback);

        // Assert
        await act.Should().ThrowAsync<FeedbackUpdateException>()
            .WithMessage("Failed to update feedback.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDelete_WhenFeedbackExists()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedbackId))
            .ReturnsAsync(new Feedback());
        _feedbackRepositoryMock.Setup(x => x.DeleteAsync(feedbackId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(feedbackId);

        // Assert
        _feedbackRepositoryMock.Verify(x => x.DeleteAsync(feedbackId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowFeedbackNotFoundException_WhenFeedbackNotExists()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedbackId))
            .ReturnsAsync((Feedback?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(feedbackId);

        // Assert
        await act.Should().ThrowAsync<FeedbackNotFoundException>()
            .WithMessage($"Feedback '{feedbackId}' was not found.");
        _feedbackRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowFeedbackDeleteException_WhenRepositoryFails()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        _feedbackRepositoryMock.Setup(x => x.GetByIdAsync(feedbackId))
            .ReturnsAsync(new Feedback());
        _feedbackRepositoryMock.Setup(x => x.DeleteAsync(feedbackId))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(feedbackId);

        // Assert
        await act.Should().ThrowAsync<FeedbackDeleteException>()
            .WithMessage("Failed to delete feedback.");
    }
}