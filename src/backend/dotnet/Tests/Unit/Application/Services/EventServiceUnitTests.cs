using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Domain.Enums;
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
public class EventServiceTests
{
    private Mock<IEventRepository> _eventRepositoryMock;
    private Mock<IRegistrationRepository> _registrationRepositoryMock;
    private Mock<IDayRepository> _dayRepositoryMock;
    private EventService _eventService;

    [TestInitialize]
    public void Setup()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _registrationRepositoryMock = new Mock<IRegistrationRepository>();
        _dayRepositoryMock = new Mock<IDayRepository>();
        _eventService = new EventService(
            _eventRepositoryMock.Object,
            _registrationRepositoryMock.Object,
            _dayRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEvent_WhenExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var expectedEvent = EventFixture.Default().WithId(eventId).Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(expectedEvent);

        // Act
        var result = await _eventService.GetByIdAsync(eventId);

        // Assert
        result.Should().Be(expectedEvent);
        _eventRepositoryMock.Verify(x => x.GetByIdAsync(eventId), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var result = await _eventService.GetByIdAsync(eventId);

        // Assert
        result.Should().BeNull();
        _eventRepositoryMock.Verify(x => x.GetByIdAsync(eventId), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnEvents_WithFilter()
    {
        // Arrange
        var filter = new EventFilter { TitleContains = "test" };
        var expectedEvents = new List<Event>
        {
            EventFixture.Default().WithTitle("test 1").Build(),
            EventFixture.Default().WithTitle("test 2").Build()
        };
        _eventRepositoryMock.Setup(x => x.GetAsync(filter))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _eventService.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(expectedEvents);
        _eventRepositoryMock.Verify(x => x.GetAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSetIdAndCallRepository_WhenIdIsEmpty()
    {
        // Arrange
        var eventWithoutId = EventFixture.Default().WithId(Guid.Empty).Build();
        _eventRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.CreateAsync(eventWithoutId);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        _eventRepositoryMock.Verify(x => x.CreateAsync(It.Is<Event>(e => e.Id == result.Id)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepIdAndCallRepository_WhenIdIsNotEmpty()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var eventWithId = EventFixture.Default().WithId(existingId).Build();
        _eventRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.CreateAsync(eventWithId);

        // Assert
        result.Id.Should().Be(existingId);
        _eventRepositoryMock.Verify(x => x.CreateAsync(eventWithId), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowEventCreateException_WhenRepositoryThrows()
    {
        // Arrange
        var eventEntity = EventFixture.Default().Build();
        _eventRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Event>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _eventService.CreateAsync(eventEntity);

        // Assert
        await act.Should().ThrowAsync<EventCreateException>()
            .WithMessage("Failed to create event.");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdate_WhenEventExists()
    {
        // Arrange
        var eventEntity = EventFixture.Default().Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventEntity.Id))
            .ReturnsAsync(eventEntity);
        _eventRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventService.UpdateAsync(eventEntity);

        // Assert
        _eventRepositoryMock.Verify(x => x.UpdateAsync(eventEntity), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowEventNotFoundException_WhenEventNotExists()
    {
        // Arrange
        var eventEntity = EventFixture.Default().Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventEntity.Id))
            .ReturnsAsync((Event?)null);

        // Act
        Func<Task> act = async () => await _eventService.UpdateAsync(eventEntity);

        // Assert
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{eventEntity.Id}' was not found.");
        _eventRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowEventUpdateException_WhenRepositoryThrows()
    {
        // Arrange
        var eventEntity = EventFixture.Default().Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventEntity.Id))
            .ReturnsAsync(eventEntity);
        _eventRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _eventService.UpdateAsync(eventEntity);

        // Assert
        await act.Should().ThrowAsync<EventUpdateException>()
            .WithMessage("Failed to update event.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDelete_WhenEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = EventFixture.Default().WithId(eventId).Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);
        _eventRepositoryMock.Setup(x => x.DeleteAsync(eventId))
            .Returns(Task.CompletedTask);

        // Act
        await _eventService.DeleteAsync(eventId);

        // Assert
        _eventRepositoryMock.Verify(x => x.DeleteAsync(eventId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowEventNotFoundException_WhenEventNotExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        Func<Task> act = async () => await _eventService.DeleteAsync(eventId);

        // Assert
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{eventId}' was not found.");
        _eventRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowEventDeleteException_WhenRepositoryThrows()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = EventFixture.Default().WithId(eventId).Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);
        _eventRepositoryMock.Setup(x => x.DeleteAsync(eventId))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = async () => await _eventService.DeleteAsync(eventId);

        // Assert
        await act.Should().ThrowAsync<EventDeleteException>()
            .WithMessage("Failed to delete event.");
    }

    [TestMethod]
    public async Task GetByParticipantUserIdAsync_ShouldReturnEvents_FromUserRegistrations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        var registrations = new List<Registration>
        {
            RegistrationFixture.Default().WithEventId(eventId1).WithUserId(userId).Build(),
            RegistrationFixture.Default().WithEventId(eventId2).WithUserId(userId).Build()
        };

        var event1 = EventFixture.Default().WithId(eventId1).Build();
        var event2 = EventFixture.Default().WithId(eventId2).Build();

        _registrationRepositoryMock.Setup(x => x.GetRegistrationsAsync(
                It.Is<RegistrationFilter>(f => f.UserId == userId), false))
            .ReturnsAsync(registrations);

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId1))
            .ReturnsAsync(event1);
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId2))
            .ReturnsAsync(event2);

        // Act
        var result = await _eventService.GetByParticipantUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(event1);
        result.Should().Contain(event2);
    }

    [TestMethod]
    public async Task GetByParticipantUserIdAsync_ShouldApplyPagination()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var registrations = new List<Registration>();
        var events = new List<Event>();

        for (int i = 1; i <= 5; i++)
        {
            var eventId = Guid.NewGuid();
            registrations.Add(RegistrationFixture.Default().WithEventId(eventId).WithUserId(userId).Build());
            events.Add(EventFixture.Default().WithId(eventId).Build());
        }

        _registrationRepositoryMock.Setup(x => x.GetRegistrationsAsync(
                It.Is<RegistrationFilter>(f => f.UserId == userId), false))
            .ReturnsAsync(registrations);

        foreach (var ev in events)
        {
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(ev.Id))
                .ReturnsAsync(ev);
        }

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _eventService.GetByParticipantUserIdAsync(userId, filter);

        // Assert
        result.Should().HaveCount(2);
        // Events are in original order (by registration order), pagination applied after retrieval
    }

    [TestMethod]
    public async Task GetByOrganizerUserIdAsync_ShouldReturnEvents_WhereUserIsOrganizer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        var registrations = new List<Registration>
        {
            RegistrationFixture.Default()
                .WithEventId(eventId1)
                .WithUserId(userId)
                .WithType(RegistrationType.Organizer)
                .Build(),
            RegistrationFixture.Default()
                .WithEventId(eventId2)
                .WithUserId(userId)
                .WithType(RegistrationType.Organizer)
                .Build()
        };

        var event1 = EventFixture.Default().WithId(eventId1).Build();
        var event2 = EventFixture.Default().WithId(eventId2).Build();

        _registrationRepositoryMock.Setup(x => x.GetRegistrationsAsync(
                It.Is<RegistrationFilter>(f => f.UserId == userId && f.Type == RegistrationType.Organizer), false))
            .ReturnsAsync(registrations);

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId1))
            .ReturnsAsync(event1);
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId2))
            .ReturnsAsync(event2);

        // Act
        var result = await _eventService.GetByOrganizerUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(event1);
        result.Should().Contain(event2);
    }

    [TestMethod]
    public async Task GetDaysAsync_ShouldReturnDays_WhenEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = EventFixture.Default().WithId(eventId).Build();
        var days = new List<Day>
        {
            DayFixture.Default().WithEventId(eventId).WithSequenceNumber(1).Build(),
            DayFixture.Default().WithEventId(eventId).WithSequenceNumber(2).Build()
        };

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);
        _dayRepositoryMock.Setup(x => x.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);

        // Act
        var result = await _eventService.GetDaysAsync(eventId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(days);
    }

    [TestMethod]
    public async Task GetDaysAsync_ShouldThrowEventNotFoundException_WhenEventNotExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        Func<Task> act = async () => await _eventService.GetDaysAsync(eventId);

        // Assert
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{eventId}' was not found.");
        _dayRepositoryMock.Verify(x => x.GetAsync(It.IsAny<DayFilter>()), Times.Never);
    }

    [TestMethod]
    public async Task GetDaysAsync_ShouldApplyPagination()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = EventFixture.Default().WithId(eventId).Build();
        var days = new List<Day>();
        for (int i = 1; i <= 5; i++)
        {
            days.Add(DayFixture.Default().WithEventId(eventId).WithSequenceNumber(i).Build());
        }

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);
        _dayRepositoryMock.Setup(x => x.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _eventService.GetDaysAsync(eventId, filter);

        // Assert
        result.Should().HaveCount(2);
        // Should return days 3 and 4 (zero-based index 2 and 3)
        result.Should().Contain(days[2]);
        result.Should().Contain(days[3]);
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldCreateDayAndSetEventId_WhenEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = EventFixture.Default().WithId(eventId).Build();
        var dayToAdd = DayFixture.Default().WithId(Guid.Empty).Build();

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);
        _dayRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Day>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.AddDayAsync(eventId, dayToAdd);

        // Assert
        result.EventId.Should().Be(eventId);
        result.Id.Should().NotBe(Guid.Empty);
        _dayRepositoryMock.Verify(x => x.CreateAsync(It.Is<Day>(d => d.EventId == eventId && d.Id != Guid.Empty)), Times.Once);
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldThrowEventNotFoundException_WhenEventNotExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dayToAdd = DayFixture.Default().Build();
        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        Func<Task> act = async () => await _eventService.AddDayAsync(eventId, dayToAdd);

        // Assert
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{eventId}' was not found.");
        _dayRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Day>()), Times.Never);
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldPreserveExistingId_WhenDayIdIsNotEmpty()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingDayId = Guid.NewGuid();
        var dayToAdd = DayFixture.Default().WithId(existingDayId).Build();
        var eventEntity = EventFixture.Default().WithId(eventId).Build();

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);
        _dayRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Day>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.AddDayAsync(eventId, dayToAdd);

        // Assert
        result.Id.Should().Be(existingDayId);
        result.EventId.Should().Be(eventId);
        _dayRepositoryMock.Verify(x => x.CreateAsync(It.Is<Day>(d => d.Id == existingDayId && d.EventId == eventId)), Times.Once);
    }
}