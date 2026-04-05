using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class CalculationSupportServiceUnitTests
{
    private Mock<IItemService> _itemServiceMock;
    private Mock<IMenuService> _menuServiceMock;
    private Mock<IDayService> _dayServiceMock;
    private Mock<IEventService> _eventServiceMock;
    private Mock<ILocationService> _locationServiceMock;
    private Mock<IRegistrationService> _registrationServiceMock;
    private CalculationSupportService _service;

    [TestInitialize]
    public void Setup()
    {
        _itemServiceMock = new Mock<IItemService>();
        _menuServiceMock = new Mock<IMenuService>();
        _dayServiceMock = new Mock<IDayService>();
        _eventServiceMock = new Mock<IEventService>();
        _locationServiceMock = new Mock<ILocationService>();
        _registrationServiceMock = new Mock<IRegistrationService>();

        _service = new CalculationSupportService(
            _itemServiceMock.Object,
            _menuServiceMock.Object,
            _dayServiceMock.Object,
            _eventServiceMock.Object,
            _locationServiceMock.Object,
            _registrationServiceMock.Object);
    }

    #region GetSingleDayCostAsync

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldReturnSumOfMenuCostAndLocationCost_WhenDayExists()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var day = new Day { Id = dayId, EventId = eventId, MenuId = menuId };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);

        var menu = new Menu { Id = menuId, MenuItems = new List<MenuItem> { new MenuItem(Guid.NewGuid(), 2) } };
        _menuServiceMock.Setup(s => s.GetByIdAsync(menuId, true)).ReturnsAsync(menu);

        var item = new Item { Id = menu.MenuItems[0].ItemId, Cost = 10m };
        _itemServiceMock.Setup(s => s.GetByIdAsync(menu.MenuItems[0].ItemId)).ReturnsAsync(item);

        var ev = new Event { Id = eventId, LocationId = locationId };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);

        var location = new Location { Cost = 100m };
        _locationServiceMock.Setup(s => s.GetByIdAsync(locationId)).ReturnsAsync(location);

        // Act
        var result = await _service.GetSingleDayCostAsync(dayId);

        // Assert
        result.Should().Be(10m * 2 + 100m);
    }

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldThrowEconomyServiceException_WhenDayNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync((Day?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetSingleDayCostAsync(dayId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Day '{dayId}' was not found.");
    }

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldThrowEconomyServiceException_WhenMenuNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, MenuId = Guid.NewGuid() };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _menuServiceMock.Setup(s => s.GetByIdAsync(day.MenuId, true)).ReturnsAsync((Menu?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetSingleDayCostAsync(dayId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Menu '{day.MenuId}' was not found.");
    }

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldThrowEconomyServiceException_WhenEventNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = Guid.NewGuid(), MenuId = Guid.NewGuid() };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _menuServiceMock.Setup(s => s.GetByIdAsync(day.MenuId, true)).ReturnsAsync(new Menu());
        _eventServiceMock.Setup(s => s.GetByIdAsync(day.EventId)).ReturnsAsync((Event?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetSingleDayCostAsync(dayId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Event '{day.EventId}' was not found.");
    }

    #endregion

    #region GetDayCoefficientAsync

    private async Task SetupDayCostAsync(Guid dayId, decimal cost)
    {
        var menuId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var day = new Day { Id = dayId, MenuId = menuId, EventId = eventId };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);

        var menu = new Menu { Id = menuId, MenuItems = new List<MenuItem>() };
        _menuServiceMock.Setup(s => s.GetByIdAsync(menuId, true)).ReturnsAsync(menu);

        var ev = new Event { Id = eventId, LocationId = locationId };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);

        var location = new Location { Cost = cost }; // Здесь cost будет добавлен к menuCost
        _locationServiceMock.Setup(s => s.GetByIdAsync(locationId)).ReturnsAsync(location);

        // menuCost = 0, так как MenuItems пуст, итого общая стоимость = cost
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldReturnSumOfCostsDividedByMinCost_WhenAllDaysValid()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1Id = Guid.NewGuid();
        var day2Id = Guid.NewGuid();
        var menu1Id = Guid.NewGuid();
        var menu2Id = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var day1 = new Day { Id = day1Id, EventId = eventId, MenuId = menu1Id };
        var day2 = new Day { Id = day2Id, EventId = eventId, MenuId = menu2Id };
        var allDays = new List<Day> { day1, day2 };

        // Настройка dayService
        _dayServiceMock.Setup(s => s.GetByIdAsync(day1Id)).ReturnsAsync(day1);
        _dayServiceMock.Setup(s => s.GetByIdAsync(day2Id)).ReturnsAsync(day2);
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId))).ReturnsAsync(allDays);

        // Настройка menuService (пустые меню)
        var menu1 = new Menu { Id = menu1Id, MenuItems = new List<MenuItem>() };
        var menu2 = new Menu { Id = menu2Id, MenuItems = new List<MenuItem>() };
        _menuServiceMock.Setup(s => s.GetByIdAsync(menu1Id, true)).ReturnsAsync(menu1);
        _menuServiceMock.Setup(s => s.GetByIdAsync(menu2Id, true)).ReturnsAsync(menu2);

        // Настройка eventService
        var ev = new Event { Id = eventId, LocationId = locationId };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);

        // Настройка locationService (стоимость локации = 100 для обоих дней)
        var location = new Location { Cost = 100m };
        _locationServiceMock.Setup(s => s.GetByIdAsync(locationId)).ReturnsAsync(location);

        // Act
        var result = await _service.GetDayCoefficientAsync(new[] { day1Id, day2Id });

        // Assert
        result.Should().Be(2); // (100+100)/100 = 2
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldReturnZero_WhenNoDaysProvided()
    {
        // Act
        var result = await _service.GetDayCoefficientAsync(Array.Empty<Guid>());

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldThrowEconomyServiceException_WhenFirstDayNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync((Day?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetDayCoefficientAsync(new[] { dayId }))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("Invalid day IDs provided.");
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldThrowEconomyServiceException_WhenEventHasNoDays()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = Guid.NewGuid() };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _dayServiceMock.Setup(s => s.GetAsync(It.IsAny<DayFilter>())).ReturnsAsync(new List<Day>());

        // Act & Assert
        await _service.Invoking(s => s.GetDayCoefficientAsync(new[] { dayId }))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("Event has no days.");
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldThrowEconomyServiceException_WhenSomeDayNotBelongToEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1Id = Guid.NewGuid();
        var day2Id = Guid.NewGuid();

        var day1 = new Day { Id = day1Id, EventId = eventId };
        var day2 = new Day { Id = day2Id, EventId = Guid.NewGuid() }; // different event

        _dayServiceMock.Setup(s => s.GetByIdAsync(day1Id)).ReturnsAsync(day1);
        _dayServiceMock.Setup(s => s.GetByIdAsync(day2Id)).ReturnsAsync(day2);
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId))).ReturnsAsync(new List<Day> { day1 });

        // Act & Assert
        await _service.Invoking(s => s.GetDayCoefficientAsync(new[] { day1Id, day2Id }))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("Some days do not belong to the event.");
    }

    #endregion

    #region EnsureDaysFromSameEventAsync

    [TestMethod]
    public async Task EnsureDaysFromSameEventAsync_ShouldNotThrow_WhenAllDaysFromSameEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1Id = Guid.NewGuid();
        var day2Id = Guid.NewGuid();

        _dayServiceMock.Setup(s => s.GetByIdAsync(day1Id)).ReturnsAsync(new Day { Id = day1Id, EventId = eventId });
        _dayServiceMock.Setup(s => s.GetByIdAsync(day2Id)).ReturnsAsync(new Day { Id = day2Id, EventId = eventId });

        // Act & Assert
        await _service.Invoking(s => s.EnsureDaysFromSameEventAsync(new[] { day1Id, day2Id }))
            .Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task EnsureDaysFromSameEventAsync_ShouldThrowEconomyServiceException_WhenDaysFromDifferentEvents()
    {
        // Arrange
        var day1Id = Guid.NewGuid();
        var day2Id = Guid.NewGuid();

        _dayServiceMock.Setup(s => s.GetByIdAsync(day1Id)).ReturnsAsync(new Day { Id = day1Id, EventId = Guid.NewGuid() });
        _dayServiceMock.Setup(s => s.GetByIdAsync(day2Id)).ReturnsAsync(new Day { Id = day2Id, EventId = Guid.NewGuid() });

        // Act & Assert
        await _service.Invoking(s => s.EnsureDaysFromSameEventAsync(new[] { day1Id, day2Id }))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("All days must belong to the same event.");
    }

    [TestMethod]
    public async Task EnsureDaysFromSameEventAsync_ShouldThrowEconomyServiceException_WhenDayNotFound()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync((Day?)null);

        // Act & Assert
        await _service.Invoking(s => s.EnsureDaysFromSameEventAsync(new[] { dayId }))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Day '{dayId}' was not found.");
    }

    #endregion

    #region GetParticipantsByDayAsync

    [TestMethod]
    public async Task GetParticipantsByDayAsync_ShouldReturnCountOfPaidRegistrationsWithThatDay_WhenIncludePrivilegedTrue()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var registrations = new List<Registration>
        {
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = true, Type = RegistrationType.Vip, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = false, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = true, Type = RegistrationType.Organizer, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = Guid.NewGuid() } } }
        };

        _registrationServiceMock.Setup(s => s.GetAsync(It.IsAny<RegistrationFilter>(), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetParticipantsByDayAsync(dayId, includePrivileged: true);

        // Assert
        result.Should().Be(3); // Standard + Vip + Organizer, all paid
    }

    [TestMethod]
    public async Task GetParticipantsByDayAsync_ShouldExcludeVipAndOrganizer_WhenIncludePrivilegedFalse()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var registrations = new List<Registration>
        {
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = true, Type = RegistrationType.Vip, Days = new List<Day> { new Day { Id = dayId } } },
            new Registration { Payment = true, Type = RegistrationType.Organizer, Days = new List<Day> { new Day { Id = dayId } } }
        };

        _registrationServiceMock.Setup(s => s.GetAsync(It.IsAny<RegistrationFilter>(), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetParticipantsByDayAsync(dayId, includePrivileged: false);

        // Assert
        result.Should().Be(1); // Only Standard
    }

    #endregion

    #region GetParticipantsCountExactAsync

    [TestMethod]
    public async Task GetParticipantsCountExactAsync_ShouldReturnCountOfRegistrationsWithExactDaySet_WhenIncludePrivilegedTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dayIds = new[] { Guid.NewGuid(), Guid.NewGuid() }.ToList();
        var dayIdSet = dayIds.ToHashSet();

        var registrations = new List<Registration>
        {
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = dayIds.Select(id => new Day { Id = id }).ToList() },
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = dayIds.Select(id => new Day { Id = id }).ToList() },
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new[] { dayIds[0] }.Select(id => new Day { Id = id }).ToList() }, // not exact
            new Registration { Payment = false, Type = RegistrationType.Standard, Days = dayIds.Select(id => new Day { Id = id }).ToList() }
        };

        _dayServiceMock.Setup(s => s.GetByIdAsync(dayIds[0])).ReturnsAsync(new Day { Id = dayIds[0], EventId = eventId });
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayIds[1])).ReturnsAsync(new Day { Id = dayIds[1], EventId = eventId });
        _registrationServiceMock.Setup(s => s.GetAsync(It.Is<RegistrationFilter>(f => f.EventId == eventId), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetParticipantsCountExactAsync(dayIds, includePrivileged: true);

        // Assert
        result.Should().Be(2); // Two exact matches with payment
    }

    [TestMethod]
    public async Task GetParticipantsCountExactAsync_ShouldReturnZero_WhenNoDayIds()
    {
        // Act
        var result = await _service.GetParticipantsCountExactAsync(Array.Empty<Guid>(), true);

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetParticipantsCountExactAsync_ShouldThrow_WhenDaysNotFromSameEvent()
    {
        // Arrange
        var dayIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayIds[0])).ReturnsAsync(new Day { EventId = Guid.NewGuid() });
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayIds[1])).ReturnsAsync(new Day { EventId = Guid.NewGuid() });

        // Act & Assert
        await _service.Invoking(s => s.GetParticipantsCountExactAsync(dayIds, true))
            .Should().ThrowAsync<EconomyServiceException>();
    }

    #endregion

    #region GetCurrentDayCombinationsAsync

    [TestMethod]
    public async Task GetCurrentDayCombinationsAsync_ShouldReturnDistinctDayCombinations_WhenIncludePrivilegedTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();
        var day3 = Guid.NewGuid();

        var registrations = new List<Registration>
        {
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } } },
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } } },
            new Registration { Payment = true, Type = RegistrationType.Vip, Days = new List<Day> { new Day { Id = day3 } } },
            new Registration { Payment = false, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = day1 } } }
        };

        _registrationServiceMock.Setup(s => s.GetAsync(It.Is<RegistrationFilter>(f => f.EventId == eventId), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetCurrentDayCombinationsAsync(eventId, includePrivileged: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.OrderBy(x => x).SequenceEqual(new[] { day1, day2 }.OrderBy(x => x)));
        result.Should().Contain(c => c.SequenceEqual(new[] { day3 }));
    }

    [TestMethod]
    public async Task GetCurrentDayCombinationsAsync_ShouldExcludePrivilegedRegistrations_WhenIncludePrivilegedFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();

        var registrations = new List<Registration>
        {
            new Registration { Payment = true, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = day1 } } },
            new Registration { Payment = true, Type = RegistrationType.Vip, Days = new List<Day> { new Day { Id = day2 } } },
            new Registration { Payment = true, Type = RegistrationType.Organizer, Days = new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } } }
        };

        _registrationServiceMock.Setup(s => s.GetAsync(It.Is<RegistrationFilter>(f => f.EventId == eventId), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetCurrentDayCombinationsAsync(eventId, includePrivileged: false);

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().ContainSingle().Which.Should().Be(day1);
    }

    [TestMethod]
    public async Task GetCurrentDayCombinationsAsync_ShouldReturnEmptyList_WhenNoPaidRegistrations()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var registrations = new List<Registration>
        {
            new Registration { Payment = false, Type = RegistrationType.Standard, Days = new List<Day> { new Day { Id = Guid.NewGuid() } } }
        };

        _registrationServiceMock.Setup(s => s.GetAsync(It.Is<RegistrationFilter>(f => f.EventId == eventId), true))
            .ReturnsAsync(registrations);

        // Act
        var result = await _service.GetCurrentDayCombinationsAsync(eventId, true);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}