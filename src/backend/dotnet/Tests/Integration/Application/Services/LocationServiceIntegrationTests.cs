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
public class LocationServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private LocationService _sutService = null!;

    [TestInitialize]
    public void Setup()
    {
        var repository = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        _sutService = new LocationService(repository);
    }

    #region Вспомогательные методы

    private async Task<Location> CreateLocationAsync(
        string title = "Test Location",
        string description = "Test Description",
        decimal cost = 100m,
        int capacity = 50)
    {
        var location = LocationFixture.Default()
            .WithTitle(title)
            .WithDescription(description)
            .WithCost(cost)
            .WithCapacity(capacity)
            .Build();
        return await _sutService.CreateAsync(location);
    }

    #endregion

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var location = new Location
        {
            Id = Guid.Empty,
            Title = "New Location",
            Description = "Desc",
            Cost = 200m,
            Capacity = 100
        };

        // Act
        var result = await _sutService.CreateAsync(location);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("New Location");
        saved.Description.Should().Be("Desc");
        saved.Cost.Should().Be(200m);
        saved.Capacity.Should().Be(100);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        // Arrange
        var fixedId = Guid.NewGuid();
        var location = new Location
        {
            Id = fixedId,
            Title = "Fixed Id Location",
            Description = "Desc",
            Cost = 300m,
            Capacity = 150
        };

        // Act
        var result = await _sutService.CreateAsync(location);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnLocation_WhenExists()
    {
        // Arrange
        var location = await CreateLocationAsync();

        // Act
        var result = await _sutService.GetByIdAsync(location.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(location.Id);
        result.Title.Should().Be(location.Title);
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
    public async Task GetAsync_ShouldReturnAllLocations_WhenNoFilter()
    {
        // Arrange
        var loc1 = await CreateLocationAsync(title: "Alpha");
        var loc2 = await CreateLocationAsync(title: "Beta");

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(l => l.Id).Should().Contain([loc1.Id, loc2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByTitle()
    {
        // Arrange
        await CreateLocationAsync(title: "Conference Hall");
        await CreateLocationAsync(title: "Meeting Room");
        await CreateLocationAsync(title: "Conference Center");

        var filter = new LocationFilter { TitleContains = "Conference" };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        result.Select(l => l.Title).Should().Contain(["Conference Hall", "Conference Center"]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByCostRange()
    {
        // Arrange
        await CreateLocationAsync(cost: 50);
        await CreateLocationAsync(cost: 150);
        await CreateLocationAsync(cost: 250);

        var filter = new LocationFilter { CostFrom = 100, CostTo = 200 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().ContainSingle();
        result[0].Cost.Should().Be(150);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByCapacityRange()
    {
        // Arrange
        await CreateLocationAsync(capacity: 10);
        await CreateLocationAsync(capacity: 50);
        await CreateLocationAsync(capacity: 100);

        var filter = new LocationFilter { CapacityFrom = 30, CapacityTo = 80 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().ContainSingle();
        result[0].Capacity.Should().Be(50);
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await CreateLocationAsync(title: $"Location {i}");
        }

        var filter = new LocationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOrderedByTitle()
    {
        // Arrange
        await CreateLocationAsync(title: "Charlie");
        await CreateLocationAsync(title: "Alice");
        await CreateLocationAsync(title: "Bob");

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Select(l => l.Title).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingLocation()
    {
        // Arrange
        var location = await CreateLocationAsync(title: "Old Title", description: "Old Desc", cost: 100, capacity: 50);

        var updated = new Location
        {
            Id = location.Id,
            Title = "New Title",
            Description = "New Desc",
            Cost = 999,
            Capacity = 999
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(location.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.Cost.Should().Be(999);
        result.Capacity.Should().Be(999);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowLocationNotFoundException_WhenLocationDoesNotExist()
    {
        // Arrange
        var location = new Location { Id = Guid.NewGuid(), Title = "Ghost" };

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(location);

        // Assert
        await act.Should().ThrowAsync<LocationNotFoundException>()
            .WithMessage($"Location '{location.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingLocation()
    {
        // Arrange
        var location = await CreateLocationAsync();

        // Act
        await _sutService.DeleteAsync(location.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(location.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowLocationNotFoundException_WhenLocationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<LocationNotFoundException>()
            .WithMessage($"Location '{id}' was not found.");
    }
}