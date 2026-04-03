using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using DataAccess.Converters;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class RegistrationDayRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    private async Task<Guid> CreateLocationAsync(EventorDbContext context)
    {
        var location = new LocationDb(
            Guid.NewGuid(),
            "Test Location",
            "Description",
            100,
            50
        );
        context.Locations.Add(location);
        await context.SaveChangesAsync();
        return location.Id;
    }

    private async Task<Guid> CreateEventAsync(EventorDbContext context, Guid locationId)
    {
        var ev = new EventDb(
            Guid.NewGuid(),
            "Test Event",
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            locationId,
            1,
            0);
        context.Events.Add(ev);
        await context.SaveChangesAsync();
        return ev.Id;
    }

    private async Task<Guid> CreateMenuAsync(EventorDbContext context)
    {
        var menu = new MenuDb(
            Guid.NewGuid(),
            "Test Menu",
            "Description"
        );
        context.Menus.Add(menu);
        await context.SaveChangesAsync();
        return menu.Id;
    }

    private async Task<Guid> CreateDayAsync(EventorDbContext context, Guid eventId, Guid menuId, int sequenceNumber = 1)
    {
        var day = new DayDb(
            Guid.NewGuid(),
            eventId,
            menuId,
            $"Day {sequenceNumber}",
            sequenceNumber,
            "Description"
        );
        context.Days.Add(day);
        await context.SaveChangesAsync();
        return day.Id;
    }

    private async Task<Guid> CreateUserAsync(EventorDbContext context)
    {
        var user = new UserDb(
            Guid.NewGuid(),
            "Test User",
            "+1234567890",
            GenderConverter.ToDb(Domain.Enums.Gender.Male),
            UserRoleConverter.ToDb(Domain.Enums.UserRole.User),
            "hash"
        );
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private async Task<Guid> CreateRegistrationAsync(EventorDbContext context, Guid eventId, Guid userId)
    {
        var registration = new RegistrationDb(
            Guid.NewGuid(),
            eventId,
            userId,
            RegistrationTypeConverter.ToDb(Domain.Enums.RegistrationType.Standard),
            false
        );
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();
        return registration.Id;
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldAddParticipation()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var dayId = await CreateDayAsync(context, eventId, menuId);
        var userId = await CreateUserAsync(context);
        var registrationId = await CreateRegistrationAsync(context, eventId, userId);

        await repository.AddDayAsync(registrationId, dayId);

        var participation = await context.Participations
            .FirstOrDefaultAsync(p => p.RegistrationId == registrationId && p.DayId == dayId);
        participation.Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldAllowMultipleDaysForSameRegistration()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day1Id = await CreateDayAsync(context, eventId, menuId, 1);
        var day2Id = await CreateDayAsync(context, eventId, menuId, 2);
        var userId = await CreateUserAsync(context);
        var registrationId = await CreateRegistrationAsync(context, eventId, userId);

        await repository.AddDayAsync(registrationId, day1Id);
        await repository.AddDayAsync(registrationId, day2Id);

        var participations = await context.Participations
            .Where(p => p.RegistrationId == registrationId)
            .ToListAsync();
        participations.Should().HaveCount(2);
        participations.Select(p => p.DayId).Should().Contain(new[] { day1Id, day2Id });
    }

    [TestMethod]
    public async Task AddDayAsync_ShouldThrow_WhenDuplicateAdded()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var dayId = await CreateDayAsync(context, eventId, menuId);
        var userId = await CreateUserAsync(context);
        var registrationId = await CreateRegistrationAsync(context, eventId, userId);

        await repository.AddDayAsync(registrationId, dayId);

        Func<Task> act = async () => await repository.AddDayAsync(registrationId, dayId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already being tracked*");
    }

    [TestMethod]
    public async Task RemoveDayAsync_ShouldRemoveParticipation()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var dayId = await CreateDayAsync(context, eventId, menuId);
        var userId = await CreateUserAsync(context);
        var registrationId = await CreateRegistrationAsync(context, eventId, userId);
        await repository.AddDayAsync(registrationId, dayId);

        await repository.RemoveDayAsync(registrationId, dayId);

        var participation = await context.Participations
            .FirstOrDefaultAsync(p => p.RegistrationId == registrationId && p.DayId == dayId);
        participation.Should().BeNull();
    }

    [TestMethod]
    public async Task RemoveDayAsync_ShouldThrow_WhenParticipationNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var registrationId = Guid.NewGuid();
        var dayId = Guid.NewGuid();

        Func<Task> act = async () => await repository.RemoveDayAsync(registrationId, dayId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{registrationId}*{dayId}*");
    }

    [TestMethod]
    public async Task RemoveDayAsync_ShouldNotAffectOtherParticipations()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationDayRepository(context, NullLogger<RegistrationDayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day1Id = await CreateDayAsync(context, eventId, menuId, 1);
        var day2Id = await CreateDayAsync(context, eventId, menuId, 2);
        var userId = await CreateUserAsync(context);
        var registrationId = await CreateRegistrationAsync(context, eventId, userId);
        await repository.AddDayAsync(registrationId, day1Id);
        await repository.AddDayAsync(registrationId, day2Id);

        await repository.RemoveDayAsync(registrationId, day1Id);

        var remaining = await context.Participations
            .Where(p => p.RegistrationId == registrationId)
            .ToListAsync();
        remaining.Should().HaveCount(1);
        remaining.Single().DayId.Should().Be(day2Id);
    }
}