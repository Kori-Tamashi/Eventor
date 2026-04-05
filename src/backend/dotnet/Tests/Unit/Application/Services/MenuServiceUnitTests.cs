using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tests.Core.Fixtures;

namespace Tests.Unit.Application.Services;

[TestClass]
[TestCategory("Unit")]
public class MenuServiceUnitTests
{
    private Mock<IMenuRepository> _menuRepositoryMock;
    private Mock<IMenuItemRepository> _menuItemRepositoryMock;
    private MenuService _sut;

    [TestInitialize]
    public void Setup()
    {
        _menuRepositoryMock = new Mock<IMenuRepository>();
        _menuItemRepositoryMock = new Mock<IMenuItemRepository>();
        _sut = new MenuService(_menuRepositoryMock.Object, _menuItemRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnMenu_WhenExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var expectedMenu = MenuFixture.Default().WithId(menuId).Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(expectedMenu);

        // Act
        var result = await _sut.GetByIdAsync(menuId, true);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuId);
        _menuRepositoryMock.Verify(x => x.GetByIdAsync(menuId, true), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, false))
            .ReturnsAsync((Menu?)null);

        // Act
        var result = await _sut.GetByIdAsync(menuId, false);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnListOfMenus()
    {
        // Arrange
        var filter = new MenuFilter { TitleContains = "test" };
        var expectedMenus = new List<Menu>
        {
            MenuFixture.Default().WithTitle("Test1").Build(),
            MenuFixture.Default().WithTitle("Test2").Build()
        };
        _menuRepositoryMock
            .Setup(x => x.GetAsync(filter, true))
            .ReturnsAsync(expectedMenus);

        // Act
        var result = await _sut.GetAsync(filter, true);

        // Assert
        result.Should().HaveCount(2);
        _menuRepositoryMock.Verify(x => x.GetAsync(filter, true), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenEmpty()
    {
        // Arrange
        var menu = new Menu { Id = Guid.Empty, Title = "New Menu", Description = "Desc" };
        _menuRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Menu>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(menu);

        // Assert
        result.Id.Should().NotBeEmpty();
        _menuRepositoryMock.Verify(x => x.CreateAsync(It.Is<Menu>(m => m.Id != Guid.Empty)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepExistingId()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var menu = new Menu { Id = menuId, Title = "Existing Id Menu" };
        _menuRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Menu>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(menu);

        // Assert
        result.Id.Should().Be(menuId);
        _menuRepositoryMock.Verify(x => x.CreateAsync(menu), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowMenuCreateException_WhenRepositoryFails()
    {
        // Arrange
        var menu = MenuFixture.Default().Build();
        _menuRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Menu>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<MenuCreateException>(
            () => _sut.CreateAsync(menu));
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdate_WhenMenuExists()
    {
        // Arrange
        var menu = MenuFixture.Default().Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menu.Id, false))
            .ReturnsAsync(menu);
        _menuRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Menu>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(menu);

        // Assert
        _menuRepositoryMock.Verify(x => x.UpdateAsync(menu), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        var menu = MenuFixture.Default().Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menu.Id, false))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.UpdateAsync(menu));
        _menuRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Menu>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowMenuUpdateException_WhenRepositoryFails()
    {
        // Arrange
        var menu = MenuFixture.Default().Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menu.Id, false))
            .ReturnsAsync(menu);
        _menuRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Menu>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<MenuUpdateException>(
            () => _sut.UpdateAsync(menu));
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDelete_WhenMenuExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var existingMenu = MenuFixture.Default().WithId(menuId).Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, false))
            .ReturnsAsync(existingMenu);
        _menuRepositoryMock
            .Setup(x => x.DeleteAsync(menuId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(menuId);

        // Assert
        _menuRepositoryMock.Verify(x => x.DeleteAsync(menuId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, false))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.DeleteAsync(menuId));
        _menuRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowMenuDeleteException_WhenRepositoryFails()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var existingMenu = MenuFixture.Default().WithId(menuId).Build();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, false))
            .ReturnsAsync(existingMenu);
        _menuRepositoryMock
            .Setup(x => x.DeleteAsync(menuId))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<MenuDeleteException>(
            () => _sut.DeleteAsync(menuId));
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldReturnItems_WhenMenuExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var menuItems = new List<MenuItem>
        {
            new MenuItem(Guid.NewGuid(), 2),
            new MenuItem(Guid.NewGuid(), 5)
        };
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = menuItems;

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        // Act
        var result = await _sut.GetItemsAsync(menuId);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.GetItemsAsync(menuId));
    }

    [TestMethod]
    public async Task GetItemsAsync_ShouldApplyPagination()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var menuItems = new List<MenuItem>();
        for (int i = 0; i < 10; i++)
            menuItems.Add(new MenuItem(Guid.NewGuid(), i));

        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = menuItems;

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        var pagination = new PaginationFilter { PageNumber = 2, PageSize = 3 };

        // Act
        var result = await _sut.GetItemsAsync(menuId, pagination);

        // Assert
        result.Should().HaveCount(3);
        // Проверяем, что это вторая страница (элементы с 3 по 5 индексы)
        result.Should().Equal(menuItems.Skip(3).Take(3));
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldReturnAmount_WhenMenuItemExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menuItems = new List<MenuItem> { new MenuItem(itemId, 10) };
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = menuItems;

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        // Act
        var result = await _sut.GetItemAmountAsync(menuId, itemId);

        // Assert
        result.Should().Be(10);
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.GetItemAmountAsync(menuId, Guid.NewGuid()));
    }

    [TestMethod]
    public async Task GetItemAmountAsync_ShouldThrowMenuServiceException_WhenItemNotInMenu()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem>(); // пусто

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        // Act & Assert
        await Assert.ThrowsAsync<MenuServiceException>(
            () => _sut.GetItemAmountAsync(menuId, Guid.NewGuid()));
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldAddItem_WhenMenuExistsAndItemNotExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var amount = 5;
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem>();

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);
        _menuItemRepositoryMock
            .Setup(x => x.AddAsync(menuId, It.IsAny<MenuItem>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AddItemAsync(menuId, itemId, amount);

        // Assert
        _menuItemRepositoryMock.Verify(x => x.AddAsync(menuId, It.Is<MenuItem>(mi => mi.ItemId == itemId && mi.Amount == amount)), Times.Once);
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.AddItemAsync(Guid.NewGuid(), Guid.NewGuid(), 1));
        _menuItemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<MenuItem>()), Times.Never);
    }

    [TestMethod]
    public async Task AddItemAsync_ShouldThrowMenuServiceException_WhenItemAlreadyExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem> { new MenuItem(itemId, 2) };

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        // Act & Assert
        await Assert.ThrowsAsync<MenuServiceException>(
            () => _sut.AddItemAsync(menuId, itemId, 3));
        _menuItemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<MenuItem>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldUpdate_WhenMenuExistsAndItemExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var newAmount = 15;
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem> { new MenuItem(itemId, 5) };

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);
        _menuItemRepositoryMock
            .Setup(x => x.UpdateAsync(menuId, It.IsAny<MenuItem>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateItemAmountAsync(menuId, itemId, newAmount);

        // Assert
        _menuItemRepositoryMock.Verify(x => x.UpdateAsync(menuId, It.Is<MenuItem>(mi => mi.ItemId == itemId && mi.Amount == newAmount)), Times.Once);
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.UpdateItemAmountAsync(Guid.NewGuid(), Guid.NewGuid(), 1));
        _menuItemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<MenuItem>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateItemAmountAsync_ShouldThrowMenuServiceException_WhenItemNotInMenu()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem>();

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);

        // Act & Assert
        await Assert.ThrowsAsync<MenuServiceException>(
            () => _sut.UpdateItemAmountAsync(menuId, Guid.NewGuid(), 10));
        _menuItemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<MenuItem>()), Times.Never);
    }

    [TestMethod]
    public async Task RemoveItemAsync_ShouldRemove_WhenMenuExists()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menu = MenuFixture.Default().WithId(menuId).Build();
        menu.MenuItems = new List<MenuItem> { new MenuItem(itemId, 1) };

        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(menuId, true))
            .ReturnsAsync(menu);
        _menuItemRepositoryMock
            .Setup(x => x.RemoveAsync(menuId, itemId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RemoveItemAsync(menuId, itemId);

        // Assert
        _menuItemRepositoryMock.Verify(x => x.RemoveAsync(menuId, itemId), Times.Once);
    }

    [TestMethod]
    public async Task RemoveItemAsync_ShouldThrowMenuNotFoundException_WhenMenuNotExists()
    {
        // Arrange
        _menuRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync((Menu?)null);

        // Act & Assert
        await Assert.ThrowsAsync<MenuNotFoundException>(
            () => _sut.RemoveItemAsync(Guid.NewGuid(), Guid.NewGuid()));
        _menuItemRepositoryMock.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}