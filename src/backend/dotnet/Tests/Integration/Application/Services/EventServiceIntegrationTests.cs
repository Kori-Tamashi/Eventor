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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class EventServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private EventService _sutService = null!;
    private UserRepository _userRepository = null!;
    private RegistrationRepository _registrationRepository = null!;
    private DayRepository _dayRepository = null!;
    private MenuService _menuService = null!;
    private LocationService _locationService = null!;

    [TestInitialize]
    public void Setup()
    {
        var eventLogger = NullLogger<EventRepository>.Instance;
        var eventRepository = new EventRepository(DbContext!, eventLogger);

        var registrationLogger = NullLogger<RegistrationRepository>.Instance;
        _registrationRepository = new RegistrationRepository(DbContext!, registrationLogger);

        var dayLogger = NullLogger<DayRepository>.Instance;
        _dayRepository = new DayRepository(DbContext!, dayLogger);

        _sutService = new EventService(eventRepository, _registrationRepository, _dayRepository);

        var userLogger = NullLogger<UserRepository>.Instance;
        _userRepository = new UserRepository(DbContext!, userLogger);

        var menuRepository = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        var menuItemRepository = new MenuItemRepository(DbContext!, NullLogger<MenuItemRepository>.Instance);
        _menuService = new MenuService(menuRepository, menuItemRepository);

        var locationRepo = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        _locationService = new LocationService(locationRepo);
    }

    #region Helper Methods

    private async Task<Location> CreateLocationAsync(decimal cost = 1000m, int capacity = 100)
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithDescription("Location Desc")
            .WithCost(cost)
            .WithCapacity(capacity)
            .Build();
        return await _locationService.CreateAsync(location);
    }

    private async Task<Menu> CreateMenuAsync(string title = "Test Menu")
    {
        var menu = MenuFixture.Default()
            .WithTitle(title)
            .WithDescription("Menu Description")
            .Build();
        return await _menuService.CreateAsync(menu);
    }

    /// <summary>
    /// Создаёт событие с уже существующей локацией (объект Location, не только Id).
    /// </summary>
    private async Task<Event> CreateValidEventAsync(Location location)
    {
        var ev = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithTitle("Test Event")
            .WithDescription("Event Description")
            .WithStartDate(DateOnly.FromDateTime(DateTime.Today))
            .WithDaysCount(2)
            .WithPercent(10)
            .Build();
        return await _sutService.CreateAsync(ev);
    }

    private async Task<User> CreateUserAsync(string phone)
    {
        var user = UserFixture.Default()
            .WithName($"User {phone}")
            .WithPhone(phone)
            .WithGender(Gender.Male)
            .WithRole(UserRole.User)
            .WithPasswordHash("hash")
            .Build();
        await _userRepository.CreateAsync(user);
        return user;
    }

    private async Task<Registration> CreateRegistrationAsync(Guid eventId, Guid userId, RegistrationType type, bool payment)
    {
        var regRepo = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        var regDayRepo = new RegistrationDayRepository(DbContext!, NullLogger<RegistrationDayRepository>.Instance);
        var regService = new RegistrationService(regRepo, regDayRepo);
        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(type)
            .WithPayment(payment)
            .Build();
        return await regService.CreateAsync(registration, Array.Empty<Guid>());
    }

    private async Task<Day> CreateDayWithMenuAsync(Guid eventId, int sequenceNumber)
    {
        var menu = await CreateMenuAsync($"Menu for day {sequenceNumber}");
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menu.Id)
            .WithTitle($"Day {sequenceNumber}")
            .WithSequenceNumber(sequenceNumber)
            .WithDescription("Day description")
            .Build();
        await _dayRepository.CreateAsync(day);
        return day;
    }

    /// <summary>
    /// Очищает кэш DbContext, чтобы избежать возврата устаревших данных.
    /// </summary>
    private void ClearCache()
    {
        DbContext!.ChangeTracker.Clear();
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldPersistEvent_AndGenerateId_WhenIdIsEmpty()
    {
        var location = await CreateLocationAsync();
        var ev = new Event
        {
            Id = Guid.Empty,
            LocationId = location.Id,
            Title = "New Event",
            Description = "Desc",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            DaysCount = 3,
            Percent = 5
        };

        var result = await _sutService.CreateAsync(ev);

        result.Id.Should().NotBeEmpty();
        ClearCache();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("New Event");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        var location = await CreateLocationAsync();
        var fixedId = Guid.NewGuid();
        var ev = new Event
        {
            Id = fixedId,
            LocationId = location.Id,
            Title = "Fixed Id Event",
            Description = "Desc",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            DaysCount = 1,
            Percent = 0
        };

        var result = await _sutService.CreateAsync(ev);

        result.Id.Should().Be(fixedId);
        ClearCache();
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowEventCreateException_WhenDuplicateId()
    {
        var location = await CreateLocationAsync();
        var existing = await CreateValidEventAsync(location);
        var duplicate = new Event
        {
            Id = existing.Id,
            LocationId = location.Id,
            Title = "Duplicate",
            Description = "Desc",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            DaysCount = 1,
            Percent = 0
        };

        Func<Task> act = async () => await _sutService.CreateAsync(duplicate);
        await act.Should().ThrowAsync<EventCreateException>();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEvent_WhenExists()
    {
        var location = await CreateLocationAsync();
        var ev = await CreateValidEventAsync(location);

        var result = await _sutService.GetByIdAsync(ev.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ev.Id);
        result.Title.Should().Be(ev.Title);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutService.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllEvents_OrderedByStartDateThenId()
    {
        var location = await CreateLocationAsync();
        var ev1 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithStartDate(new DateOnly(2025, 2, 1))
            .WithTitle("Second")
            .Build();
        var ev2 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithStartDate(new DateOnly(2025, 1, 1))
            .WithTitle("First")
            .Build();

        await _sutService.CreateAsync(ev1);
        await _sutService.CreateAsync(ev2);
        ClearCache();

        var result = await _sutService.GetAsync();

        result.Should().HaveCount(2);
        result[0].StartDate.Should().BeBefore(result[1].StartDate);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByLocationId()
    {
        var location1 = await CreateLocationAsync();
        var location2 = await CreateLocationAsync();
        var ev1 = await CreateValidEventAsync(location1);
        var ev2 = await CreateValidEventAsync(location2);
        var filter = new EventFilter { LocationId = location1.Id };

        ClearCache();
        var result = await _sutService.GetAsync(filter);

        result.Should().HaveCount(1);
        result[0].LocationId.Should().Be(location1.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByDateRange()
    {
        var location = await CreateLocationAsync();
        var ev1 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithStartDate(new DateOnly(2025, 1, 10))
            .Build();
        var ev2 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithStartDate(new DateOnly(2025, 2, 20))
            .Build();

        await _sutService.CreateAsync(ev1);
        await _sutService.CreateAsync(ev2);
        ClearCache();

        var filter = new EventFilter
        {
            StartDateFrom = new DateOnly(2025, 2, 1),
            StartDateTo = new DateOnly(2025, 2, 28)
        };

        var result = await _sutService.GetAsync(filter);
        result.Should().HaveCount(1);
        result[0].StartDate.Should().Be(new DateOnly(2025, 2, 20));
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitleContains()
    {
        var location = await CreateLocationAsync();
        var ev1 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithTitle("Summer Festival")
            .Build();
        var ev2 = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithTitle("Winter Meetup")
            .Build();

        await _sutService.CreateAsync(ev1);
        await _sutService.CreateAsync(ev2);
        ClearCache();

        var filter = new EventFilter { TitleContains = "summer" };
        var result = await _sutService.GetAsync(filter);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Summer Festival");
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        var location = await CreateLocationAsync();
        for (int i = 1; i <= 5; i++)
        {
            var ev = EventFixture.Default()
                .WithLocationId(location.Id)
                .WithTitle($"Event {i}")
                .WithStartDate(new DateOnly(2025, 1, i))
                .Build();
            await _sutService.CreateAsync(ev);
        }
        ClearCache();

        var filter = new EventFilter { PageNumber = 2, PageSize = 2 };
        var result = await _sutService.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingEvent()
    {
        var location = await CreateLocationAsync();
        var ev = await CreateValidEventAsync(location);

        var updated = new Event
        {
            Id = ev.Id,
            LocationId = ev.LocationId,
            Title = "Updated Title",
            Description = "Updated Desc",
            StartDate = new DateOnly(2026, 1, 1),
            DaysCount = 5,
            Percent = 20
        };

        await _sutService.UpdateAsync(updated);
        ClearCache();

        var result = await _sutService.GetByIdAsync(ev.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.StartDate.Should().Be(new DateOnly(2026, 1, 1));
        result.DaysCount.Should().Be(5);
        result.Percent.Should().Be(20);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowEventNotFoundException_WhenEventDoesNotExist()
    {
        var ev = EventFixture.Default().Build();
        Func<Task> act = async () => await _sutService.UpdateAsync(ev);
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{ev.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingEvent()
    {
        // Arrange
        var location = await CreateLocationAsync();
        var ev = await CreateValidEventAsync(location);

        // Act
        await _sutService.DeleteAsync(ev.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(ev.Id);
        result.Should().BeNull();

        var direct = await DbContext!.Events.FindAsync(ev.Id);
        direct.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowEventNotFoundException_WhenEventDoesNotExist()
    {
        var id = Guid.NewGuid();
        Func<Task> act = async () => await _sutService.DeleteAsync(id);
        await act.Should().ThrowAsync<EventNotFoundException>()
            .WithMessage($"Event '{id}' was not found.");
    }

    [TestMethod]
    public async Task GetByParticipantUserIdAsync_ShouldReturnEventsWhereUserHasRegistration()
    {
        var location = await CreateLocationAsync();
        var event1 = await CreateValidEventAsync(location);
        var event2 = await CreateValidEventAsync(location);
        var user = await CreateUserAsync("+111111111");

        await CreateRegistrationAsync(event1.Id, user.Id, RegistrationType.Standard, true);
        // user не зарегистрирован на event2

        ClearCache();
        var result = await _sutService.GetByParticipantUserIdAsync(user.Id);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(event1.Id);
    }

    [TestMethod]
    public async Task GetByParticipantUserIdAsync_ShouldApplyPagination()
    {
        var location = await CreateLocationAsync();
        var events = new[]
        {
            await CreateValidEventAsync(location),
            await CreateValidEventAsync(location),
            await CreateValidEventAsync(location)
        };
        var user = await CreateUserAsync("+222222222");
        foreach (var ev in events)
        {
            await CreateRegistrationAsync(ev.Id, user.Id, RegistrationType.Standard, true);
        }
        ClearCache();

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 1 };
        var result = await _sutService.GetByParticipantUserIdAsync(user.Id, filter);

        result.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task GetByOrganizerUserIdAsync_ShouldReturnEventsWhereUserIsOrganizer()
    {
        var location = await CreateLocationAsync();
        var event1 = await CreateValidEventAsync(location);
        var event2 = await CreateValidEventAsync(location);
        var organizer = await CreateUserAsync("+333333333");

        await CreateRegistrationAsync(event1.Id, organizer.Id, RegistrationType.Organizer, true);
        // user не организатор на event2

        ClearCache();
        var result = await _sutService.GetByOrganizerUserIdAsync(organizer.Id);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(event1.Id);
    }

    [TestMethod]
    public async Task GetDaysAsync_ShouldReturnDaysOfEvent()
    {
        var location = await CreateLocationAsync();
        var ev = await CreateValidEventAsync(location);

        await CreateDayWithMenuAsync(ev.Id, 1);
        await CreateDayWithMenuAsync(ev.Id, 2);
        ClearCache();

        var days = await _sutService.GetDaysAsync(ev.Id);
        days.Should().HaveCount(2);
        days.Select(d => d.SequenceNumber).Should().Contain([1, 2]);
    }

    [TestMethod]
    public async Task GetDaysAsync_ShouldThrowEventNotFoundException_WhenEventNotFound()
    {
        Func<Task> act = async () => await _sutService.GetDaysAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldAddDayToEvent()
    {
        var location = await CreateLocationAsync();
        var ev = await CreateValidEventAsync(location);
        var menu = await CreateMenuAsync();
        var day = new Day
        {
            Id = Guid.Empty,
            MenuId = menu.Id,
            Title = "New Day",
            Description = "Day Desc",
            SequenceNumber = 1
        };

        var result = await _sutService.AddDayAsync(ev.Id, day);
        result.Id.Should().NotBeEmpty();
        result.EventId.Should().Be(ev.Id);

        ClearCache();
        var savedDay = await _dayRepository.GetByIdAsync(result.Id);
        savedDay.Should().NotBeNull();
        savedDay!.Title.Should().Be("New Day");
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldThrowEventNotFoundException_WhenEventNotFound()
    {
        var menu = await CreateMenuAsync();
        var day = DayFixture.Default().WithMenuId(menu.Id).Build();
        Func<Task> act = async () => await _sutService.AddDayAsync(Guid.NewGuid(), day);
        await act.Should().ThrowAsync<EventNotFoundException>();
    }
}