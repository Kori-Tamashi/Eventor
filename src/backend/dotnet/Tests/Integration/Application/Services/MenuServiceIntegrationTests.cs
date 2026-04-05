using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Repositories;
using Domain.Filters;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class MenuServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private MenuService _sutService = null!;

    [TestInitialize]
    public void Setup()
    {
        var menuRepository = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        var menuItemRepository = new MenuItemRepository(DbContext!, NullLogger<MenuItemRepository>.Instance);
        _sutService = new MenuService(menuRepository, menuItemRepository);
    }

    #region Helper Methods

    private async Task<Item> CreateItemAsync(string title = "Test Item", decimal cost = 100m)
    {
        var item = ItemFixture.Default()
            .WithTitle(title)
            .WithCost(cost)
            .Build();
        var itemRepository = new ItemRepository(DbContext!, NullLogger<ItemRepository>.Instance);
        await itemRepository.CreateAsync(item);
        return item;
    }

    private async Task<Menu> CreateMenuAsync(string title = "Test Menu", string description = "Description")
    {
        var menu = MenuFixture.Default()
            .WithTitle(title)
            .WithDescription(description)
            .Build();
        return await _sutService.CreateAsync(menu);
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var menu = new Menu
        {
            Id = Guid.Empty,
            Title = "New Menu",
            Description = "New Description"
        };

        // Act
        var result = await _sutService.CreateAsync(menu);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("New Menu");
        saved.Description.Should().Be("New Description");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdProvided()
    {
        // Arrange
        var fixedId = Guid.NewGuid();
        var menu = new Menu
        {
            Id = fixedId,
            Title = "Fixed Id Menu",
            Description = "Desc"
        };

        // Act
        var result = await _sutService.CreateAsync(menu);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowMenuCreateException_WhenDuplicateId()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var duplicate = new Menu
        {
            Id = menu.Id,
            Title = "Duplicate",
            Description = "Duplicate"
        };

        // Act
        Func<Task> act = async () => await _sutService.CreateAsync(duplicate);

        // Assert
        await act.Should().ThrowAsync<MenuCreateException>();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnMenu_WhenExists()
    {
        // Arrange
        var menu = await CreateMenuAsync();

        // Act
        var result = await _sutService.GetByIdAsync(menu.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(menu.Id);
        result.Title.Should().Be(menu.Title);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldIncludeItems_WhenRequested()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 5);

        // Act
        var result = await _sutService.GetByIdAsync(menu.Id, includeItems: true);

        // Assert
        result.Should().NotBeNull();
        result!.MenuItems.Should().HaveCount(1);
        result.MenuItems.First().ItemId.Should().Be(item.Id);
        result.MenuItems.First().Amount.Should().Be(5);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _sutService.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllMenus_WhenNoFilter()
    {
        // Arrange
        var menu1 = await CreateMenuAsync("Menu A");
        var menu2 = await CreateMenuAsync("Menu B");

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(m => m.Id).Should().Contain([menu1.Id, menu2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        // Arrange
        await CreateMenuAsync("Breakfast Special");
        await CreateMenuAsync("Dinner Special");
        await CreateMenuAsync("Lunch Menu");

        var filter = new MenuFilter { TitleContains = "Special" };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        result.All(m => m.Title.Contains("Special")).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await CreateMenuAsync($"Menu {i}");
        }

        var filter = new MenuFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldIncludeItems_WhenRequested()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 3);

        // Act
        var result = await _sutService.GetAsync(includeItems: true);

        // Assert
        var targetMenu = result.FirstOrDefault(m => m.Id == menu.Id);
        targetMenu.Should().NotBeNull();
        targetMenu!.MenuItems.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync("Old Title", "Old Desc");

        var updated = new Menu
        {
            Id = menu.Id,
            Title = "New Title",
            Description = "New Desc"
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(menu.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowMenuNotFoundException_WhenMenuDoesNotExist()
    {
        // Arrange
        var menu = new Menu { Id = Guid.NewGuid(), Title = "Ghost", Description = "Ghost" };

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(menu);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>()
            .WithMessage($"Menu '{menu.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync();

        // Act
        await _sutService.DeleteAsync(menu.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(menu.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowMenuNotFoundException_WhenMenuDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>()
            .WithMessage($"Menu '{id}' was not found.");
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldReturnAllItemsFromMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item1 = await CreateItemAsync("Item1", 10);
        var item2 = await CreateItemAsync("Item2", 20);
        await _sutService.AddItemAsync(menu.Id, item1.Id, 2);
        await _sutService.AddItemAsync(menu.Id, item2.Id, 3);

        // Act
        var items = await _sutService.GetItemsAsync(menu.Id);

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(mi => mi.ItemId == item1.Id && mi.Amount == 2);
        items.Should().Contain(mi => mi.ItemId == item2.Id && mi.Amount == 3);
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldApplyPagination()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        for (int i = 1; i <= 5; i++)
        {
            var item = await CreateItemAsync($"Item{i}", i * 10);
            await _sutService.AddItemAsync(menu.Id, item.Id, i);
        }

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var items = await _sutService.GetItemsAsync(menu.Id, filter);

        // Assert
        items.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldThrowMenuNotFoundException_WhenMenuDoesNotExist()
    {
        // Act
        Func<Task> act = async () => await _sutService.GetItemsAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>();
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldReturnCorrectAmount()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 7);

        // Act
        var amount = await _sutService.GetItemAmountAsync(menu.Id, item.Id);

        // Assert
        amount.Should().Be(7);
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldThrowMenuNotFoundException_WhenMenuNotFound()
    {
        // Arrange
        var item = await CreateItemAsync();

        // Act
        Func<Task> act = async () => await _sutService.GetItemAmountAsync(Guid.NewGuid(), item.Id);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>();
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldThrowMenuServiceException_WhenItemNotInMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync(); // not added

        // Act
        Func<Task> act = async () => await _sutService.GetItemAmountAsync(menu.Id, item.Id);

        // Assert
        await act.Should().ThrowAsync<MenuServiceException>()
            .WithMessage($"Item '{item.Id}' is not in menu '{menu.Id}'.");
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldAddMenuItem()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();

        // Act
        await _sutService.AddItemAsync(menu.Id, item.Id, 4);

        // Assert
        var items = await _sutService.GetItemsAsync(menu.Id);
        items.Should().ContainSingle(mi => mi.ItemId == item.Id && mi.Amount == 4);
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldThrowMenuNotFoundException_WhenMenuNotFound()
    {
        // Arrange
        var item = await CreateItemAsync();

        // Act
        Func<Task> act = async () => await _sutService.AddItemAsync(Guid.NewGuid(), item.Id, 1);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>();
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldThrowMenuServiceException_WhenItemAlreadyExists()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 2);

        // Act
        Func<Task> act = async () => await _sutService.AddItemAsync(menu.Id, item.Id, 3);

        // Assert
        await act.Should().ThrowAsync<MenuServiceException>()
            .WithMessage($"Item '{item.Id}' already exists in menu '{menu.Id}'.");
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldChangeAmount()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 2);

        // Act
        await _sutService.UpdateItemAmountAsync(menu.Id, item.Id, 10);

        // Assert
        var amount = await _sutService.GetItemAmountAsync(menu.Id, item.Id);
        amount.Should().Be(10);
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldThrowMenuNotFoundException_WhenMenuNotFound()
    {
        // Arrange
        var item = await CreateItemAsync();

        // Act
        Func<Task> act = async () => await _sutService.UpdateItemAmountAsync(Guid.NewGuid(), item.Id, 1);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>();
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldThrowMenuServiceException_WhenItemNotInMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync(); // not added

        // Act
        Func<Task> act = async () => await _sutService.UpdateItemAmountAsync(menu.Id, item.Id, 5);

        // Assert
        await act.Should().ThrowAsync<MenuServiceException>()
            .WithMessage($"Item '{item.Id}' is not in menu '{menu.Id}'.");
    }

    [TestMethod]
    public async Task RemoveItemAsync_ShouldDeleteMenuItem()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync();
        await _sutService.AddItemAsync(menu.Id, item.Id, 2);

        // Act
        await _sutService.RemoveItemAsync(menu.Id, item.Id);

        // Assert
        var items = await _sutService.GetItemsAsync(menu.Id);
        items.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RemoveItemAsync_ShouldThrowMenuNotFoundException_WhenMenuNotFound()
    {
        // Arrange
        var item = await CreateItemAsync();

        // Act
        Func<Task> act = async () => await _sutService.RemoveItemAsync(Guid.NewGuid(), item.Id);

        // Assert
        await act.Should().ThrowAsync<MenuNotFoundException>();
    }

    [TestMethod]
    public async Task RemoveItemAsync_ShouldThrowMenuServiceException_WhenItemNotInMenu()
    {
        // Arrange
        var menu = await CreateMenuAsync();
        var item = await CreateItemAsync(); // не добавлен в меню

        // Act
        Func<Task> act = async () => await _sutService.RemoveItemAsync(menu.Id, item.Id);

        // Assert
        await act.Should().ThrowAsync<MenuServiceException>()
            .WithMessage("Failed to remove item from menu.");
    }
}