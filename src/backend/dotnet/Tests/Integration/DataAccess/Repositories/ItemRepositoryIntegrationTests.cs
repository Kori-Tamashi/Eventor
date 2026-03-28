using DataAccess.Repositories;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
public class ItemRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private ItemRepository _sutRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<ItemRepository>.Instance;
        _sutRepository = new ItemRepository(DbContext!, logger);
    }
    
    [TestMethod]
    public async Task CreateAsync_ShouldSaveItem()
    {
        var item = ItemFixture.Default().Build();

        await _sutRepository.CreateAsync(item);

        var result = await _sutRepository.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be(item.Title);
        result.Cost.Should().Be(item.Cost);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnAllItems()
    {
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("A").Build());
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("B").Build());

        var result = await _sutRepository.GetAsync();

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Apple").Build());
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Banana").Build());
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Pineapple").Build());

        var filter = new ItemFilter
        {
            TitleContains = "apple"
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
        result.Select(x => x.Title)
            .Should()
            .Contain(["Apple", "Pineapple"]);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        var items = Enumerable.Range(1, 5)
            .Select(i => ItemFixture.Default()
                .WithTitle($"Item {i}")
                .Build());

        foreach (var item in items)
            await _sutRepository.CreateAsync(item);

        var filter = new ItemFilter
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
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Вода").Build());
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Кола").Build());
        await _sutRepository.CreateAsync(ItemFixture.Default().WithTitle("Сыр").Build());

        var result = await _sutRepository.GetAsync();

        result.Select(x => x.Title)
            .Should()
            .BeInAscendingOrder();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnEmpty_WhenPageOutOfRange()
    {
        var items = Enumerable.Range(1, 3)
            .Select(i => ItemFixture.Default()
                .WithTitle($"Item {i}")
                .Build());

        foreach (var item in items)
            await _sutRepository.CreateAsync(item);

        var filter = new ItemFilter
        {
            PageNumber = 10,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateItem()
    {
        var item = ItemFixture.Default().Build();
        await _sutRepository.CreateAsync(item);

        var updated = ItemFixture.Default()
            .WithId(item.Id)
            .WithTitle("Updated Title")
            .WithCost(999)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Cost.Should().Be(999);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var item = ItemFixture.Default().Build();

        var act = async () => await _sutRepository.UpdateAsync(item);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        var item = ItemFixture.Default().Build();
        await _sutRepository.CreateAsync(item);

        await _sutRepository.DeleteAsync(item.Id);

        var result = await _sutRepository.GetByIdAsync(item.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}