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
public class EventControllersIntegrationTests : DatabaseIntegrationTestBase
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
    public async Task GetEvents_ShouldReturnList()
    {
        // Arrange
        var location = new LocationDb(
            id: Guid.NewGuid(),
            title: "Test Location",
            description: "Desc",
            cost: 1000,
            capacity: 100
        );
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var event1 = new EventDb(
            id: Guid.NewGuid(),
            title: "Concert",
            description: "Rock concert",
            startDate: new DateOnly(2025, 6, 1),
            locationId: location.Id,
            daysCount: 1,
            percent: 0
        );
        var event2 = new EventDb(
            id: Guid.NewGuid(),
            title: "Conference",
            description: "Tech conference",
            startDate: new DateOnly(2025, 7, 15),
            locationId: location.Id,
            daysCount: 2,
            percent: 5
        );
        DbContext.Events.AddRange(event1, event2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/events");
        var events = await response.Content.ReadFromJsonAsync<List<Event>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(events);
        Assert.HasCount(2, events);
        Assert.IsTrue(events.Any(e => e.Title == "Concert"));
        Assert.IsTrue(events.Any(e => e.Title == "Conference"));
    }
    
    [TestMethod]
    public async Task GetEvents_WhenNoEvents_ShouldReturnEmptyList()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync("/api/v1/events");
        var events = await response.Content.ReadFromJsonAsync<List<Event>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(events);
        Assert.IsEmpty(events);
    }
    
    [TestMethod]
    public async Task CreateEvent_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var location = new LocationDb(
            Guid.NewGuid(), 
            "Test Loc", 
            "Desc", 
            1000, 
            100);
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var request = new CreateEventRequest
        {
            Title = "New Event",
            Description = "Description",
            StartDate = new DateOnly(2025, 12, 25),
            LocationId = location.Id,
            DaysCount = 3,
            Percent = 10
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/events", request);
        var created = await response.Content.ReadFromJsonAsync<Event>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(request.Title, created.Title);
        Assert.AreEqual(request.DaysCount, created.DaysCount);
        
        var eventInDb = await DbContext.Events.FindAsync(created.Id);
        Assert.IsNotNull(eventInDb);
    }
    
    [TestMethod]
    public async Task GetEventById_WhenExists_ReturnsOk()
    {
        // Arrange
        var location = new LocationDb(
            Guid.NewGuid(), 
            "Loc", 
            "Desc", 
            500, 
            50);
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var eventEntity = new EventDb(Guid.NewGuid(), 
            "Event1", 
            "Desc", 
            new DateOnly(2025, 1, 1), 
            location.Id, 
            2, 
            5);
        DbContext.Events.Add(eventEntity);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/events/{eventEntity.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Event>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(dto);
        Assert.AreEqual(eventEntity.Id, dto.Id);
        Assert.AreEqual(eventEntity.Title, dto.Title);
    }
    
    [TestMethod]
    public async Task GetEventById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/events/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteEvent_WhenExists_ReturnsNoContentAndRemoves()
    {
        // Arrange
        var location = new LocationDb(Guid.NewGuid(), "Loc", "Desc", 500, 50);
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var eventEntity = new EventDb(Guid.NewGuid(), "ToDelete", "Desc", new DateOnly(2025, 1, 1), location.Id, 1, 0);
        DbContext.Events.Add(eventEntity);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/events/{eventEntity.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        DbContext.Entry(eventEntity).State = EntityState.Detached;
        var deleted = await DbContext.Events.FindAsync(eventEntity.Id);
        Assert.IsNull(deleted);
    }
}