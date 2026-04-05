using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Repositories;
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
public class DayServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private DayService _sutService = null!;

    [TestInitialize]
    public void Setup()
    {
        var dayRepository = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        _sutService = new DayService(dayRepository);
    }

    #region вспомогательные методы создания связанных сущностей

    private async Task<Guid> CreateLocationAsync()
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithCost(1000m)
            .Build();
        var locationRepo = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        await locationRepo.CreateAsync(location);
        return location.Id;
    }

    private async Task<Guid> CreateEventAsync(Guid locationId)
    {
        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Test Event")
            .Build();
        var eventRepo = new EventRepository(DbContext!, NullLogger<EventRepository>.Instance);
        await eventRepo.CreateAsync(ev);
        return ev.Id;
    }

    private async Task<Guid> CreateMenuAsync()
    {
        var menu = MenuFixture.Default()
            .WithTitle("Test Menu")
            .Build();
        var menuRepo = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        await menuRepo.CreateAsync(menu);
        return menu.Id;
    }

    private async Task<Day> CreateValidDayAsync()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("Integration Day")
            .WithSequenceNumber(1)
            .Build();
        return day;
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldPersistDay_AndGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = new Day
        {
            Id = Guid.Empty,
            EventId = eventId,
            MenuId = menuId,
            Title = "New Day",
            SequenceNumber = 2,
            Description = "Desc"
        };

        // Act
        var result = await _sutService.CreateAsync(day);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("New Day");
        saved.SequenceNumber.Should().Be(2);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();
        var fixedId = Guid.NewGuid();

        var day = new Day
        {
            Id = fixedId,
            EventId = eventId,
            MenuId = menuId,
            Title = "Fixed Id Day",
            SequenceNumber = 3,
            Description = "Some description"
        };

        // Act
        var result = await _sutService.CreateAsync(day);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Fixed Id Day");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowDayCreateException_WhenDuplicateId()
    {
        // Arrange
        var existingDay = await CreateValidDayAsync();
        await _sutService.CreateAsync(existingDay);

        var duplicate = new Day
        {
            Id = existingDay.Id,
            EventId = existingDay.EventId,
            MenuId = existingDay.MenuId,
            Title = "Duplicate",
            SequenceNumber = 99,
            Description = "Some description"
        };

        // Act
        Func<Task> act = async () => await _sutService.CreateAsync(duplicate);

        // Assert
        await act.Should().ThrowAsync<DayCreateException>();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnDay_WhenExists()
    {
        // Arrange
        var day = await CreateValidDayAsync();
        await _sutService.CreateAsync(day);

        // Act
        var result = await _sutService.GetByIdAsync(day.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(day.Id);
        result.Title.Should().Be(day.Title);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _sutService.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllDays_WhenNoFilter()
    {
        // Arrange
        var day1 = await CreateValidDayAsync();
        var day2 = await CreateValidDayAsync();
        await _sutService.CreateAsync(day1);
        await _sutService.CreateAsync(day2);

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(d => d.Id).Should().Contain([day1.Id, day2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByEventId()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var event1 = await CreateEventAsync(locationId);
        var event2 = await CreateEventAsync(locationId);
        var menuId1 = await CreateMenuAsync(); // первое меню
        var menuId2 = await CreateMenuAsync(); // второе меню

        var day1 = DayFixture.Default()
            .WithEventId(event1)
            .WithMenuId(menuId1)
            .Build();
        var day2 = DayFixture.Default()
            .WithEventId(event2)
            .WithMenuId(menuId2)
            .Build();

        await _sutService.CreateAsync(day1);
        await _sutService.CreateAsync(day2);

        var filter = new DayFilter { EventId = event1 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.All(d => d.EventId == event1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByMenuId()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menu1 = await CreateMenuAsync();
        var menu2 = await CreateMenuAsync();

        var day1 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menu1)
            .Build();
        var day2 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menu2)
            .Build();

        await _sutService.CreateAsync(day1);
        await _sutService.CreateAsync(day2);

        var filter = new DayFilter { MenuId = menu1 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.All(d => d.MenuId == menu1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);

        for (int i = 1; i <= 5; i++)
        {
            var menuId = await CreateMenuAsync();
            var day = DayFixture.Default()
                .WithEventId(eventId)
                .WithMenuId(menuId)
                .WithSequenceNumber(i)
                .Build();
            await _sutService.CreateAsync(day);
        }

        var filter = new DayFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingDay()
    {
        // Arrange
        var day = await CreateValidDayAsync();
        await _sutService.CreateAsync(day);

        var updated = new Day
        {
            Id = day.Id,
            EventId = day.EventId,
            MenuId = day.MenuId,
            Title = "Updated Title",
            Description = "Updated Desc",
            SequenceNumber = 99
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(day.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.SequenceNumber.Should().Be(99);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowDayNotFoundException_WhenDayDoesNotExist()
    {
        // Arrange
        var day = await CreateValidDayAsync(); // не сохраняем

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(day);

        // Assert
        await act.Should().ThrowAsync<DayNotFoundException>()
            .WithMessage($"Day '{day.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingDay()
    {
        // Arrange
        var day = await CreateValidDayAsync();
        await _sutService.CreateAsync(day);

        // Act
        await _sutService.DeleteAsync(day.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(day.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowDayNotFoundException_WhenDayDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<DayNotFoundException>()
            .WithMessage($"Day '{id}' was not found.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSetGeneratedId_WhenIdEmpty()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = new Day
        {
            Id = Guid.Empty,
            EventId = eventId,
            MenuId = menuId,
            Title = "Auto Id Day",
            SequenceNumber = 5,
            Description = "Some description"
        };

        // Act
        var result = await _sutService.CreateAsync(day);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Auto Id Day");
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnEmpty_WhenNoDaysMatchFilter()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();
        await _sutService.CreateAsync(day);

        var filter = new DayFilter { EventId = Guid.NewGuid() };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnDaysOrderedById()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day1 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithSequenceNumber(2)
            .Build();
        var day2 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithSequenceNumber(1)
            .Build();

        await _sutService.CreateAsync(day1);
        await _sutService.CreateAsync(day2);

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().BeInAscendingOrder(d => d.Id);
    }
}