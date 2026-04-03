using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Repositories;
using Domain.Filters;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Core.Fixtures;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class LocationRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistLocation()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithDescription("Test Description")
            .WithCost(250m)
            .WithCapacity(100)
            .Build();

        await repository.CreateAsync(location);

        var result = await repository.GetByIdAsync(location.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Location");
        result.Description.Should().Be("Test Description");
        result.Cost.Should().Be(250m);
        result.Capacity.Should().Be(100);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnLocation_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location = LocationFixture.Default().Build();
        await repository.CreateAsync(location);

        var result = await repository.GetByIdAsync(location.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(location.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllLocations_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location1 = LocationFixture.Default().WithTitle("Location A").Build();
        var location2 = LocationFixture.Default().WithTitle("Location B").Build();
        await repository.CreateAsync(location1);
        await repository.CreateAsync(location2);

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
        result.Select(l => l.Title).Should().Contain(new[] { "Location A", "Location B" });
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByCostRange()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location1 = LocationFixture.Default().WithCost(100m).Build();
        var location2 = LocationFixture.Default().WithCost(200m).Build();
        var location3 = LocationFixture.Default().WithCost(300m).Build();
        await repository.CreateAsync(location1);
        await repository.CreateAsync(location2);
        await repository.CreateAsync(location3);

        var filter = new LocationFilter { CostFrom = 150m, CostTo = 250m };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.First().Cost.Should().Be(200m);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByCapacityRange()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location1 = LocationFixture.Default().WithCapacity(50).Build();
        var location2 = LocationFixture.Default().WithCapacity(100).Build();
        var location3 = LocationFixture.Default().WithCapacity(150).Build();
        await repository.CreateAsync(location1);
        await repository.CreateAsync(location2);
        await repository.CreateAsync(location3);

        var filter = new LocationFilter { CapacityFrom = 80, CapacityTo = 120 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.First().Capacity.Should().Be(100);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        for (int i = 1; i <= 5; i++)
        {
            var location = LocationFixture.Default().WithTitle($"Location {i}").Build();
            await repository.CreateAsync(location);
        }

        var filter = new LocationFilter { PageNumber = 2, PageSize = 2 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
        result.Select(l => l.Title).Should().Contain(new[] { "Location 3", "Location 4" });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateLocation()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location = LocationFixture.Default()
            .WithTitle("Old Title")
            .WithDescription("Old Desc")
            .WithCost(100m)
            .WithCapacity(50)
            .Build();
        await repository.CreateAsync(location);

        var updated = LocationFixture.Default()
            .WithId(location.Id)
            .WithTitle("New Title")
            .WithDescription("New Desc")
            .WithCost(200m)
            .WithCapacity(100)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(location.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.Cost.Should().Be(200m);
        result.Capacity.Should().Be(100);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location = LocationFixture.Default().Build();

        Func<Task> act = async () => await repository.UpdateAsync(location);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveLocation()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);
        var location = LocationFixture.Default().Build();
        await repository.CreateAsync(location);

        await repository.DeleteAsync(location.Id);

        var result = await repository.GetByIdAsync(location.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new LocationRepository(context, NullLogger<LocationRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}