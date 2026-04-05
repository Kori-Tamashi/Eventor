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
public class CalculationServiceUnitTests
{
    private Mock<IItemService> _itemServiceMock;
    private Mock<IMenuService> _menuServiceMock;
    private Mock<IDayService> _dayServiceMock;
    private Mock<IEventService> _eventServiceMock;
    private Mock<ICalculationSupportService> _supportServiceMock;
    private CalculationService _calculationService;

    [TestInitialize]
    public void Setup()
    {
        _itemServiceMock = new Mock<IItemService>();
        _menuServiceMock = new Mock<IMenuService>();
        _dayServiceMock = new Mock<IDayService>();
        _eventServiceMock = new Mock<IEventService>();
        _supportServiceMock = new Mock<ICalculationSupportService>();
        _calculationService = new CalculationService(
            _itemServiceMock.Object,
            _menuServiceMock.Object,
            _dayServiceMock.Object,
            _eventServiceMock.Object,
            _supportServiceMock.Object);
    }

    #region GetItemCostAsync

    [TestMethod]
    public async Task GetItemCostAsync_ShouldReturnCost_WhenItemExists()
    {
        var itemId = Guid.NewGuid();
        var expectedCost = 150m;
        _itemServiceMock.Setup(s => s.GetByIdAsync(itemId)).ReturnsAsync(new Item { Id = itemId, Cost = expectedCost });

        var result = await _calculationService.GetItemCostAsync(itemId);

        result.Should().Be(expectedCost);
    }

