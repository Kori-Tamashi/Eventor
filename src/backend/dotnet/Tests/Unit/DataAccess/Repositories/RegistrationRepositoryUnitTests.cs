using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAccess.Converters;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class RegistrationRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new EventorDbContext(options);
    }

    private EventorDbContext CreateInMemoryContext()
    {
        return CreateInMemoryContext(Guid.NewGuid().ToString());
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

    private async Task<Guid> CreateUserAsync(EventorDbContext context)
    {
        var user = new UserDb(
            Guid.NewGuid(),
            "Test User",
            "+1234567890",
            GenderConverter.ToDb(Gender.Male),
            UserRoleConverter.ToDb(UserRole.User),
            "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
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

    private async Task<Guid> CreateDayAsync(EventorDbContext context, Guid eventId, Guid menuId, int sequenceNumber)
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

    [TestMethod]
    public async Task CreateAsync_ShouldPersistRegistration()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Standard)
            .WithPayment(false)
            .Build();

        await repository.CreateAsync(registration);

        var result = await repository.GetByIdAsync(registration.Id);
        result.Should().NotBeNull();
        result!.EventId.Should().Be(eventId);
        result.UserId.Should().Be(userId);
        result.Type.Should().Be(RegistrationType.Standard);
        result.Payment.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnRegistration_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);
        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .Build();
        await repository.CreateAsync(registration);

        var result = await repository.GetByIdAsync(registration.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(registration.Id);
    }


    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldReturnAllRegistrations_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId1 = await CreateUserAsync(context);
        var userId2 = await CreateUserAsync(context);

        var registration1 = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId1)
            .Build();
        var registration2 = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId2)
            .Build();

        await repository.CreateAsync(registration1);
        await repository.CreateAsync(registration2);

        var result = await repository.GetRegistrationsAsync();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByEventId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var event1 = await CreateEventAsync(context, locationId);
        var event2 = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(event1)
            .WithUserId(userId)
            .Build());
        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(event2)
            .WithUserId(userId)
            .Build());

        var filter = new RegistrationFilter { EventId = event1 };
        var result = await repository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.All(r => r.EventId == event1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByUserId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var user1 = await CreateUserAsync(context);
        var user2 = await CreateUserAsync(context);

        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(user1)
            .Build());
        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(user2)
            .Build());

        var filter = new RegistrationFilter { UserId = user1 };
        var result = await repository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.All(r => r.UserId == user1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByType()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Standard)
            .Build());
        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Vip)
            .Build());

        var filter = new RegistrationFilter { Type = RegistrationType.Standard };
        var result = await repository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.All(r => r.Type == RegistrationType.Standard).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldFilterByPayment()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithPayment(false)
            .Build());
        await repository.CreateAsync(RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithPayment(true)
            .Build());

        var filter = new RegistrationFilter { Payment = true };
        var result = await repository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(1);
        result.All(r => r.Payment).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetRegistrationsAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);

        for (int i = 1; i <= 5; i++)
        {
            var userId = await CreateUserAsync(context);
            await repository.CreateAsync(RegistrationFixture.Default()
                .WithEventId(eventId)
                .WithUserId(userId)
                .Build());
        }

        var filter = new RegistrationFilter { PageNumber = 2, PageSize = 2 };
        var result = await repository.GetRegistrationsAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateRegistration()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Standard)
            .WithPayment(false)
            .Build();
        await repository.CreateAsync(registration);

        var updated = RegistrationFixture.Default()
            .WithId(registration.Id)
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Vip)
            .WithPayment(true)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(registration.Id);
        result.Should().NotBeNull();
        result!.Type.Should().Be(RegistrationType.Vip);
        result.Payment.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .Build();

        Func<Task> act = async () => await repository.UpdateAsync(registration);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveRegistration()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var userId = await CreateUserAsync(context);

        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .Build();
        await repository.CreateAsync(registration);

        await repository.DeleteAsync(registration.Id);

        var result = await repository.GetByIdAsync(registration.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new RegistrationRepository(context, NullLogger<RegistrationRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}