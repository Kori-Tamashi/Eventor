using DataAccess.Context;
using DataAccess.Repositories;
using Application.Services;
using Domain.Models;
using Domain.Enums;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;
using Eventor.Services.Exceptions;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class CalculationSupportServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private CalculationSupportService _sutService = null!;

    private DayRepository _dayRepository = null!;
    private EventRepository _eventRepository = null!;
    private LocationRepository _locationRepository = null!;
    private MenuRepository _menuRepository = null!;
    private ItemRepository _itemRepository = null!;
    private RegistrationRepository _registrationRepository = null!;
    private RegistrationDayRepository _registrationDayRepository = null!;

    private DayService _dayService = null!;
    private EventService _eventService = null!;
    private LocationService _locationService = null!;
    private MenuService _menuService = null!;
    private ItemService _itemService = null!;
    private RegistrationService _registrationService = null!;

    [TestInitialize]
    public void Setup()
    {
        var loggerFactory = NullLoggerFactory.Instance;

        _dayRepository = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        _eventRepository = new EventRepository(DbContext!, NullLogger<EventRepository>.Instance);
        _locationRepository = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        _menuRepository = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        _itemRepository = new ItemRepository(DbContext!, NullLogger<ItemRepository>.Instance);
        _registrationRepository = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        _registrationDayRepository = new RegistrationDayRepository(DbContext!, NullLogger<RegistrationDayRepository>.Instance);

        _dayService = new DayService(_dayRepository);
        _eventService = new EventService(_eventRepository, _registrationRepository, _dayRepository);
        _locationService = new LocationService(_locationRepository);
        _menuService = new MenuService(_menuRepository, new MenuItemRepository(DbContext!, NullLogger<MenuItemRepository>.Instance));
        _itemService = new ItemService(_itemRepository);
        _registrationService = new RegistrationService(_registrationRepository, _registrationDayRepository);

        _sutService = new CalculationSupportService(
            _itemService,
            _menuService,
            _dayService,
            _eventService,
            _locationService,
            _registrationService);
    }

    #region Helper Methods

    private async Task<(Guid locationId, Guid eventId, List<Guid> dayIds, List<Guid> menuIds, List<Guid> itemIds)>
        CreateFullEventAsync(int daysCount = 2)
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithCost(1000m)
            .WithCapacity(100)
            .Build();
        await _locationService.CreateAsync(location);

        var ev = EventFixture.Default()
            .WithLocationId(location.Id)
            .WithTitle("Test Event")
            .WithDaysCount(daysCount)
            .WithPercent(10)
            .Build();
        await _eventService.CreateAsync(ev);

        var dayIds = new List<Guid>();
        var menuIds = new List<Guid>();
        var itemIds = new List<Guid>();

        for (int i = 1; i <= daysCount; i++)
        {
            var menu = MenuFixture.Default()
                .WithTitle($"Menu Day {i}")
                .Build();
            await _menuService.CreateAsync(menu);
            menuIds.Add(menu.Id);

            var day = DayFixture.Default()
                .WithEventId(ev.Id)
                .WithMenuId(menu.Id)
                .WithTitle($"Day {i}")
                .WithSequenceNumber(i)
                .Build();
            await _dayService.CreateAsync(day);
            dayIds.Add(day.Id);
        }

        // Create some items and add to menus
        for (int i = 1; i <= 3; i++)
        {
            var item = ItemFixture.Default()
                .WithTitle($"Item {i}")
                .WithCost(50m * i)
                .Build();
            await _itemService.CreateAsync(item);
            itemIds.Add(item.Id);

            // Add to each menu
            foreach (var menuId in menuIds)
            {
                await _menuService.AddItemAsync(menuId, item.Id, amount: i);
            }
        }

        return (location.Id, ev.Id, dayIds, menuIds, itemIds);
    }

    private async Task<Guid> CreateUserAsync(string phone = "+1234567890")
    {
        var user = UserFixture.Default()
            .WithPhone(phone)
            .Build();
        await new UserRepository(DbContext!, NullLogger<UserRepository>.Instance).CreateAsync(user);
        return user.Id;
    }

    private async Task<Guid> CreateRegistrationAsync(Guid eventId, Guid userId, RegistrationType type, bool payment, List<Guid> dayIds)
    {
        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(type)
            .WithPayment(payment)
            .Build();

        await _registrationService.CreateAsync(registration, dayIds);
        return registration.Id;
    }

    #endregion

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldReturnSumOfMenuAndLocationCost()
    {
        // Arrange
        var (_, _, dayIds, menuIds, _) = await CreateFullEventAsync(1);
        var dayId = dayIds[0];
        var menuId = menuIds[0];

        // Предметы уже добавлены в меню с количеством i (1,2,3) и стоимостью 50*i
        // Стоимость меню = 50*1*1 + 50*2*2 + 50*3*3 = 50 + 200 + 450 = 700
        // Стоимость локации = 1000
        // Ожидаем 1700

        // Act
        var result = await _sutService.GetSingleDayCostAsync(dayId);

        // Assert
        result.Should().Be(700m + 1000m);
    }

    [TestMethod]
    public async Task GetSingleDayCostAsync_ShouldThrow_WhenDayNotFound()
    {
        // Arrange
        var nonExistentDayId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.GetSingleDayCostAsync(nonExistentDayId);

        // Assert
        await act.Should().ThrowAsync<EconomyServiceException>()
            .WithMessage($"Day '{nonExistentDayId}' was not found.");
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldCalculateCorrectCoefficient()
    {
        // Arrange
        var (_, _, dayIds, _, _) = await CreateFullEventAsync(2);
        var day1 = await _dayService.GetByIdAsync(dayIds[0]);
        var day2 = await _dayService.GetByIdAsync(dayIds[1]);

        // Стоимость дня = menuCost + locationCost (1000)
        // MenuCost для обоих дней одинакова (700) -> стоимость дня = 1700
        // Коэффициент для одного дня = 1700 / min(1700,1700) = 1
        // Для двух дней = (1700+1700) / 1700 = 2

        // Act
        var coeffSingle = await _sutService.GetDayCoefficientAsync(new[] { dayIds[0] });
        var coeffBoth = await _sutService.GetDayCoefficientAsync(dayIds);

        // Assert
        coeffSingle.Should().Be(1);
        coeffBoth.Should().Be(2);
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldReturnZero_WhenEmptyList()
    {
        // Act
        var result = await _sutService.GetDayCoefficientAsync(Array.Empty<Guid>());

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public async Task GetDayCoefficientAsync_ShouldThrow_WhenDayFromDifferentEvent()
    {
        // Arrange
        var (_, _, dayIds1, _, _) = await CreateFullEventAsync(1);
        var (_, _, dayIds2, _, _) = await CreateFullEventAsync(1);

        var mixed = new[] { dayIds1[0], dayIds2[0] };

        // Act
        Func<Task> act = async () => await _sutService.GetDayCoefficientAsync(mixed);

        // Assert
        await act.Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("*do not belong to the event*");
    }

    [TestMethod]
    public async Task EnsureDaysFromSameEventAsync_ShouldNotThrow_WhenSameEvent()
    {
        // Arrange
        var (_, _, dayIds, _, _) = await CreateFullEventAsync(2);

        // Act
        Func<Task> act = async () => await _sutService.EnsureDaysFromSameEventAsync(dayIds);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task EnsureDaysFromSameEventAsync_ShouldThrow_WhenDifferentEvents()
    {
        // Arrange
        var (_, _, dayIds1, _, _) = await CreateFullEventAsync(1);
        var (_, _, dayIds2, _, _) = await CreateFullEventAsync(1);
        var mixed = new[] { dayIds1[0], dayIds2[0] };

        // Act
        Func<Task> act = async () => await _sutService.EnsureDaysFromSameEventAsync(mixed);

        // Assert
        await act.Should().ThrowAsync<EconomyServiceException>()
            .WithMessage("*same event*");
    }

    [TestMethod]
    public async Task GetParticipantsByDayAsync_ShouldCountOnlyPaidRegistrations()
    {
        // Arrange
        var (_, eventId, dayIds, _, _) = await CreateFullEventAsync(1);
        var dayId = dayIds[0];
        var userId1 = await CreateUserAsync("+111");
        var userId2 = await CreateUserAsync("+222");
        var userId3 = await CreateUserAsync("+333");

        await CreateRegistrationAsync(eventId, userId1, RegistrationType.Standard, payment: true, new List<Guid> { dayId });
        await CreateRegistrationAsync(eventId, userId2, RegistrationType.Standard, payment: false, new List<Guid> { dayId });
        await CreateRegistrationAsync(eventId, userId3, RegistrationType.Vip, payment: true, new List<Guid> { dayId });

        // Act
        var resultIncludePrivileged = await _sutService.GetParticipantsByDayAsync(dayId, includePrivileged: true);
        var resultExcludePrivileged = await _sutService.GetParticipantsByDayAsync(dayId, includePrivileged: false);

        // Assert
        resultIncludePrivileged.Should().Be(2); // standard paid + vip paid
        resultExcludePrivileged.Should().Be(1); // only standard paid
    }

    [TestMethod]
    public async Task GetParticipantsCountExactAsync_ShouldCountRegistrationsWithExactlyThoseDays()
    {
        // Arrange
        var (_, eventId, dayIds, _, _) = await CreateFullEventAsync(2);
        var day1 = dayIds[0];
        var day2 = dayIds[1];

        var user1 = await CreateUserAsync("+111");
        var user2 = await CreateUserAsync("+222");
        var user3 = await CreateUserAsync("+333");
        var user4 = await CreateUserAsync("+444");

        // Только день 1
        await CreateRegistrationAsync(eventId, user1, RegistrationType.Standard, true, new List<Guid> { day1 });
        // Только день 2
        await CreateRegistrationAsync(eventId, user2, RegistrationType.Standard, true, new List<Guid> { day2 });
        // Оба дня
        await CreateRegistrationAsync(eventId, user3, RegistrationType.Standard, true, new List<Guid> { day1, day2 });
        // День 1 + день 2 (дубликат комбинации)
        await CreateRegistrationAsync(eventId, user4, RegistrationType.Vip, true, new List<Guid> { day1, day2 });

        // Act
        var countDay1Only = await _sutService.GetParticipantsCountExactAsync(new[] { day1 }, includePrivileged: true);
        var countDay2Only = await _sutService.GetParticipantsCountExactAsync(new[] { day2 }, includePrivileged: true);
        var countBoth = await _sutService.GetParticipantsCountExactAsync(new[] { day1, day2 }, includePrivileged: true);
        var countBothExcludeVip = await _sutService.GetParticipantsCountExactAsync(new[] { day1, day2 }, includePrivileged: false);

        // Assert
        countDay1Only.Should().Be(1);
        countDay2Only.Should().Be(1);
        countBoth.Should().Be(2); // user3 + user4
        countBothExcludeVip.Should().Be(1); // только user3
    }

    [TestMethod]
    public async Task GetCurrentDayCombinationsAsync_ShouldReturnUniqueCombinations()
    {
        // Arrange
        var (_, eventId, dayIds, _, _) = await CreateFullEventAsync(2);
        var day1 = dayIds[0];
        var day2 = dayIds[1];

        var user1 = await CreateUserAsync("+111");
        var user2 = await CreateUserAsync("+222");
        var user3 = await CreateUserAsync("+333"); // VIP

        // Только день 1 (Standard)
        await CreateRegistrationAsync(eventId, user1, RegistrationType.Standard, true, new List<Guid> { day1 });
        // Только день 2 (Standard)
        await CreateRegistrationAsync(eventId, user2, RegistrationType.Standard, true, new List<Guid> { day2 });
        // Оба дня (VIP) – уникальная комбинация
        await CreateRegistrationAsync(eventId, user3, RegistrationType.Vip, true, new List<Guid> { day1, day2 });

        // Act
        var combinationsInclude = await _sutService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: true);
        var combinationsExclude = await _sutService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: false);

        // Assert
        combinationsInclude.Should().HaveCount(3); // [day1], [day2], [day1,day2]
        combinationsExclude.Should().HaveCount(2); // [day1], [day2]  (VIP excluded)
    }

    [TestMethod]
    public async Task GetCurrentDayCombinationsAsync_ShouldReturnEmpty_WhenNoPaidRegistrations()
    {
        // Arrange
        var (_, eventId, dayIds, _, _) = await CreateFullEventAsync(1);
        var dayId = dayIds[0];
        var user = await CreateUserAsync("+111");

        // Registration without payment
        await CreateRegistrationAsync(eventId, user, RegistrationType.Standard, payment: false, new List<Guid> { dayId });

        // Act
        var result = await _sutService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: true);

        // Assert
        result.Should().BeEmpty();
    }
}