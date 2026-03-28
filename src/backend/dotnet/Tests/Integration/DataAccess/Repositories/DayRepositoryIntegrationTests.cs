using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass] 
public class DayRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private DayRepository _sutRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<DayRepository>.Instance;
        _sutRepository = new DayRepository(DbContext!, logger);
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
    
    [TestMethod]
    public async Task CreateAsync_ShouldPersistDay()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("Day 1")
            .WithSequenceNumber(1)
            .Build();

        await _sutRepository.CreateAsync(day);

        var result = await _sutRepository.GetByIdAsync(day.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Day 1");
        result.EventId.Should().Be(eventId);
        result.MenuId.Should().Be(menuId);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByEventId()
    {
        var locationId = await CreateLocationAsync();
        var event1 = await CreateEventAsync(locationId);
        var event2 = await CreateEventAsync(locationId);
        var menu1 = await CreateMenuAsync();
        var menu2 = await CreateMenuAsync();

        await _sutRepository.CreateAsync(
            DayFixture.Default().WithEventId(event1).WithMenuId(menu1).Build());

        await _sutRepository.CreateAsync(
            DayFixture.Default().WithEventId(event2).WithMenuId(menu2).Build());

        var filter = new DayFilter
        {
            EventId = event1
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(x => x.EventId == event1).Should().BeTrue();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByMenuId()
    {
        var locationId = await CreateLocationAsync();
        var event1 = await CreateEventAsync(locationId);
        var event2 = await CreateEventAsync(locationId);
        var menu1 = await CreateMenuAsync();
        var menu2 = await CreateMenuAsync();

        await _sutRepository.CreateAsync(
            DayFixture.Default().WithEventId(event1).WithMenuId(menu1).Build());

        await _sutRepository.CreateAsync(
            DayFixture.Default().WithEventId(event2).WithMenuId(menu2).Build());

        var filter = new DayFilter
        {
            MenuId = menu1
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(x => x.MenuId == menu1).Should().BeTrue();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        
        for (int i = 1; i <= 5; i++)
        {
            var menuId = await CreateMenuAsync();

            await _sutRepository.CreateAsync(
                DayFixture.Default()
                    .WithEventId(eventId)
                    .WithMenuId(menuId)
                    .WithSequenceNumber(i)
                    .Build());
        }

        var filter = new DayFilter
        {
            PageNumber = 2,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateDay()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("Old")
            .Build();

        await _sutRepository.CreateAsync(day);

        var updated = DayFixture.Default()
            .WithId(day.Id)
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle("New")
            .WithDescription("NewDesc")
            .WithSequenceNumber(2)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(day.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Description.Should().Be("NewDesc");
        result.SequenceNumber.Should().Be(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();

        var act = async () => await _sutRepository.UpdateAsync(day);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveDay()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();

        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .Build();

        await _sutRepository.CreateAsync(day);

        await _sutRepository.DeleteAsync(day.Id);

        var result = await _sutRepository.GetByIdAsync(day.Id);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
