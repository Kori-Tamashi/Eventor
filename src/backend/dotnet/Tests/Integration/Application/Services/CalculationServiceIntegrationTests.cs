using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Context;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class CalculationServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private IItemService _itemService = null!;
    private IMenuService _menuService = null!;
    private IDayService _dayService = null!;
    private IEventService _eventService = null!;
    private ILocationService _locationService = null!;
    private IRegistrationService _registrationService = null!;
    private ICalculationSupportService _calculationSupportService = null!;
    private ICalculationService _calculationService = null!;

    [TestInitialize]
    public void Setup()
    {
        var itemRepository = new ItemRepository(DbContext!, NullLogger<ItemRepository>.Instance);
        _itemService = new ItemService(itemRepository);

        var menuRepository = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        var menuItemRepository = new MenuItemRepository(DbContext!, NullLogger<MenuItemRepository>.Instance);
        _menuService = new MenuService(menuRepository, menuItemRepository);

        var dayRepository = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        _dayService = new DayService(dayRepository);

        var eventRepository = new EventRepository(DbContext!, NullLogger<EventRepository>.Instance);
        var registrationRepository = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        var registrationDayRepository = new RegistrationDayRepository(DbContext!, NullLogger<RegistrationDayRepository>.Instance);
        _registrationService = new RegistrationService(registrationRepository, registrationDayRepository);
        _eventService = new EventService(eventRepository, registrationRepository, dayRepository);

        var locationRepository = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        _locationService = new LocationService(locationRepository);

        _calculationSupportService = new CalculationSupportService(
            _itemService,
            _menuService,
            _dayService,
            _eventService,
            _locationService,
            _registrationService);

        _calculationService = new CalculationService(
            _itemService,
            _menuService,
            _dayService,
            _eventService,
            _calculationSupportService);
    }

    #region Helper Methods

    private async Task<Location> CreateLocationAsync(decimal cost = 1000m, int capacity = 100)
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithDescription("Test Description")
            .WithCost(cost)
            .WithCapacity(capacity)
            .Build();
        return await _locationService.CreateAsync(location);
    }

    private async Task<Item> CreateItemAsync(string title, decimal cost)
    {
        var item = ItemFixture.Default()
            .WithTitle(title)
            .WithCost(cost)
            .Build();
        return await _itemService.CreateAsync(item);
    }

    private async Task<Menu> CreateMenuWithItemsAsync(params (Item item, int amount)[] items)
    {
        var menu = MenuFixture.Default()
            .WithTitle("Test Menu")
            .WithDescription("Menu Description")
            .Build();
        menu = await _menuService.CreateAsync(menu);

        foreach (var (item, amount) in items)
        {
            await _menuService.AddItemAsync(menu.Id, item.Id, amount);
        }

        return await _menuService.GetByIdAsync(menu.Id, includeItems: true) ?? throw new Exception("Menu not found");
    }

    private async Task<Event> CreateEventAsync(
        Location location,
        DateOnly startDate,
        int daysCount,
        double percent = 0)
    {
        var ev = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithTitle("Test Event")
            .WithDescription("Event Description")
            .WithStartDate(startDate)
            .WithDaysCount(daysCount)
            .WithPercent(percent)
            .Build();
        return await _eventService.CreateAsync(ev);
    }

    private async Task<Day> AddDayToEventAsync(Event ev, Menu menu, string title, int sequenceNumber)
    {
        var day = DayFixture.Default()
            .WithEventId(ev.Id)
            .WithMenuId(menu.Id)
            .WithTitle(title)
            .WithSequenceNumber(sequenceNumber)
            .WithDescription("Day Description")
            .Build();
        return await _eventService.AddDayAsync(ev.Id, day);
    }

    private async Task<User> CreateUserAsync(string phone)
    {
        var user = UserFixture.Default()
            .WithName("Test User")
            .WithPhone(phone)
            .WithGender(Gender.Male)
            .WithRole(UserRole.User)
            .WithPasswordHash("hash")
            .Build();
        // Предполагается, что есть UserService; но для простоты создадим через репозиторий напрямую
        var userRepository = new UserRepository(DbContext!, NullLogger<UserRepository>.Instance);
        await userRepository.CreateAsync(user);
        return user;
    }

    private async Task<Registration> RegisterUserOnEventAsync(
        User user,
        Event ev,
        IReadOnlyCollection<Guid> dayIds,
        RegistrationType type = RegistrationType.Standard,
        bool payment = true)
    {
        var registration = RegistrationFixture.Default()
            .WithEventId(ev.Id)
            .WithUserId(user.Id)
            .WithType(type)
            .WithPayment(payment)
            .Build();
        return await _registrationService.CreateAsync(registration, dayIds);
    }

    #endregion

    [TestMethod]
    public async Task GetItemCostAsync_ShouldReturnCorrectCost()
    {
        // Arrange
        var item = await CreateItemAsync("Apple", 50m);

        // Act
        var cost = await _calculationService.GetItemCostAsync(item.Id);

        // Assert
        cost.Should().Be(50m);
    }

    [TestMethod]
    public async Task GetMenuCostAsync_ShouldReturnSumOfItemsCost()
    {
        // Arrange
        var item1 = await CreateItemAsync("Bread", 30m);
        var item2 = await CreateItemAsync("Butter", 20m);
        var menu = await CreateMenuWithItemsAsync(
            (item1, 2),   // 2 * 30 = 60
            (item2, 3));  // 3 * 20 = 60, total 120

        // Act
        var cost = await _calculationService.GetMenuCostAsync(menu.Id);

        // Assert
        cost.Should().Be(120m);
    }

    [TestMethod]
    public async Task GetEventCostAsync_ShouldReturnSumOfDaysCosts()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 500m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 2);

        var item1 = await CreateItemAsync("Item1", 10m);
        var item2 = await CreateItemAsync("Item2", 20m);
        var menu1 = await CreateMenuWithItemsAsync((item1, 1), (item2, 2)); // 10 + 40 = 50
        var menu2 = await CreateMenuWithItemsAsync((item1, 3));              // 30

        var day1 = await AddDayToEventAsync(ev, menu1, "Day1", 1);
        var day2 = await AddDayToEventAsync(ev, menu2, "Day2", 2);

        // Event cost = menu1 cost + menu2 cost + location cost * daysCount? 
        // По логике GetEventCostAsync возвращает сумму GetDaysCostAsync, которая суммирует GetSingleDayCostAsync.
        // GetSingleDayCostAsync = menuCost + locationCost.
        // Стоимость локации добавляется к каждому дню. Значит:
        // day1: 50 + 500 = 550, day2: 30 + 500 = 530, итого 1080.
        var expectedCost = 550m + 530m;

        // Act
        var cost = await _calculationService.GetEventCostAsync(ev.Id);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [TestMethod]
    public async Task GetDaysCostAsync_ShouldReturnSumOfSelectedDays()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 300m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 2);

        var item = await CreateItemAsync("Item", 100m);
        var menu1 = await CreateMenuWithItemsAsync((item, 1)); // 100
        var menu2 = await CreateMenuWithItemsAsync((item, 2)); // 200

        var day1 = await AddDayToEventAsync(ev, menu1, "Day1", 1);
        var day2 = await AddDayToEventAsync(ev, menu2, "Day2", 2);

        // day1 cost: 100 + 300 = 400; day2: 200 + 300 = 500; sum = 900
        var expectedCost = 400m + 500m;

        // Act
        var cost = await _calculationService.GetDaysCostAsync(new[] { day1.Id, day2.Id });

        // Assert
        cost.Should().Be(expectedCost);
    }

    [TestMethod]
    public async Task GetDayPriceAsync_ShouldCalculateWithPercentAndCoefficient()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 1000m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 1, percent: 10);

        var item = await CreateItemAsync("Item", 100m);
        var menu = await CreateMenuWithItemsAsync((item, 2)); // 200

        var day = await AddDayToEventAsync(ev, menu, "SingleDay", 1);

        // Создадим участников, чтобы GetParticipantsByDayAsync вернул >0
        var user1 = await CreateUserAsync("+111111111");
        var user2 = await CreateUserAsync("+222222222");
        await RegisterUserOnEventAsync(user1, ev, new[] { day.Id }, payment: true);
        await RegisterUserOnEventAsync(user2, ev, new[] { day.Id }, payment: true);

        // Фундаментальная цена P0 = totalCost / (A * participants)
        // totalCost = menuCost + locationCost = 200 + 1000 = 1200
        // A = selectedCost / minCost = (200+1000) / (200+1000) = 1 (т.к. один день)
        // participants = 2
        // P0 = 1200 / (1 * 2) = 600
        // Цена с наценкой: (1 + percent/100) * A * P0 = 1.1 * 1 * 600 = 660

        // Act
        var price = await _calculationService.GetDayPriceAsync(day.Id);

        // Assert
        price.Should().Be(660m);
    }

    [TestMethod]
    public async Task GetFundamentalPriceForSingleDayAsync_ShouldCalculateCorrectly()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 200m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 1);

        var item = await CreateItemAsync("Item", 50m);
        var menu = await CreateMenuWithItemsAsync((item, 2)); // 100

        var day = await AddDayToEventAsync(ev, menu, "Day", 1);

        var user1 = await CreateUserAsync("+111111111");
        var user2 = await CreateUserAsync("+222222222");
        var user3 = await CreateUserAsync("+333333333");
        await RegisterUserOnEventAsync(user1, ev, new[] { day.Id }, payment: true);
        await RegisterUserOnEventAsync(user2, ev, new[] { day.Id }, payment: true);
        await RegisterUserOnEventAsync(user3, ev, new[] { day.Id }, payment: true);

        // totalCost = menuCost + locationCost = 100 + 200 = 300
        // A = 1 (один день), participants = 3
        // P0 = 300 / (1 * 3) = 100
        var expected = 100m;

        // Act
        var fundamentalPrice = await _calculationService.GetFundamentalPriceForSingleDayAsync(ev.Id);

        // Assert
        fundamentalPrice.Should().Be(expected);
    }

    [TestMethod]
    public async Task IsSingleDayCaseBalancedAsync_ShouldReturnTrueWhenIncomeCoversCost()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 500m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 1, percent: 0);

        var item = await CreateItemAsync("Item", 100m);
        var menu = await CreateMenuWithItemsAsync((item, 2)); // 200

        var day = await AddDayToEventAsync(ev, menu, "Day", 1);

        // totalCost = 200 + 500 = 700
        // Чтобы покрыть расходы, нужно income >= 700
        // Цена дня = P0 = totalCost / participants (т.к. A=1, percent=0) => price = 700/participants
        // income = price * participants = 700, всегда равно totalCost. Значит всегда true.

        var user1 = await CreateUserAsync("+111111111");
        var user2 = await CreateUserAsync("+222222222");
        await RegisterUserOnEventAsync(user1, ev, new[] { day.Id }, payment: true);
        await RegisterUserOnEventAsync(user2, ev, new[] { day.Id }, payment: true);

        // Act
        var isBalanced = await _calculationService.IsSingleDayCaseBalancedAsync(ev.Id);

        // Assert
        isBalanced.Should().BeTrue();
    }

    [TestMethod]
    public async Task IsSingleDayCaseBalancedAsync_ShouldThrowException_WhenNoParticipants()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 500m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 1);

        var item = await CreateItemAsync("Item", 100m);
        var menu = await CreateMenuWithItemsAsync((item, 2));
        await AddDayToEventAsync(ev, menu, "Day", 1);

        // Act & Assert
        var act = async () => await _calculationService.IsSingleDayCaseBalancedAsync(ev.Id);
        await act.Should().ThrowAsync<Eventor.Services.Exceptions.EconomyServiceException>()
            .WithMessage("No day combinations found for event.");
    }

    [TestMethod]
    public async Task GetDayPriceWithPrivilegesAsync_ShouldIgnorePrivilegedParticipants()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 1000m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 1, percent: 0);

        var item = await CreateItemAsync("Item", 100m);
        var menu = await CreateMenuWithItemsAsync((item, 1)); // 100
        var day = await AddDayToEventAsync(ev, menu, "Day", 1);

        // totalCost = 100 + 1000 = 1100
        // Создаём 2 обычных участника и 2 привилегированных (VIP, Organizer)
        var user1 = await CreateUserAsync("+111111111");
        var user2 = await CreateUserAsync("+222222222");
        var vip = await CreateUserAsync("+333333333");
        var org = await CreateUserAsync("+444444444");

        await RegisterUserOnEventAsync(user1, ev, new[] { day.Id }, RegistrationType.Standard, true);
        await RegisterUserOnEventAsync(user2, ev, new[] { day.Id }, RegistrationType.Standard, true);
        await RegisterUserOnEventAsync(vip, ev, new[] { day.Id }, RegistrationType.Vip, true);
        await RegisterUserOnEventAsync(org, ev, new[] { day.Id }, RegistrationType.Organizer, true);

        // GetDayPriceWithPrivilegesAsync: использует фундаментальную цену без учёта привилегированных.
        // P0 = totalCost / (A * participants_non_privileged) = 1100 / (1 * 2) = 550
        // Price = (1+0) * A * P0 = 550
        var expectedPrice = 550m;

        // Act
        var price = await _calculationService.GetDayPriceWithPrivilegesAsync(day.Id);

        // Assert
        price.Should().Be(expectedPrice);
    }

    [TestMethod]
    public async Task GetDaysPriceAsync_ShouldCalculatePriceForMultipleDays()
    {
        // Arrange
        var location = await CreateLocationAsync(cost: 100m);
        var ev = await CreateEventAsync(location, DateOnly.FromDateTime(DateTime.Today), daysCount: 2);

        var item = await CreateItemAsync("Item", 50m);
        var menu1 = await CreateMenuWithItemsAsync((item, 1)); // 50
        var menu2 = await CreateMenuWithItemsAsync((item, 2)); // 100

        var day1 = await AddDayToEventAsync(ev, menu1, "Day1", 1);
        var day2 = await AddDayToEventAsync(ev, menu2, "Day2", 2);

        // Создаём участников, которые выбрали оба дня
        var user = await CreateUserAsync("+111111111");
        await RegisterUserOnEventAsync(user, ev, new[] { day1.Id, day2.Id }, payment: true);

        // totalCost = (day1Cost + day2Cost) = (50+100)+(100+100)=350? 
        // day1Cost = menu1Cost + locationCost = 50+100=150
        // day2Cost = 100+100=200, total=350
        // A для комбинации = (cost1+cost2) / minDayCost. minDayCost = min(150,200)=150
        // A = (150+200)/150 = 350/150 ≈ 2.33333...
        // participants = 1 (только один участник выбрал именно эту комбинацию)
        // P0 = totalCost / (A * participants) = 350 / (2.33333 * 1) = 150
        // Price = (1+percent)*A*P0 = 1 * 2.33333 * 150 = 350

        // Ожидаемая цена = totalCost, так как участник один и покрывает все расходы
        var expectedPrice = 350m;

        // Act
        var price = await _calculationService.GetDaysPriceAsync(new[] { day1.Id, day2.Id });

        // Assert
        price.Should().Be(expectedPrice);
    }
}