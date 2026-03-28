using DataAccess.Converters;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
public class RegistrationRepositoryIntergationTests : DatabaseIntegrationTestBase
{
    private RegistrationRepository _sutRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<RegistrationRepository>.Instance;
        _sutRepository = new RegistrationRepository(DbContext!, logger);
    }
    
    private async Task<Guid> CreateLocationAsync()
    {
        var location = new LocationDb(
            Guid.NewGuid(),
            "Test Location",
            "Description",
            100,
            50
        );

        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        return location.Id;
    }

    private async Task<Guid> CreateEventAsync(Guid locationId)
    {
        var ev = new EventDb(
            Guid.NewGuid(),
            "Test Event",
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            locationId,
            1,
            0);

        DbContext!.Events.Add(ev);
        await DbContext.SaveChangesAsync();

        return ev.Id;
    }

    private async Task<Guid> CreateMenuAsync()
    {
        var menu = new MenuDb(
            Guid.NewGuid(),
            "Test Menu",
            "Description"
        );

        DbContext!.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        return menu.Id;
    }

    private async Task<Guid> CreateDayAsync(Guid eventId, Guid menuId)
    {
        var day = new DayDb(
            Guid.NewGuid(),
            eventId,
            menuId,
            "Day",
            1,
            "Description"
        );

        DbContext!.Days.Add(day);
        await DbContext.SaveChangesAsync();

        return day.Id;
    }
    
    private async Task<Guid> CreateUserAsync(String phoneNumber="+10000000000")
    {
        var user = new UserDb(
            Guid.NewGuid(),
            name: "Test User",
            phone: phoneNumber,
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "hashed_password"
        );

        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();
        
        return user.Id;
    }
    
    private async Task<(
        Guid registrationId, 
        Guid dayId)> CreateRegistrationWithParticipationAsync()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();
        var dayId = await CreateDayAsync(eventId, menuId);

        var registrationId = Guid.NewGuid();

        var registration = new RegistrationDb(
            registrationId,
            eventId,
            userId,
            RegistrationTypeConverter.ToDb(RegistrationType.Standard),
            true
        );

        DbContext!.Registrations.Add(registration);

        var participation = new ParticipationDb(dayId, registrationId);

        DbContext.Participations.Add(participation);

        await DbContext.SaveChangesAsync();

        return (registrationId, dayId);
    }
    
    [TestMethod]
    public async Task CreateAsync_ShouldPersistRegistration()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var userId = await CreateUserAsync();

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithPayment(true)
            .Build();

        await _sutRepository.CreateAsync(registration);

        var result = await _sutRepository.GetByIdAsync(registration.Id);

        result.Should().NotBeNull();
        result!.EventId.Should().Be(eventId);
        result!.UserId.Should().Be(userId);
        result.Payment.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldIncludeDays_WhenRequested()
    {
        var (registrationId, 
            dayId) = await CreateRegistrationWithParticipationAsync();

        var result = await _sutRepository.GetByIdAsync(
            registrationId, 
            includeDays: true);

        result.Should().NotBeNull();
        result!.Days.Should().NotBeNull();
        result.Days.Should().HaveCount(1);
        result.Days.First().Id.Should().Be(dayId);
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByEventId()
    {
        var location1 = await CreateLocationAsync();
        var event1 = await CreateEventAsync(location1);
        var user1 = await CreateUserAsync();
        
        var location2 = await CreateLocationAsync();
        var event2 = await CreateEventAsync(location2);
        var user2 = await CreateUserAsync(phoneNumber: "+10000000001");

        var reg1 = RegistrationFixture.Default()
            .WithUserId(user1).WithEventId(event1).Build();
        var reg2 = RegistrationFixture.Default()
            .WithUserId(user2).WithEventId(event2).Build();

        await _sutRepository.CreateAsync(reg1);
        await _sutRepository.CreateAsync(reg2);

        var filter = new RegistrationFilter
        {
            EventId = event1
        };

        var result = await _sutRepository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.All(r => r.EventId == event1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByUserId()
    {
        var location1 = await CreateLocationAsync();
        var event1 = await CreateEventAsync(location1);
        var location2 = await CreateLocationAsync();
        var event2 = await CreateEventAsync(location2);
        var user = await CreateUserAsync();

        var reg1 = RegistrationFixture.Default()
            .WithUserId(user).WithEventId(event1).Build();
        var reg2 = RegistrationFixture.Default()
            .WithUserId(user).WithEventId(event2).Build();
        
        await _sutRepository.CreateAsync(reg1);
        await _sutRepository.CreateAsync(reg2);

        var filter = new RegistrationFilter
        {
            UserId = user
        };

        var result = await _sutRepository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(2);
        result.All(r => r.UserId == user).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByPayment()
    {
        var location1 = await CreateLocationAsync();
        var event1 = await CreateEventAsync(location1);
        var location2 = await CreateLocationAsync();
        var event2 = await CreateEventAsync(location2);
        var user = await CreateUserAsync();

        var reg1 = RegistrationFixture.Default()
            .WithUserId(user)
            .WithEventId(event1)
            .WithPayment(true)
            .Build();
        var reg2 = RegistrationFixture.Default()
            .WithUserId(user)
            .WithEventId(event2)
            .WithPayment(false)
            .Build();

        await _sutRepository.CreateAsync(reg1);
        await _sutRepository.CreateAsync(reg2);

        var filter = new RegistrationFilter
        {
            Payment = true
        };

        var result = await _sutRepository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.First().Payment.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateRegistration()
    {
        var location1 = await CreateLocationAsync();
        var event1 = await CreateEventAsync(location1);
        var user1 = await CreateUserAsync();
        
        var location2 = await CreateLocationAsync();
        var event2 = await CreateEventAsync(location2);
        var user2 = await CreateUserAsync(phoneNumber: "+10000000001");

        var registration = RegistrationFixture.Default()
            .WithEventId(event1)
            .WithUserId(user1)
            .WithPayment(true)
            .Build();

        await _sutRepository.CreateAsync(registration);

        var updated = RegistrationFixture.Default()
            .WithId(registration.Id)
            .WithEventId(event2)
            .WithUserId(user2)
            .WithPayment(false)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(registration.Id);

        result.Should().NotBeNull();
        result!.Payment.Should().BeFalse();
        result.EventId.Should().Be(event2);
        result.UserId.Should().Be(user2);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveRegistration()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var userId = await CreateUserAsync();

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithPayment(true)
            .Build();

        await _sutRepository.CreateAsync(registration);

        await _sutRepository.DeleteAsync(registration.Id);

        var result = await _sutRepository.GetByIdAsync(registration.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByEventIdAndUserId()
    {
        var location1 = await CreateLocationAsync();
        var event1 = await CreateEventAsync(location1);
        var location2 = await CreateLocationAsync();
        var event2 = await CreateEventAsync(location2);

        var user = await CreateUserAsync();

        var reg1 = RegistrationFixture.Default()
            .WithUserId(user)
            .WithEventId(event1)
            .Build();

        var reg2 = RegistrationFixture.Default()
            .WithUserId(user)
            .WithEventId(event2)
            .Build();

        await _sutRepository.CreateAsync(reg1);
        await _sutRepository.CreateAsync(reg2);

        var filter = new RegistrationFilter
        {
            EventId = event1,
            UserId = user
        };

        var result = await _sutRepository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.First().EventId.Should().Be(event1);
        result.First().UserId.Should().Be(user);
    }
    
    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldReturnEmpty_WhenNoMatches()
    {
        var location = await CreateLocationAsync();
        var eventId = await CreateEventAsync(location);
        var user = await CreateUserAsync();

        var registration = RegistrationFixture.Default()
            .WithUserId(user)
            .WithEventId(eventId)
            .Build();

        await _sutRepository.CreateAsync(registration);

        var filter = new RegistrationFilter
        {
            EventId = Guid.NewGuid() // несуществующий
        };

        var result = await _sutRepository.GetRegistrationsAsync(filter);

        result.Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldNotIncludeDays_WhenIncludeDaysFalse()
    {
        var (registrationId, _) = await CreateRegistrationWithParticipationAsync();

        var result = await _sutRepository.GetByIdAsync(
            registrationId,
            includeDays: false);

        result.Should().NotBeNull();
        result!.Days.Should().BeNullOrEmpty();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var location = await CreateLocationAsync();
        var eventId = await CreateEventAsync(location);
        var userId = await CreateUserAsync();

        var registration = RegistrationFixture.Default()
            .WithId(Guid.NewGuid())
            .WithEventId(eventId)
            .WithUserId(userId)
            .Build();

        Func<Task> act = async () => await _sutRepository.UpdateAsync(registration);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        Func<Task> act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
