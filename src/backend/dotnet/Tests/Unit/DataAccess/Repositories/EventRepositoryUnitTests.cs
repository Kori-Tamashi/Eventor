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
public class EventRepositoryUnitTests
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

    [TestMethod]
    public async Task CreateAsync_ShouldPersistEvent()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Test Event")
            .WithDescription("Test Description")
            .WithStartDate(startDate)
            .WithDaysCount(3)
            .WithPercent(10.5)
            .Build();

        await repository.CreateAsync(ev);

        var result = await repository.GetByIdAsync(ev.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Event");
        result.Description.Should().Be("Test Description");
        result.StartDate.Should().Be(startDate);
        result.LocationId.Should().Be(locationId);
        result.DaysCount.Should().Be(3);
        result.Percent.Should().Be(10.5);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEvent_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var ev = EventFixture.Default().WithLocationId(locationId).Build();
        await repository.CreateAsync(ev);

        var result = await repository.GetByIdAsync(ev.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ev.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllEvents_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var ev1 = EventFixture.Default().WithLocationId(locationId).Build();
        var ev2 = EventFixture.Default().WithLocationId(locationId).Build();
        await repository.CreateAsync(ev1);
        await repository.CreateAsync(ev2);

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByLocationId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var location1 = await CreateLocationAsync(context);
        var location2 = await CreateLocationAsync(context);
        var ev1 = EventFixture.Default().WithLocationId(location1).Build();
        var ev2 = EventFixture.Default().WithLocationId(location2).Build();
        await repository.CreateAsync(ev1);
        await repository.CreateAsync(ev2);

        var filter = new EventFilter { LocationId = location1 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(e => e.LocationId == location1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByStartDateRange()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var datePast = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

        var evPast = EventFixture.Default().WithLocationId(locationId).WithStartDate(datePast).Build();
        var evNow = EventFixture.Default().WithLocationId(locationId).WithStartDate(dateNow).Build();
        var evFuture = EventFixture.Default().WithLocationId(locationId).WithStartDate(dateFuture).Build();

        await repository.CreateAsync(evPast);
        await repository.CreateAsync(evNow);
        await repository.CreateAsync(evFuture);

        var filter = new EventFilter
        {
            StartDateFrom = dateNow,
            StartDateTo = dateFuture
        };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
        result.Select(e => e.StartDate).Should().Contain(dateNow);
        result.Select(e => e.StartDate).Should().Contain(dateFuture);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateEvent()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Old Title")
            .WithDescription("Old Description")
            .Build();
        await repository.CreateAsync(ev);

        var updated = EventFixture.Default()
            .WithId(ev.Id)
            .WithLocationId(locationId)
            .WithTitle("New Title")
            .WithDescription("New Description")
            .WithStartDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)))
            .WithDaysCount(7)
            .WithPercent(20.0)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(ev.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New Description");
        result.StartDate.Should().Be(updated.StartDate);
        result.DaysCount.Should().Be(7);
        result.Percent.Should().Be(20.0);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var ev = EventFixture.Default().WithLocationId(locationId).Build();

        Func<Task> act = async () => await repository.UpdateAsync(ev);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveEvent()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var locationId = await CreateLocationAsync(context);
        var ev = EventFixture.Default().WithLocationId(locationId).Build();
        await repository.CreateAsync(ev);

        await repository.DeleteAsync(ev.Id);

        var result = await repository.GetByIdAsync(ev.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EventRepository(context, NullLogger<EventRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}