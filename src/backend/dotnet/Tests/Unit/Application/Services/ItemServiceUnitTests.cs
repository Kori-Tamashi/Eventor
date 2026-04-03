using System;
using System.Collections.Generic;
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
public class ItemServiceUnitTests
{
    private Mock<IItemRepository> _itemRepositoryMock;
    private ItemService _sut;

    [TestInitialize]
    public void Setup()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();
        _sut = new ItemService(_itemRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        // Arrange
        var expectedItem = ItemFixture.Default().Build();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(expectedItem.Id))
            .ReturnsAsync(expectedItem);

        // Act
        var result = await _sut.GetByIdAsync(expectedItem.Id);

        // Assert
        result.Should().BeEquivalentTo(expectedItem);
        _itemRepositoryMock.Verify(x => x.GetByIdAsync(expectedItem.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Item)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
        _itemRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnList_WhenFilterProvided()
    {
        // Arrange
        var filter = new ItemFilter { TitleContains = "test" };
        var expectedItems = new List<Item>
        {
            ItemFixture.Default().WithTitle("test1").Build(),
            ItemFixture.Default().WithTitle("test2").Build()
        };
        _itemRepositoryMock
            .Setup(x => x.GetAsync(filter))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _sut.GetAsync(filter);

        // Assert
        result.Should().BeEquivalentTo(expectedItems);
        _itemRepositoryMock.Verify(x => x.GetAsync(filter), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSetIdAndCallRepository_WhenIdIsEmpty()
    {
        // Arrange
        var item = ItemFixture.Default().WithId(Guid.Empty).Build();
        _itemRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Item>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _sut.CreateAsync(item);

        // Assert
        result.Id.Should().NotBeEmpty();
        _itemRepositoryMock.Verify(x => x.CreateAsync(It.Is<Item>(i => i.Id == result.Id)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldNotChangeIdAndCallRepository_WhenIdIsNotEmpty()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        var originalId = item.Id;
        _itemRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Item>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _sut.CreateAsync(item);

        // Assert
        result.Id.Should().Be(originalId);
        _itemRepositoryMock.Verify(x => x.CreateAsync(It.Is<Item>(i => i.Id == originalId)), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowItemCreateException_WhenRepositoryThrows()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        var dbException = new Exception("DB error");
        _itemRepositoryMock
            .Setup(x => x.CreateAsync(item))
            .ThrowsAsync(dbException);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(item);

        // Assert
        var exception = await act.Should().ThrowAsync<ItemCreateException>()
            .WithMessage("Failed to create item.");
        exception.Which.InnerException.Should().BeSameAs(dbException);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldCallRepository_WhenItemExists()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id))
            .ReturnsAsync(item);
        _itemRepositoryMock
            .Setup(x => x.UpdateAsync(item))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _sut.UpdateAsync(item);

        // Assert
        _itemRepositoryMock.Verify(x => x.UpdateAsync(item), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowItemNotFoundException_WhenItemNotFound()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id))
            .ReturnsAsync((Item)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(item);

        // Assert
        await act.Should().ThrowAsync<ItemNotFoundException>()
            .WithMessage($"Item '{item.Id}' was not found.");
        _itemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Item>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowItemUpdateException_WhenRepositoryThrows()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        var dbException = new Exception("DB error");
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id))
            .ReturnsAsync(item);
        _itemRepositoryMock
            .Setup(x => x.UpdateAsync(item))
            .ThrowsAsync(dbException);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(item);

        // Assert
        var exception = await act.Should().ThrowAsync<ItemUpdateException>()
            .WithMessage("Failed to update item.");
        exception.Which.InnerException.Should().BeSameAs(dbException);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldCallRepository_WhenItemExists()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id))
            .ReturnsAsync(item);
        _itemRepositoryMock
            .Setup(x => x.DeleteAsync(item.Id))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _sut.DeleteAsync(item.Id);

        // Assert
        _itemRepositoryMock.Verify(x => x.DeleteAsync(item.Id), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowItemNotFoundException_WhenItemNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Item)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<ItemNotFoundException>()
            .WithMessage($"Item '{id}' was not found.");
        _itemRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowItemDeleteException_WhenRepositoryThrows()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        var dbException = new Exception("DB error");
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id))
            .ReturnsAsync(item);
        _itemRepositoryMock
            .Setup(x => x.DeleteAsync(item.Id))
            .ThrowsAsync(dbException);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(item.Id);

        // Assert
        var exception = await act.Should().ThrowAsync<ItemDeleteException>()
            .WithMessage("Failed to delete item.");
        exception.Which.InnerException.Should().BeSameAs(dbException);
    }
}