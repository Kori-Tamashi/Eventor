using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class MenuItemRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    private async Task<Guid> CreateMenuAsync(EventorDbContext context)
    {
        var menu = MenuFixture.Default().Build();
        var menuDb = new MenuDb(menu.Id, menu.Title, menu.Description);
        context.Menus.Add(menuDb);
        await context.SaveChangesAsync();
        return menu.Id;
    }

    private async Task<Guid> CreateItemAsync(EventorDbContext context)
    {
        var item = ItemFixture.Default().Build();
        var itemDb = new ItemDb(item.Id, item.Title, item.Cost);
        context.Items.Add(itemDb);
        await context.SaveChangesAsync();
        return item.Id;
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddMenuItem()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = await CreateMenuAsync(context);
        var itemId = await CreateItemAsync(context);
        var menuItem = new MenuItem(itemId, 5);

        // Act
        await repository.AddAsync(menuId, menuItem);

        // Assert
        var saved = await context.MenuItems.FirstOrDefaultAsync(mi => mi.MenuId == menuId && mi.ItemId == itemId);
        saved.Should().NotBeNull();
        saved!.Amount.Should().Be(5);
    }

    [TestMethod]
    public async Task AddAsync_ShouldThrow_WhenDuplicate()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = await CreateMenuAsync(context);
        var itemId = await CreateItemAsync(context);
        var menuItem = new MenuItem(itemId, 5);
        await repository.AddAsync(menuId, menuItem);

        // Act
        Func<Task> act = async () => await repository.AddAsync(menuId, menuItem);

        // Assert
        await act.Should().ThrowAsync<Exception>(); // может быть DbUpdateException или другая ошибка из-за уникальности ключа
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateAmount()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = await CreateMenuAsync(context);
        var itemId = await CreateItemAsync(context);
        var menuItem = new MenuItem(itemId, 5);
        await repository.AddAsync(menuId, menuItem);

        // Act
        var updatedMenuItem = new MenuItem(itemId, 10);
        await repository.UpdateAsync(menuId, updatedMenuItem);

        // Assert
        var saved = await context.MenuItems.FirstOrDefaultAsync(mi => mi.MenuId == menuId && mi.ItemId == itemId);
        saved.Should().NotBeNull();
        saved!.Amount.Should().Be(10);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menuItem = new MenuItem(itemId, 5);

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(menuId, menuItem);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*MenuItem not found for MenuId {menuId} and ItemId {itemId}*");
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveMenuItem()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = await CreateMenuAsync(context);
        var itemId = await CreateItemAsync(context);
        var menuItem = new MenuItem(itemId, 5);
        await repository.AddAsync(menuId, menuItem);

        // Act
        await repository.RemoveAsync(menuId, itemId);

        // Assert
        var saved = await context.MenuItems.FirstOrDefaultAsync(mi => mi.MenuId == menuId && mi.ItemId == itemId);
        saved.Should().BeNull();
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new MenuItemRepository(context, NullLogger<MenuItemRepository>.Instance);
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await repository.RemoveAsync(menuId, itemId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*MenuItem not found for MenuId {menuId} and ItemId {itemId}*");
    }
}