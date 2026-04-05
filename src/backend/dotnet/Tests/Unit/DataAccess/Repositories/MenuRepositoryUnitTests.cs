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
public class MenuRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    private async Task<Guid> CreateItemAsync(EventorDbContext context)
    {
        var item = ItemFixture.Default()
            .WithTitle("Test Item")
            .WithCost(50m)
            .Build();

        var itemDb = new ItemDb(item.Id, item.Title, item.Cost);
        context.Items.Add(itemDb);
        await context.SaveChangesAsync();
        return item.Id;
    }

    private async Task AddMenuItemAsync(EventorDbContext context, Guid menuId, Guid itemId, int amount)
    {
        var menuItem = new MenuItemDb(menuId, itemId, amount);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistMenu()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default()
            .WithTitle("Test Menu")
            .WithDescription("Test Description")
            .Build();

        await repository.CreateAsync(menu);

        var result = await repository.GetByIdAsync(menu.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Menu");
        result.Description.Should().Be("Test Description");
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnMenu_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default().Build();
        await repository.CreateAsync(menu);

        var result = await repository.GetByIdAsync(menu.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(menu.Id);
        result.Title.Should().Be(menu.Title);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithIncludeItems_ShouldIncludeMenuItems()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default().Build();
        await repository.CreateAsync(menu);

        var itemId = await CreateItemAsync(context);
        await AddMenuItemAsync(context, menu.Id, itemId, 5);

        var result = await repository.GetByIdAsync(menu.Id, includeItems: true);

        result.Should().NotBeNull();
        result!.MenuItems.Should().HaveCount(1);
        result.MenuItems.First().ItemId.Should().Be(itemId);
        result.MenuItems.First().Amount.Should().Be(5);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllMenus_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu1 = MenuFixture.Default().WithTitle("Menu 1").Build();
        var menu2 = MenuFixture.Default().WithTitle("Menu 2").Build();
        await repository.CreateAsync(menu1);
        await repository.CreateAsync(menu2);

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
        result.Select(m => m.Title).Should().Contain(["Menu 1", "Menu 2"]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        for (int i = 1; i <= 5; i++)
        {
            var menu = MenuFixture.Default().WithTitle($"Menu {i}").Build();
            await repository.CreateAsync(menu);
        }

        var filter = new MenuFilter { PageNumber = 2, PageSize = 2 };

        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_WithIncludeItems_ShouldIncludeMenuItems()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default().Build();
        await repository.CreateAsync(menu);

        var itemId1 = await CreateItemAsync(context);
        var itemId2 = await CreateItemAsync(context);
        await AddMenuItemAsync(context, menu.Id, itemId1, 3);
        await AddMenuItemAsync(context, menu.Id, itemId2, 7);

        var result = await repository.GetAsync(includeItems: true);

        var foundMenu = result.FirstOrDefault(m => m.Id == menu.Id);
        foundMenu.Should().NotBeNull();
        foundMenu!.MenuItems.Should().HaveCount(2);
        foundMenu.MenuItems.Should().Contain(mi => mi.ItemId == itemId1 && mi.Amount == 3);
        foundMenu.MenuItems.Should().Contain(mi => mi.ItemId == itemId2 && mi.Amount == 7);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateMenu()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default()
            .WithTitle("Old Title")
            .WithDescription("Old Description")
            .Build();
        await repository.CreateAsync(menu);

        var updated = MenuFixture.Default()
            .WithId(menu.Id)
            .WithTitle("New Title")
            .WithDescription("New Description")
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(menu.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New Description");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default().Build();

        Func<Task> act = async () => await repository.UpdateAsync(menu);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveMenu()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);
        var menu = MenuFixture.Default().Build();
        await repository.CreateAsync(menu);

        await repository.DeleteAsync(menu.Id);

        var result = await repository.GetByIdAsync(menu.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new MenuRepository(context, NullLogger<MenuRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}