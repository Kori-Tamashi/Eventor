using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
[TestCategory("Integration")]
public class EventRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private EventRepository _sutRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<EventRepository>.Instance;
        _sutRepository = new EventRepository(DbContext!, logger);
    }
    
    private async Task<Guid> CreateLocationAsync()
    {
        var id = Guid.NewGuid();

        var locationDb = new LocationDb(
            id,
            title: "Test Location",
            description: "Test Description",
            cost: 100,
            capacity: 50);

        DbContext!.Locations.Add(locationDb); 
        await DbContext.SaveChangesAsync();

        return id; 
    }
    
    [TestMethod]
    public async Task CreateAsync_ShouldPersistEvent()
    {
        var locationId = await CreateLocationAsync();

        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Event 1")
            .WithDescription("Description")
            .WithStartDate(new DateOnly(2025, 1, 1))
            .WithDaysCount(1)
            .WithPercent(10)
            .Build();

        await _sutRepository.CreateAsync(ev);

        var result = await _sutRepository.GetByIdAsync(ev.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Event 1");
        result.LocationId.Should().Be(locationId);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnAllEvents_OrderedByDateThenId()
    {
        var locationId = await CreateLocationAsync();

        var e1 = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("B")
            .WithStartDate(new DateOnly(2025, 1, 2))
            .Build();

        var e2 = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("A")
            .WithStartDate(new DateOnly(2025, 1, 1))
            .Build();

        await _sutRepository.CreateAsync(e1);
        await _sutRepository.CreateAsync(e2);

        var result = await _sutRepository.GetAsync();

        result.Should().HaveCount(2);
        result[0].StartDate.Should().BeBefore(result[1].StartDate);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByLocationId()
    {
        var location1 = await CreateLocationAsync();
        var location2 = await CreateLocationAsync();

        var e1 = EventFixture.Default()
            .WithLocationId(location1)
            .Build();

        var e2 = EventFixture.Default()
            .WithLocationId(location2)
            .Build();

        await _sutRepository.CreateAsync(e1);
        await _sutRepository.CreateAsync(e2);

        var filter = new EventFilter
        {
            LocationId = location1
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(x => x.LocationId == location1).Should().BeTrue();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByDateRange()
    {
        var locationId = await CreateLocationAsync();

        await _sutRepository.CreateAsync(
            EventFixture.Default()
                .WithLocationId(locationId)
                .WithStartDate(new DateOnly(2025, 1, 1))
                .Build());

        await _sutRepository.CreateAsync(
            EventFixture.Default()
                .WithLocationId(locationId)
                .WithStartDate(new DateOnly(2025, 2, 1))
                .Build());

        var filter = new EventFilter
        {
            StartDateFrom = new DateOnly(2025, 1, 15),
            StartDateTo = new DateOnly(2025, 2, 15)
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result[0].StartDate.Should().Be(new DateOnly(2025, 2, 1));
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        var locationId = await CreateLocationAsync();

        await _sutRepository.CreateAsync(
            EventFixture.Default()
                .WithLocationId(locationId)
                .WithTitle("Conference")
                .Build());

        await _sutRepository.CreateAsync(
            EventFixture.Default()
                .WithLocationId(locationId)
                .WithTitle("Meetup")
                .Build());

        var filter = new EventFilter
        {
            TitleContains = "conf"
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Conference");
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        var locationId = await CreateLocationAsync();

        for (int i = 1; i <= 5; i++)
        {
            await _sutRepository.CreateAsync(
                EventFixture.Default()
                    .WithLocationId(locationId)
                    .WithStartDate(new DateOnly(2025, 1, i))
                    .Build());
        }

        var filter = new EventFilter
        {
            PageNumber = 2,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateEvent()
    {
        var locationId = await CreateLocationAsync();

        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Old")
            .WithDescription("OldDesc")
            .Build();

        await _sutRepository.CreateAsync(ev);

        var updated = EventFixture.Default()
            .WithId(ev.Id)
            .WithLocationId(locationId)
            .WithTitle("New")
            .WithDescription("NewDesc")
            .WithPercent(99)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(ev.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Description.Should().Be("NewDesc");
        result.Percent.Should().Be(99);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var locationId = await CreateLocationAsync();

        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .Build();

        var act = async () => await _sutRepository.UpdateAsync(ev);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveEvent()
    {
        var locationId = await CreateLocationAsync();

        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("ToDelete")
            .Build();

        await _sutRepository.CreateAsync(ev);

        await _sutRepository.DeleteAsync(ev.Id);

        var result = await _sutRepository.GetByIdAsync(ev.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
