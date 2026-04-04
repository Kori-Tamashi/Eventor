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
public class ItemServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private ItemService _sutService = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<ItemRepository>.Instance;
        var repository = new ItemRepository(DbContext!, logger);
        _sutService = new ItemService(repository);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistItem_AndGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.Empty,
            Title = "New Item",
            Cost = 100m
        };

        // Act
        var result = await _sutService.CreateAsync(item);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("New Item");
        saved.Cost.Should().Be(100m);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        // Arrange
        var fixedId = Guid.NewGuid();
        var item = new Item
        {
            Id = fixedId,
            Title = "Fixed Id Item",
            Cost = 50m
        };

        // Act
        var result = await _sutService.CreateAsync(item);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Fixed Id Item");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowItemCreateException_WhenDuplicateId()
    {
        // Arrange
        var existingItem = ItemFixture.Default().Build();
        await _sutService.CreateAsync(existingItem);

        var duplicate = new Item
        {
            Id = existingItem.Id,
            Title = "Duplicate",
            Cost = 999m
        };

        // Act
        Func<Task> act = async () => await _sutService.CreateAsync(duplicate);

        // Assert
        await act.Should().ThrowAsync<ItemCreateException>();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        // Arrange
        var item = ItemFixture.Default().WithTitle("GetById Test").Build();
        await _sutService.CreateAsync(item);

        // Act
        var result = await _sutService.GetByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Title.Should().Be("GetById Test");
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
    public async Task GetAsync_ShouldReturnAllItems_WhenNoFilter()
    {
        // Arrange
        var item1 = ItemFixture.Default().WithTitle("A").Build();
        var item2 = ItemFixture.Default().WithTitle("B").Build();
        await _sutService.CreateAsync(item1);
        await _sutService.CreateAsync(item2);

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(i => i.Id).Should().Contain([item1.Id, item2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitleContains()
    {
        // Arrange
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Apple").Build());
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Banana").Build());
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Pineapple").Build());

        var filter = new ItemFilter { TitleContains = "apple" };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        result.Select(i => i.Title).Should().Contain(["Apple", "Pineapple"]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await _sutService.CreateAsync(ItemFixture.Default().WithTitle($"Item{i}").Build());
        }

        var filter = new ItemFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOrderedByTitle()
    {
        // Arrange
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Charlie").Build());
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Alice").Build());
        await _sutService.CreateAsync(ItemFixture.Default().WithTitle("Bob").Build());

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Select(i => i.Title).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnEmpty_WhenPageOutOfRange()
    {
        // Arrange
        for (int i = 1; i <= 3; i++)
        {
            await _sutService.CreateAsync(ItemFixture.Default().WithTitle($"Item{i}").Build());
        }

        var filter = new ItemFilter { PageNumber = 10, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingItem()
    {
        // Arrange
        var item = ItemFixture.Default().WithTitle("Old").WithCost(10m).Build();
        await _sutService.CreateAsync(item);

        var updated = new Item
        {
            Id = item.Id,
            Title = "New Title",
            Cost = 999m
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Cost.Should().Be(999m);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowItemNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        var item = ItemFixture.Default().Build(); // не сохранён

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(item);

        // Assert
        await act.Should().ThrowAsync<ItemNotFoundException>()
            .WithMessage($"Item '{item.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingItem()
    {
        // Arrange
        var item = ItemFixture.Default().Build();
        await _sutService.CreateAsync(item);

        // Act
        await _sutService.DeleteAsync(item.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(item.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowItemNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<ItemNotFoundException>()
            .WithMessage($"Item '{id}' was not found.");
    }
}