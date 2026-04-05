using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Filters;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class DayRepositoryUnitTests
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

    [TestMethod]
    public async Task CreateAsync_ShouldPersistDay()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("Day 1")
            .WithSequenceNumber(1)
            .Build();

        await repository.CreateAsync(day);

        var result = await repository.GetByIdAsync(day.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Day 1");
        result.EventId.Should().Be(eventId);
        result.MenuId.Should().Be(menuId);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnDay_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();
        await repository.CreateAsync(day);

        var result = await repository.GetByIdAsync(day.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(day.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllDays_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId1 = await CreateMenuAsync(context);
        var menuId2 = await CreateMenuAsync(context);

        var day1 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId1)
            .WithSequenceNumber(1)
            .Build();
        var day2 = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId2)
            .WithSequenceNumber(2)
            .Build();

        await repository.CreateAsync(day1);
        await repository.CreateAsync(day2);

        // Дополнительная проверка, что оба дня физически присутствуют в БД
        (await repository.GetByIdAsync(day1.Id)).Should().NotBeNull();
        (await repository.GetByIdAsync(day2.Id)).Should().NotBeNull();

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByEventId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var event1 = await CreateEventAsync(context, locationId);
        var event2 = await CreateEventAsync(context, locationId);
        var menu1 = await CreateMenuAsync(context);
        var menu2 = await CreateMenuAsync(context);

        await repository.CreateAsync(DayFixture.Default()
            .WithEventId(event1)
            .WithMenuId(menu1)
            .WithSequenceNumber(1)
            .Build());
        await repository.CreateAsync(DayFixture.Default()
            .WithEventId(event2)
            .WithMenuId(menu2)
            .WithSequenceNumber(1)
            .Build());

        var filter = new DayFilter { EventId = event1 };

        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(x => x.EventId == event1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByMenuId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var event1 = await CreateEventAsync(context, locationId);
        var event2 = await CreateEventAsync(context, locationId);
        var menu1 = await CreateMenuAsync(context);
        var menu2 = await CreateMenuAsync(context);

        await repository.CreateAsync(DayFixture.Default()
            .WithEventId(event1)
            .WithMenuId(menu1)
            .WithSequenceNumber(1)
            .Build());
        await repository.CreateAsync(DayFixture.Default()
            .WithEventId(event2)
            .WithMenuId(menu2)
            .WithSequenceNumber(1)
            .Build());

        var filter = new DayFilter { MenuId = menu1 };

        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(x => x.MenuId == menu1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);

        for (int i = 1; i <= 5; i++)
        {
            var menuId = await CreateMenuAsync(context);
            await repository.CreateAsync(
                DayFixture.Default()
                    .WithEventId(eventId)
                    .WithMenuId(menuId)
                    .WithSequenceNumber(i)
                    .Build());
        }

        var filter = new DayFilter { PageNumber = 2, PageSize = 2 };

        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateDay()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("Old")
            .Build();
        await repository.CreateAsync(day);

        var updated = DayFixture.Default()
            .WithId(day.Id)
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("New")
            .WithDescription("NewDesc")
            .WithSequenceNumber(2)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(day.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Description.Should().Be("NewDesc");
        result.SequenceNumber.Should().Be(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();

        Func<Task> act = async () => await repository.UpdateAsync(day);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveDay()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var eventId = await CreateEventAsync(context, locationId);
        var menuId = await CreateMenuAsync(context);
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();
        await repository.CreateAsync(day);

        await repository.DeleteAsync(day.Id);

        var result = await repository.GetByIdAsync(day.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new DayRepository(context, NullLogger<DayRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}