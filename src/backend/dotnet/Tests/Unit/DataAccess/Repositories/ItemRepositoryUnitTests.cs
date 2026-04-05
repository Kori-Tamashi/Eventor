using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
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
public class ItemRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistItem()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item = ItemFixture.Default()
            .WithTitle("Test Item")
            .WithCost(150m)
            .Build();

        await repository.CreateAsync(item);

        var result = await repository.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Item");
        result.Cost.Should().Be(150m);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item = ItemFixture.Default().Build();
        await repository.CreateAsync(item);

        var result = await repository.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllItems_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item1 = ItemFixture.Default().WithTitle("Item A").Build();
        var item2 = ItemFixture.Default().WithTitle("Item B").Build();
        await repository.CreateAsync(item1);
        await repository.CreateAsync(item2);

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        for (int i = 1; i <= 5; i++)
        {
            var item = ItemFixture.Default().WithTitle($"Item {i}").Build();
            await repository.CreateAsync(item);
        }

        var filter = new ItemFilter { PageNumber = 2, PageSize = 2 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateItem()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item = ItemFixture.Default().WithTitle("Old").WithCost(100m).Build();
        await repository.CreateAsync(item);

        var updated = ItemFixture.Default()
            .WithId(item.Id)
            .WithTitle("New")
            .WithCost(200m)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Cost.Should().Be(200m);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item = ItemFixture.Default().Build();

        Func<Task> act = async () => await repository.UpdateAsync(item);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);
        var item = ItemFixture.Default().Build();
        await repository.CreateAsync(item);

        await repository.DeleteAsync(item.Id);

        var result = await repository.GetByIdAsync(item.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ItemRepository(context, NullLogger<ItemRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}