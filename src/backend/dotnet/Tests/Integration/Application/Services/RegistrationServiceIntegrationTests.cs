using System;
using System.Collections.Generic;
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
public class RegistrationServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private RegistrationService _sutService = null!;
    private UserRepository _userRepository = null!;
    private EventRepository _eventRepository = null!;
    private DayRepository _dayRepository = null!;
    private LocationRepository _locationRepository = null!;
    private MenuRepository _menuRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<RegistrationRepository>.Instance;
        var registrationRepository = new RegistrationRepository(DbContext!, logger);
        var registrationDayRepository = new RegistrationDayRepository(DbContext!, NullLogger<RegistrationDayRepository>.Instance);
        _sutService = new RegistrationService(registrationRepository, registrationDayRepository);

        _userRepository = new UserRepository(DbContext!, NullLogger<UserRepository>.Instance);
        _eventRepository = new EventRepository(DbContext!, NullLogger<EventRepository>.Instance);
        _dayRepository = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        _locationRepository = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        _menuRepository = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
    }

    #region Helper Methods

    private async Task<Location> CreateLocationAsync()
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithCost(1000m)
            .Build();
        await _locationRepository.CreateAsync(location);
        return location;
    }

    private async Task<Event> CreateEventAsync(Guid locationId)
    {
        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Test Event")
            .WithDaysCount(2)
            .Build();
        await _eventRepository.CreateAsync(ev);
        return ev;
    }

    private async Task<Menu> CreateMenuAsync()
    {
        var menu = MenuFixture.Default()
            .WithTitle("Test Menu")
            .Build();
        await _menuRepository.CreateAsync(menu);
        return menu;
    }

    private async Task<Day> CreateDayAsync(Guid eventId, Guid menuId, int sequenceNumber)
    {
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle($"Day {sequenceNumber}")
            .WithSequenceNumber(sequenceNumber)
            .Build();
        await _dayRepository.CreateAsync(day);
        return day;
    }

    private async Task<User> CreateUserAsync(string phone)
    {
        var user = UserFixture.Default()
            .WithPhone(phone)
            .Build();
        await _userRepository.CreateAsync(user);
        return user;
    }

    private async Task<(Event ev, List<Day> days)> CreateFullEventWithDaysAsync(int daysCount = 2)
    {
        var location = await CreateLocationAsync();
        var ev = await CreateEventAsync(location.Id);
        var days = new List<Day>();
        for (int i = 1; i <= daysCount; i++)
        {
            var menu = await CreateMenuAsync();
            var day = await CreateDayAsync(ev.Id, menu.Id, i);
            days.Add(day);
        }
        return (ev, days);
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldPersistRegistration_WithDays()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(2);
        var user = await CreateUserAsync("+1111111111");
        var dayIds = days.Select(d => d.Id).ToList();

        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .WithType(RegistrationType.Standard)
            .WithPayment(true)
            .Build();

        // Act
        var result = await _sutService.CreateAsync(registration, dayIds);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.EventId.Should().Be(ev.Id);
        result.UserId.Should().Be(user.Id);
        result.Type.Should().Be(RegistrationType.Standard);
        result.Payment.Should().BeTrue();

        var saved = await _sutService.GetByIdAsync(result.Id, includeDays: true);
        saved.Should().NotBeNull();
        saved!.Days.Select(d => d.Id).Should().BeEquivalentTo(dayIds);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenEmpty()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user = await CreateUserAsync("+2222222222");
        var registration = new Registration
        {
            Id = Guid.Empty,
            EventId = ev.Id,
            UserId = user.Id,
            Type = RegistrationType.Standard,
            Payment = false
        };

        // Act
        var result = await _sutService.CreateAsync(registration, new[] { days[0].Id });

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnRegistration_WithDays_WhenRequested()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user = await CreateUserAsync("+3333333333");
        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .Build();
        await _sutService.CreateAsync(registration, new[] { days[0].Id });

        // Act
        var result = await _sutService.GetByIdAsync(registration.Id, includeDays: true);

        // Assert
        result.Should().NotBeNull();
        result!.Days.Should().HaveCount(1);
        result.Days[0].Id.Should().Be(days[0].Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldNotIncludeDays_WhenNotRequested()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user = await CreateUserAsync("+4444444444");
        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .Build();
        await _sutService.CreateAsync(registration, new[] { days[0].Id });

        // Act
        var result = await _sutService.GetByIdAsync(registration.Id, includeDays: false);

        // Assert
        result.Should().NotBeNull();
        result!.Days.Should().BeNullOrEmpty();
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
    public async Task GetAsync_ShouldReturnAllRegistrations_WhenNoFilter()
    {
        // Arrange
        var (ev1, days1) = await CreateFullEventWithDaysAsync(1);
        var (ev2, days2) = await CreateFullEventWithDaysAsync(1);
        var user1 = await CreateUserAsync("+1111111111");
        var user2 = await CreateUserAsync("+2222222222");

        var reg1 = RegistrationFixture.Default().WithEventId(ev1.Id).WithUserId(user1.Id).Build();
        var reg2 = RegistrationFixture.Default().WithEventId(ev2.Id).WithUserId(user2.Id).Build();
        await _sutService.CreateAsync(reg1, new[] { days1[0].Id });
        await _sutService.CreateAsync(reg2, new[] { days2[0].Id });

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(r => r.Id).Should().Contain([reg1.Id, reg2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByEventId()
    {
        // Arrange
        var (ev1, days1) = await CreateFullEventWithDaysAsync(1);
        var (ev2, days2) = await CreateFullEventWithDaysAsync(1);
        var user = await CreateUserAsync("+1111111111");

        var reg1 = RegistrationFixture.Default().WithEventId(ev1.Id).WithUserId(user.Id).Build();
        var reg2 = RegistrationFixture.Default().WithEventId(ev2.Id).WithUserId(user.Id).Build();
        await _sutService.CreateAsync(reg1, new[] { days1[0].Id });
        await _sutService.CreateAsync(reg2, new[] { days2[0].Id });

        var filter = new RegistrationFilter { EventId = ev1.Id };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result[0].EventId.Should().Be(ev1.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByUserId()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user1 = await CreateUserAsync("+1111111111");
        var user2 = await CreateUserAsync("+2222222222");

        var reg1 = RegistrationFixture.Default().WithEventId(ev.Id).WithUserId(user1.Id).Build();
        var reg2 = RegistrationFixture.Default().WithEventId(ev.Id).WithUserId(user2.Id).Build();
        await _sutService.CreateAsync(reg1, new[] { days[0].Id });
        await _sutService.CreateAsync(reg2, new[] { days[0].Id });

        var filter = new RegistrationFilter { UserId = user1.Id };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(user1.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByType()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user1 = await CreateUserAsync("+1111111111");
        var user2 = await CreateUserAsync("+2222222222");

        var regStandard = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user1.Id)
            .WithType(RegistrationType.Standard)
            .Build();
        var regVip = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user2.Id)
            .WithType(RegistrationType.Vip)
            .Build();
        await _sutService.CreateAsync(regStandard, new[] { days[0].Id });
        await _sutService.CreateAsync(regVip, new[] { days[0].Id });

        var filter = new RegistrationFilter { Type = RegistrationType.Vip };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result[0].Type.Should().Be(RegistrationType.Vip);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByPayment()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user1 = await CreateUserAsync("+1111111111");
        var user2 = await CreateUserAsync("+2222222222");

        var regPaid = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user1.Id)
            .WithPayment(true)
            .Build();
        var regUnpaid = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user2.Id)
            .WithPayment(false)
            .Build();
        await _sutService.CreateAsync(regPaid, new[] { days[0].Id });
        await _sutService.CreateAsync(regUnpaid, new[] { days[0].Id });

        var filter = new RegistrationFilter { Payment = false };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result[0].Payment.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var userBase = await CreateUserAsync("+0000000000");
        for (int i = 1; i <= 5; i++)
        {
            var user = await CreateUserAsync($"+{i:D10}");
            var reg = RegistrationFixture.Default()
                .WithEventId(ev.Id)
                .WithUserId(user.Id)
                .Build();
            await _sutService.CreateAsync(reg, new[] { days[0].Id });
        }

        var filter = new RegistrationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ShouldReturnRegistrationsForUser_WithPagination()
    {
        // Arrange
        var user = await CreateUserAsync("+9999999999");
        var registrations = new List<Registration>();

        for (int i = 0; i < 3; i++)
        {
            var (ev, days) = await CreateFullEventWithDaysAsync(1);
            var reg = RegistrationFixture.Default()
                .WithEventId(ev.Id)
                .WithUserId(user.Id)
                .Build();
            var created = await _sutService.CreateAsync(reg, new[] { days[0].Id });
            registrations.Add(created);
        }

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 1 };

        // Act
        var result = await _sutService.GetByUserIdAsync(user.Id, filter, includeDays: false);

        // Assert
        result.Should().HaveCount(1);
        result.All(r => r.UserId == user.Id).Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateRegistrationFields()
    {
        // Arrange
        var (ev1, days1) = await CreateFullEventWithDaysAsync(1);
        var (ev2, days2) = await CreateFullEventWithDaysAsync(1);
        var user1 = await CreateUserAsync("+1111111111");
        var user2 = await CreateUserAsync("+2222222222");

        var registration = RegistrationFixture.Default()
            .WithEventId(ev1.Id)
            .WithUserId(user1.Id)
            .WithType(RegistrationType.Standard)
            .WithPayment(false)
            .Build();
        await _sutService.CreateAsync(registration, new[] { days1[0].Id });

        var updated = new Registration
        {
            Id = registration.Id,
            EventId = ev2.Id,
            UserId = user2.Id,
            Type = RegistrationType.Vip,
            Payment = true
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(registration.Id);
        result.Should().NotBeNull();
        result!.EventId.Should().Be(ev2.Id);
        result.UserId.Should().Be(user2.Id);
        result.Type.Should().Be(RegistrationType.Vip);
        result.Payment.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAddAndRemoveDays_WhenDayIdsProvided()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(3);
        var user = await CreateUserAsync("+1111111111");

        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .Build();
        // initially only day1
        await _sutService.CreateAsync(registration, new[] { days[0].Id });

        // Act: change to days2 and days3 (remove day1, add day2, day3)
        await _sutService.UpdateAsync(registration, new[] { days[1].Id, days[2].Id });

        // Assert
        var result = await _sutService.GetByIdAsync(registration.Id, includeDays: true);
        result.Should().NotBeNull();
        result!.Days.Select(d => d.Id).Should().BeEquivalentTo(new[] { days[1].Id, days[2].Id });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotChangeDays_WhenDayIdsNotProvided()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(2);
        var user = await CreateUserAsync("+1111111111");

        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .Build();
        await _sutService.CreateAsync(registration, new[] { days[0].Id, days[1].Id });

        var updated = new Registration
        {
            Id = registration.Id,
            EventId = ev.Id,
            UserId = user.Id,
            Type = RegistrationType.Vip,
            Payment = true
        };

        // Act
        await _sutService.UpdateAsync(updated); // dayIds = null

        // Assert
        var result = await _sutService.GetByIdAsync(registration.Id, includeDays: true);
        result.Should().NotBeNull();
        result!.Days.Select(d => d.Id).Should().BeEquivalentTo(new[] { days[0].Id, days[1].Id });
        result.Type.Should().Be(RegistrationType.Vip);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenRegistrationNotFound()
    {
        // Arrange
        var registration = RegistrationFixture.Default().Build();

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(registration);

        // Assert
        await act.Should().ThrowAsync<RegistrationServiceException>()
            .WithMessage($"Registration '{registration.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveRegistration()
    {
        // Arrange
        var (ev, days) = await CreateFullEventWithDaysAsync(1);
        var user = await CreateUserAsync("+1111111111");
        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .Build();
        await _sutService.CreateAsync(registration, new[] { days[0].Id });

        // Act
        await _sutService.DeleteAsync(registration.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(registration.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenRegistrationNotFound()
    {
        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<RegistrationServiceException>()
            .WithMessage($"Registration '* was not found.");
    }
}