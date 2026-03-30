using DataAccess.Repositories;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
[TestCategory("Integration")]
public class LocationRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private LocationRepository _sutRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<LocationRepository>.Instance;
        _sutRepository = new LocationRepository(DbContext!, logger);
    }
    
    [TestMethod]
    public async Task CreateAsync_ShouldSaveLocation()
    {
        var location = LocationFixture.Default().Build();

        await _sutRepository.CreateAsync(location);

        var result = await _sutRepository.GetByIdAsync(location.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be(location.Title);
        result.Description.Should().Be(location.Description);
        result.Cost.Should().Be(location.Cost);
        result.Capacity.Should().Be(location.Capacity);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnAllLocations()
    {
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("A").Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("B").Build());

        var result = await _sutRepository.GetAsync();

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Conference Hall").Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Meeting Room").Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Conference Center").Build());

        var filter = new LocationFilter
        {
            TitleContains = "Conference"
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
        result.Select(x => x.Title)
            .Should()
            .Contain(["Conference Hall", "Conference Center"]);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByCost()
    {
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCost(50).Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCost(150).Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCost(250).Build());

        var filter = new LocationFilter
        {
            CostFrom = 100,
            CostTo = 200
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().ContainSingle();
        result[0].Cost.Should().Be(150);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByCapacity()
    {
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCapacity(10).Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCapacity(50).Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithCapacity(100).Build());

        var filter = new LocationFilter
        {
            CapacityFrom = 30,
            CapacityTo = 80
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().ContainSingle();
        result[0].Capacity.Should().Be(50);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        var locations = Enumerable.Range(1, 5)
            .Select(i => LocationFixture.Default()
                .WithTitle($"Location {i}")
                .Build());

        foreach (var location in locations)
            await _sutRepository.CreateAsync(location);

        var filter = new LocationFilter
        {
            PageNumber = 2,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnOrderedByTitle()
    {
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Charlie").Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Alice").Build());
        await _sutRepository.CreateAsync(LocationFixture.Default().WithTitle("Bob").Build());

        var result = await _sutRepository.GetAsync();

        result.Select(x => x.Title)
            .Should()
            .BeInAscendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnEmpty_WhenPageOutOfRange()
    {
        var locations = Enumerable.Range(1, 3)
            .Select(i => LocationFixture.Default()
                .WithTitle($"Location {i}")
                .Build());

        foreach (var location in locations)
            await _sutRepository.CreateAsync(location);

        var filter = new LocationFilter
        {
            PageNumber = 10,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateLocation()
    {
        var location = LocationFixture.Default().Build();
        await _sutRepository.CreateAsync(location);

        var updated = LocationFixture.Default()
            .WithId(location.Id)
            .WithTitle("Updated Title")
            .WithDescription("Updated Desc")
            .WithCost(999)
            .WithCapacity(999)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(location.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.Cost.Should().Be(999);
        result.Capacity.Should().Be(999);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var location = LocationFixture.Default().Build();

        var act = async () => await _sutRepository.UpdateAsync(location);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveLocation()
    {
        var location = LocationFixture.Default().Build();
        await _sutRepository.CreateAsync(location);

        await _sutRepository.DeleteAsync(location.Id);

        var result = await _sutRepository.GetByIdAsync(location.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}