using System.Net;
using System.Net.Http.Json;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Web.Dtos;
using Tests.Core;
using Tests.Core.DatabaseIntegration;

namespace Tests.Integration.Web.Controllers;

[TestClass]
[TestCategory("Integration")]
public class LocationControllersIntergationTests : DatabaseIntegrationTestBase
{
    private CustomWebApplicationFactory<Program> _factory = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void TestInitializeHttp()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }

    [TestCleanup]
    public void TestCleanupHttp()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
    }
    
    [TestMethod]
    public async Task GetLocations_ShouldReturnListWithTotalCount()
    {
        // Arrange
        var location1 = new LocationDb(
            id: Guid.NewGuid(),
            title: "Hall A",
            description: "Big hall",
            cost: 1000,
            capacity: 200
        );
        var location2 = new LocationDb(
            id: Guid.NewGuid(),
            title: "Hall B",
            description: "Small hall",
            cost: 500,
            capacity: 50
        );
        DbContext!.Locations.AddRange(location1, location2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/locations");
        var locations = await response.Content.ReadFromJsonAsync<List<Location>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(locations);
        Assert.HasCount(2, locations);
    }
    
    [TestMethod]
    public async Task GetLocations_WithTitleFilter_ShouldReturnFiltered()
    {
        // Arrange
        var hall = new LocationDb(
            Guid.NewGuid(), 
            "Conference Hall", 
            "Big", 
            1000, 
            200);
        var room = new LocationDb(
            Guid.NewGuid(), 
            "Meeting Room", 
            "Small", 
            500, 
            20);
        DbContext!.Locations.AddRange(hall, room);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            "/api/v1/locations?TitleContains=Hall");
        var locations = await response.Content.ReadFromJsonAsync<List<Location>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.HasCount(1, locations);
        Assert.AreEqual("Conference Hall", locations[0].Title);
    }

    [TestMethod]
    public async Task GetLocations_WhenNoLocations_ShouldReturnEmptyList()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/locations");
        var locations = await response.Content.ReadFromJsonAsync<List<Location>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(locations);
        Assert.HasCount(0, locations);
    }
    
    [TestMethod]
    public async Task GetLocationById_WhenExists_ReturnsOk()
    {
        var location = new LocationDb(
            Guid.NewGuid(), 
            "Test", 
            "Desc", 
            100, 
            50);
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var response = await _httpClient.GetAsync(
            $"/api/v1/locations/{location.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Location>();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(location.Id, dto!.Id);
    }

    [TestMethod]
    public async Task GetLocationById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync(
            $"/api/v1/locations/{Guid.NewGuid()}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CreateLocation_ValidRequest_ReturnsCreated()
    {
        var request = new CreateLocationRequest
        {
            Title = "New Hall",
            Description = "Great place",
            Cost = 2000,
            Capacity = 150
        };
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/locations", request);
        var created = await response.Content.ReadFromJsonAsync<Location>();

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Title, created.Title);
    }

    [TestMethod]
    public async Task CreateLocation_InvalidData_ReturnsBadRequest()
    {
        var request = new CreateLocationRequest
        {
            Title = "",
            Description = "Desc",
            Cost = -100,
            Capacity = 0
        };
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/locations", request);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task DeleteLocation_WhenExists_ReturnsNoContentAndRemoves()
    {
        // Arrange
        var location = new LocationDb(
            id: Guid.NewGuid(),
            title: "ToDelete",
            description: "Test",
            cost: 100,
            capacity: 50
        );
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/locations/{location.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        DbContext.Entry(location).State = EntityState.Detached;
        var deleted = await DbContext.Locations.FindAsync(location.Id);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task DeleteLocation_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/admin/locations/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}