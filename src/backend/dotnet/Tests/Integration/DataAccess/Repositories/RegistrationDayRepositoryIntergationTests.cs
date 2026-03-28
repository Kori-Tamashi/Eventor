using DataAccess.Converters;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
public class RegistrationDayRepositoryIntergationTests : DatabaseIntegrationTestBase
{
    private RegistrationDayRepository _sutRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<RegistrationDayRepository>.Instance;
        _sutRepository = new RegistrationDayRepository(DbContext!, logger);
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

    private async Task<Guid> CreateUserAsync(string phoneNumber = "+10000000000")
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

    private async Task<(Guid registrationId, Guid dayId)> CreateRegistrationWithParticipationAsync()
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

    private async Task<ParticipationDb?> GetParticipationAsync(Guid registrationId, Guid dayId)
    {
        return await DbContext!.Participations
            .FirstOrDefaultAsync(p => p.RegistrationId == registrationId && p.DayId == dayId);
    }
    
    [TestMethod]
    public async Task RemoveDayAsync_ShouldRemoveParticipation()
    {
        var (registrationId, dayId) = await CreateRegistrationWithParticipationAsync();

        var before = await GetParticipationAsync(registrationId, dayId);
        before.Should().NotBeNull();

        await _sutRepository.RemoveDayAsync(registrationId, dayId);

        var after = await GetParticipationAsync(registrationId, dayId);
        after.Should().BeNull();
    }

    [TestMethod]
    public async Task RemoveDayAsync_ShouldThrow_WhenNotFound()
    {
        var registrationId = Guid.NewGuid();
        var dayId = Guid.NewGuid();

        var act = async () =>
            await _sutRepository.RemoveDayAsync(registrationId, dayId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task AddDayAsync_ShouldAddParticipation_ForSingleRegistration()
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
        await DbContext.SaveChangesAsync();
        
        await _sutRepository.AddDayAsync(registrationId, dayId);
        
        var participation = await DbContext.Participations
            .FirstOrDefaultAsync(p => p.RegistrationId == registrationId && p.DayId == dayId);

        participation.Should().NotBeNull();
        participation!.RegistrationId.Should().Be(registrationId);
        participation.DayId.Should().Be(dayId);
    }
}