    [TestMethod]
    public async Task GetItemCostAsync_ShouldThrowEconomyServiceException_WhenItemNotFound()
    {
        var itemId = Guid.NewGuid();
        _itemServiceMock.Setup(s => s.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);

        await _calculationService
            .Invoking(s => s.GetItemCostAsync(itemId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Item '{itemId}' was not found.");
    }

    #endregion

    #region GetMenuCostAsync

    [TestMethod]
    public async Task GetMenuCostAsync_ShouldReturnSumOfItemCosts()
    {
        var menuId = Guid.NewGuid();
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var menu = new Menu
        {
            Id = menuId,
            MenuItems = new List<MenuItem>
            {
                new MenuItem(item1Id, 2),
                new MenuItem(item2Id, 3)
            }
        };
        _menuServiceMock.Setup(s => s.GetByIdAsync(menuId, true)).ReturnsAsync(menu);
        _itemServiceMock.Setup(s => s.GetByIdAsync(item1Id)).ReturnsAsync(new Item { Cost = 100m });
        _itemServiceMock.Setup(s => s.GetByIdAsync(item2Id)).ReturnsAsync(new Item { Cost = 50m });

        var result = await _calculationService.GetMenuCostAsync(menuId);

        result.Should().Be(100m * 2 + 50m * 3);
    }

    [TestMethod]
    public async Task GetMenuCostAsync_ShouldSkipMissingItems()
    {
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menu = new Menu
        {
            Id = menuId,
            MenuItems = new List<MenuItem> { new MenuItem(itemId, 2) }
        };
        _menuServiceMock.Setup(s => s.GetByIdAsync(menuId, true)).ReturnsAsync(menu);
        _itemServiceMock.Setup(s => s.GetByIdAsync(itemId)).ReturnsAsync((Item?)null);

        var result = await _calculationService.GetMenuCostAsync(menuId);

        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetMenuCostAsync_ShouldThrow_WhenMenuNotFound()
    {
        var menuId = Guid.NewGuid();
        _menuServiceMock.Setup(s => s.GetByIdAsync(menuId, true)).ReturnsAsync((Menu?)null);

        await _calculationService
            .Invoking(s => s.GetMenuCostAsync(menuId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Menu '{menuId}' was not found.");
    }

    #endregion

    #region GetDaysCostAsync

    [TestMethod]
    public async Task GetDaysCostAsync_ShouldReturnZero_WhenEmpty()
    {
        var result = await _calculationService.GetDaysCostAsync(Array.Empty<Guid>());
        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetDaysCostAsync_ShouldSumDistinctDayCosts()
    {
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2)).ReturnsAsync(200m);

        var result = await _calculationService.GetDaysCostAsync(new[] { day1, day2, day1 });

        result.Should().Be(300m);
        _supportServiceMock.Verify(s => s.GetSingleDayCostAsync(day1), Times.Once);
        _supportServiceMock.Verify(s => s.GetSingleDayCostAsync(day2), Times.Once);
    }

    #endregion

    #region GetEventCostAsync

    [TestMethod]
    public async Task GetEventCostAsync_ShouldReturnSumOfDayCosts()
    {
        var eventId = Guid.NewGuid();
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId });
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } });
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1)).ReturnsAsync(150m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2)).ReturnsAsync(250m);

        var result = await _calculationService.GetEventCostAsync(eventId);

        result.Should().Be(400m);
    }

    [TestMethod]
    public async Task GetEventCostAsync_ShouldThrow_WhenEventNotFound()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync((Event?)null);

        await _calculationService
            .Invoking(s => s.GetEventCostAsync(eventId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Event '{eventId}' was not found.");
    }

    #endregion

    #region GetDayPriceAsync

    [TestMethod]
    public async Task GetDayPriceAsync_ShouldCalculateCorrectPrice()
    {
        var dayId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = eventId };
        var ev = new Event { Id = eventId, Percent = 20 };
        var coefficient = 1.5m;

        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(dayId))))
            .ReturnsAsync(coefficient);

        // Моки для приватного метода CalculateFundamentalPriceNDAsync
        var days = new List<Day> { day };
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(dayId, true)).ReturnsAsync(5);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(dayId)).ReturnsAsync(200m);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { dayId } });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(dayId)), true))
            .ReturnsAsync(5);

        var result = await _calculationService.GetDayPriceAsync(dayId);

        // Фундаментальная цена = totalCost / (coeff * participants) = 200 / (1.5*5) = 26.666...
        // Цена = (1 + 0.2) * 26.666... * 1.5 = 1.2 * 40 = 48
        result.Should().BeApproximately(48m, 0.01m);
    }

    [TestMethod]
    public async Task GetDayPriceAsync_ShouldThrow_WhenDayNotFound()
    {
        var dayId = Guid.NewGuid();
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync((Day?)null);

        await _calculationService
            .Invoking(s => s.GetDayPriceAsync(dayId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Day '{dayId}' was not found.");
    }

    #endregion

    #region GetDayPriceWithPrivilegesAsync

    [TestMethod]
    public async Task GetDayPriceWithPrivilegesAsync_ShouldCalculateUsingPrivilegedFundamental()
    {
        var dayId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = eventId };
        var ev = new Event { Id = eventId, Percent = 10 };
        var coefficient = 2m;

        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(dayId))))
            .ReturnsAsync(coefficient);

        var days = new List<Day> { day };
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(dayId, false)).ReturnsAsync(3);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(dayId)).ReturnsAsync(150m);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, false))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { dayId } });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(dayId)), false))
            .ReturnsAsync(3);

        var result = await _calculationService.GetDayPriceWithPrivilegesAsync(dayId);

        // totalCost = 150, fundamental = 150 / (2*3) = 25, price = 1.1 * 25 * 2 = 55
        result.Should().BeApproximately(55m, 0.01m);
    }

    #endregion

    #region GetDaysPriceAsync

    [TestMethod]
    public async Task GetDaysPriceAsync_ShouldReturnZero_WhenEmpty()
    {
        var result = await _calculationService.GetDaysPriceAsync(Array.Empty<Guid>());
        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetDaysPriceAsync_ShouldCalculatePriceForCombination()
    {
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();
        var dayIds = new[] { day1, day2 };
        var eventId = Guid.NewGuid();
        var firstDay = new Day { Id = day1, EventId = eventId };
        var ev = new Event { Id = eventId, Percent = 15 };

        _dayServiceMock.Setup(s => s.GetByIdAsync(day1)).ReturnsAsync(firstDay);
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _supportServiceMock.Setup(s => s.EnsureDaysFromSameEventAsync(dayIds)).Returns(Task.CompletedTask);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(dayIds)).ReturnsAsync(3m);

        var days = new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } };
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(day1, true)).ReturnsAsync(4);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(day2, true)).ReturnsAsync(6);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2)).ReturnsAsync(200m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Count() == 1 && c.Contains(day1))))
            .ReturnsAsync(1m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Count() == 1 && c.Contains(day2))))
            .ReturnsAsync(2m);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { dayIds });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(dayIds, true)).ReturnsAsync(5);

        var result = await _calculationService.GetDaysPriceAsync(dayIds);

        // totalCost = 300, fundamental = 300 / (3*5) = 20, price = 1.15 * 20 * 3 = 69
        result.Should().BeApproximately(69m, 0.01m);
    }

    #endregion

    #region GetFundamentalPriceForSingleDayAsync

    [TestMethod]
    public async Task GetFundamentalPriceForSingleDayAsync_ShouldCalculateCorrectly()
    {
        var eventId = Guid.NewGuid();
        var day1 = new Day { Id = Guid.NewGuid(), EventId = eventId };
        var day2 = new Day { Id = Guid.NewGuid(), EventId = eventId };
        var days = new List<Day> { day1, day2 };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId });
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1.Id)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2.Id)).ReturnsAsync(200m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(day1.Id) && c.Count() == 1)))
            .ReturnsAsync(1m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(day2.Id) && c.Count() == 1)))
            .ReturnsAsync(2m);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(day1.Id, true)).ReturnsAsync(10);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(day2.Id, true)).ReturnsAsync(5);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { day1.Id }, new[] { day2.Id } });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(day1.Id)), true))
            .ReturnsAsync(10);
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(day2.Id)), true))
            .ReturnsAsync(5);

        var result = await _calculationService.GetFundamentalPriceForSingleDayAsync(eventId);

        // totalCost = 300, sum = 1*10 + 2*5 = 20, fundamental = 15
        result.Should().Be(15m);
    }

    [TestMethod]
    public async Task GetFundamentalPriceForSingleDayAsync_ShouldThrow_WhenEventHasNoDays()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId });
        _dayServiceMock.Setup(s => s.GetAsync(It.IsAny<DayFilter>())).ReturnsAsync(new List<Day>());

        await _calculationService
            .Invoking(s => s.GetFundamentalPriceForSingleDayAsync(eventId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("Event must contain at least one day.");
    }

    #endregion

    #region GetFundamentalPriceForMultiDayAsync

    [TestMethod]
    public async Task GetFundamentalPriceForMultiDayAsync_ShouldCalculateCorrectly()
    {
        var eventId = Guid.NewGuid();
        var day1 = new Day { Id = Guid.NewGuid(), EventId = eventId };
        var day2 = new Day { Id = Guid.NewGuid(), EventId = eventId };
        var days = new List<Day> { day1, day2 };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId });
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1.Id)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2.Id)).ReturnsAsync(200m);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { day1.Id }, new[] { day2.Id } });
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(day1.Id) && c.Count() == 1)))
            .ReturnsAsync(1m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(day2.Id) && c.Count() == 1)))
            .ReturnsAsync(2m);
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(day1.Id)), true))
            .ReturnsAsync(10);
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(day2.Id)), true))
            .ReturnsAsync(5);

        var result = await _calculationService.GetFundamentalPriceForMultiDayAsync(eventId);

        result.Should().Be(15m);
    }

    #endregion

    #region IsSingleDayCaseBalancedAsync

    [TestMethod]
    public async Task IsSingleDayCaseBalancedAsync_ShouldReturnTrue_WhenIncomeSufficient()
    {
        var eventId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = eventId };
        var days = new List<Day> { day };
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId, Percent = 0 });
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId))).ReturnsAsync(days);
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId)).ReturnsAsync(day);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(dayId)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(dayId)))).ReturnsAsync(1m);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(dayId, true)).ReturnsAsync(5);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { dayId } });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(dayId)), true))
            .ReturnsAsync(5);

        var result = await _calculationService.IsSingleDayCaseBalancedAsync(eventId);
        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task IsSingleDayCaseBalancedAsync_ShouldThrow_WhenNoParticipants()
    {
        var eventId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var day = new Day { Id = dayId, EventId = eventId };
        var days = new List<Day> { day };

        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId))
            .ReturnsAsync(new Event { Id = eventId, Percent = 0 });
        _dayServiceMock.Setup(s => s.GetAsync(It.Is<DayFilter>(f => f.EventId == eventId)))
            .ReturnsAsync(days);
        _dayServiceMock.Setup(s => s.GetByIdAsync(dayId))
            .ReturnsAsync(day);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(dayId))
            .ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(It.Is<IEnumerable<Guid>>(c => c.Contains(dayId))))
            .ReturnsAsync(1m);
        _supportServiceMock.Setup(s => s.GetParticipantsByDayAsync(dayId, true))
            .ReturnsAsync(0);
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { new[] { dayId } });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(
                It.Is<IReadOnlyCollection<Guid>>(c => c.Contains(dayId)), true))
            .ReturnsAsync(0);

        await _calculationService
            .Invoking(s => s.IsSingleDayCaseBalancedAsync(eventId))
            .Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("Fundamental price denominator must be positive.");
    }

    #endregion

    #region IsMultiDayCaseBalancedAsync

    [TestMethod]
    public async Task IsMultiDayCaseBalancedAsync_ShouldReturnTrue_WhenIncomeSufficient()
    {
        var eventId = Guid.NewGuid();
        var day1 = Guid.NewGuid();
        var day2 = Guid.NewGuid();
        var combination = new[] { day1, day2 };
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>> { combination });
        _supportServiceMock.Setup(s => s.GetParticipantsCountExactAsync(combination, true)).ReturnsAsync(10);
        _supportServiceMock.Setup(s => s.GetDayCoefficientAsync(combination)).ReturnsAsync(2m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day1)).ReturnsAsync(100m);
        _supportServiceMock.Setup(s => s.GetSingleDayCostAsync(day2)).ReturnsAsync(100m);
        _dayServiceMock.Setup(s => s.GetAsync(It.IsAny<DayFilter>()))
            .ReturnsAsync(new List<Day> { new Day { Id = day1 }, new Day { Id = day2 } });
        _eventServiceMock.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(new Event { Id = eventId, Percent = 10 });
        _dayServiceMock.Setup(s => s.GetByIdAsync(day1)).ReturnsAsync(new Day { Id = day1, EventId = eventId });

        var result = await _calculationService.IsMultiDayCaseBalancedAsync(eventId);
        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task IsMultiDayCaseBalancedAsync_ShouldReturnFalse_WhenNoCombinations()
    {
        var eventId = Guid.NewGuid();
        _supportServiceMock.Setup(s => s.GetCurrentDayCombinationsAsync(eventId, true))
            .ReturnsAsync(new List<IReadOnlyCollection<Guid>>());

        var result = await _calculationService.IsMultiDayCaseBalancedAsync(eventId);
        result.Should().BeFalse();
    }

    #endregion
}