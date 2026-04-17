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
public class DayControllersIntegrationTests : DatabaseIntegrationTestBase
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
    public async Task GetDays_WhenNoDays_ReturnsEmptyList()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/days");
        var days = await response.Content.ReadFromJsonAsync<List<Day>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(days);
        Assert.IsEmpty(days);
    }
    
    [TestMethod]
    public async Task GetDayById_WhenExists_ReturnsOk()
    {
        // Arrange
        var location = new LocationDb(
            Guid.NewGuid(), 
            "Loc", 
            "Desc", 
            1000, 
            100);
        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        var eventEntity = new EventDb(
            Guid.NewGuid(), 
            "Event", 
            "Desc", 
            new DateOnly(2025, 1, 1), 
            location.Id, 
            2, 
            0);
        DbContext.Events.Add(eventEntity);
        await DbContext.SaveChangesAsync();

        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Menu", 
            "Desc");
        DbContext.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        var day = new DayDb(
            Guid.NewGuid(),
            eventEntity.Id, 
            menu.Id, 
            "Special Day", 
            1, 
            "Desc");
        DbContext.Days.Add(day);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/days/{day.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Day>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(dto);
        Assert.AreEqual(day.Id, dto.Id);
        Assert.AreEqual(day.Title, dto.Title);
    }

    [TestMethod]
    public async Task GetDayById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/days/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}