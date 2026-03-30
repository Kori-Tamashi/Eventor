using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
[TestCategory("Integration")]
public class MenuItemIntegrationTests : DatabaseIntegrationTestBase
{
    private MenuItemRepository _sutRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<MenuItemRepository>.Instance;
        _sutRepository = new MenuItemRepository(DbContext!, logger);
    }
    
    private async Task<Guid> CreateMenuAsync()
    {
        var menu = new MenuDb(
            Guid.NewGuid(),
            title: "Test Menu",
            description: "Test Description"
        );

        DbContext!.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        return menu.Id;
    }

    private async Task<Guid> CreateItemAsync()
    {
        var item = new ItemDb(
            Guid.NewGuid(),
            title: "Test Item",
            cost: 10
        );

        DbContext!.Items.Add(item);
        await DbContext.SaveChangesAsync();

        return item.Id;
    }

    private async Task CreateMenuItemDbAsync(Guid menuId, Guid itemId, int amount)
    {
        var entity = new MenuItemDb(menuId, itemId, amount);

        DbContext!.MenuItems.Add(entity);
        await DbContext.SaveChangesAsync();
    }
    
    [TestMethod]
    public async Task AddAsync_ShouldPersistMenuItem()
    {
        var menuId = await CreateMenuAsync();
        var itemId = await CreateItemAsync();

        var menuItem = new MenuItem(itemId, amount: 5);

        await _sutRepository.AddAsync(menuId, menuItem);

        var entity = DbContext!.MenuItems
            .FirstOrDefault(x => x.MenuId == menuId && x.ItemId == itemId);

        entity.Should().NotBeNull();
        entity!.Amount.Should().Be(5);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateAmount()
    {
        var menuId = await CreateMenuAsync();
        var itemId = await CreateItemAsync();

        await CreateMenuItemDbAsync(menuId, itemId, amount: 1);

        var updated = new MenuItem(itemId, amount: 10);

        await _sutRepository.UpdateAsync(menuId, updated);

        var entity = DbContext!.MenuItems
            .FirstOrDefault(x => x.MenuId == menuId && x.ItemId == itemId);

        entity.Should().NotBeNull();
        entity!.Amount.Should().Be(10);
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var menuItem = new MenuItem(itemId, amount: 5);

        var act = async () => await _sutRepository.UpdateAsync(menuId, menuItem);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task RemoveAsync_ShouldDeleteMenuItem()
    {
        var menuId = await CreateMenuAsync();
        var itemId = await CreateItemAsync();

        await CreateMenuItemDbAsync(menuId, itemId, amount: 3);

        await _sutRepository.RemoveAsync(menuId, itemId);

        var entity = DbContext!.MenuItems
            .FirstOrDefault(x => x.MenuId == menuId && x.ItemId == itemId);

        entity.Should().BeNull();
    }
    
    [TestMethod]
    public async Task RemoveAsync_ShouldThrow_WhenNotFound()
    {
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var act = async () => await _sutRepository.RemoveAsync(menuId, itemId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}