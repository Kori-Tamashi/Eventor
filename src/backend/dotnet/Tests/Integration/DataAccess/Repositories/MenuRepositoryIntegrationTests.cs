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
public class MenuRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private MenuRepository _sutRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<MenuRepository>.Instance;
        _sutRepository = new MenuRepository(DbContext!, logger);
    }
    
    private async Task<Guid> CreateItemAsync()
    {
        var itemId = Guid.NewGuid();

        var itemDb = new ItemDb(
            itemId,
            title: "Test Item",
            cost: 10
        );

        DbContext!.Items.Add(itemDb);
        await DbContext.SaveChangesAsync();

        return itemId;
    }
    
    private async Task CreateMenuItemAsync(
        Guid menuId, 
        Guid itemId, 
        int amount = 1)
    {
        var menuItem = new MenuItemDb(
            menuId,
            itemId,
            amount
        );

        DbContext!.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistMenu()
    {
        var menu = MenuFixture.Default()
            .WithTitle("Menu 1")
            .WithDescription("Desc")
            .Build();

        await _sutRepository.CreateAsync(menu);

        var result = await _sutRepository.GetByIdAsync(menu.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Menu 1");
        result.Description.Should().Be("Desc");
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldIncludeItems_WhenRequested()
    {
        var menu = MenuFixture.Default().Build();
        await _sutRepository.CreateAsync(menu);

        var itemId = await CreateItemAsync();
        await CreateMenuItemAsync(menu.Id, itemId);

        var result = await _sutRepository.GetByIdAsync(menu.Id, includeItems: true);

        result.Should().NotBeNull();
        result!.MenuItems.Should().NotBeEmpty();
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldIncludeItems_WhenRequested()
    {
        var menu = MenuFixture.Default().Build();
        await _sutRepository.CreateAsync(menu);

        var itemId = await CreateItemAsync();
        await CreateMenuItemAsync(menu.Id, itemId);

        var result = await _sutRepository.GetAsync(includeItems: true);

        result.Should().NotBeEmpty();
        result[0].MenuItems.Should().NotBeEmpty();
    }
   
    [TestMethod]
    public async Task GetAsync_ShouldReturnAllMenus_OrderedById()
    {
        var m1 = MenuFixture.Default().Build();
        var m2 = MenuFixture.Default().Build();

        await _sutRepository.CreateAsync(m1);
        await _sutRepository.CreateAsync(m2);

        var result = await _sutRepository.GetAsync();

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(x => x.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        await _sutRepository.CreateAsync(
            MenuFixture.Default().WithTitle("Breakfast Menu").Build());

        await _sutRepository.CreateAsync(
            MenuFixture.Default().WithTitle("Dinner Menu").Build());

        var filter = new MenuFilter
        {
            TitleContains = "break"
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Breakfast Menu");
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        for (int i = 0; i < 5; i++)
        {
            await _sutRepository.CreateAsync(
                MenuFixture.Default().WithTitle($"Menu {i}").Build());
        }

        var filter = new MenuFilter
        {
            PageNumber = 2,
            PageSize = 2
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateMenu()
    {
        var menu = MenuFixture.Default()
            .WithTitle("Old")
            .WithDescription("OldDesc")
            .Build();

        await _sutRepository.CreateAsync(menu);

        var updated = MenuFixture.Default()
            .WithId(menu.Id)
            .WithTitle("New")
            .WithDescription("NewDesc")
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(menu.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Description.Should().Be("NewDesc");
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveMenu()
    {
        var menu = MenuFixture.Default().Build();
        await _sutRepository.CreateAsync(menu);

        await _sutRepository.DeleteAsync(menu.Id);

        var result = await _sutRepository.GetByIdAsync(menu.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var act = async () => await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
