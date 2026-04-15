using System.Net;
using System.Net.Http.Json;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Web.Dtos;
using Tests.Core;
using Tests.Core.DatabaseIntegration;

namespace Tests.Integration.Web.Controllers;

[TestClass]
[TestCategory("Integration")]
public class RegistrationControllersIntegrationTests : DatabaseIntegrationTestBase
{
    private CustomWebApplicationFactory<Program> _factory = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void TestInitializeHttp()
    {
        TestInitialize();
        _factory = new CustomWebApplicationFactory<Program>();

        _httpClient = _factory.CreateClient();
    }

    [TestCleanup]
    public void TestCleanupHttp()
    {
        TestCleanup();
        _httpClient?.Dispose();
        _factory?.Dispose();
    }
    
    private async Task<(
        UserDb User, 
        LocationDb Location, 
        EventDb Event, 
        List<DayDb> Days)> CreateTestDataAsync()
    {
        var user = new UserDb(
            Guid.NewGuid(), 
            "Test User", 
            "+1234567890", 
            GenderDb.Male, 
            UserRoleDb.User, 
            "hash");
        DbContext!.Users.Add(user);

        var location = new LocationDb(
            Guid.NewGuid(), 
            "Test Location", 
            "Desc", 
            1000, 
            100);
        DbContext.Locations.Add(location);
        
        await DbContext.SaveChangesAsync();
        
        var eventEntity = new EventDb(
            Guid.NewGuid(), 
            "Test Event", 
            "Desc",
            new DateOnly(
                2025, 
                6, 
                1), 
            location.Id, 
            2, 
            10);
        DbContext.Events.Add(eventEntity);
        await DbContext.SaveChangesAsync();
        
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Test Menu", 
            "Menu Desc");
        DbContext.Menus.Add(menu);
        await DbContext.SaveChangesAsync();
        
        var day1 = new DayDb(
            Guid.NewGuid(), 
            eventEntity.Id, 
            menu.Id, 
            "Day 1", 
            1, 
            "First day");
        var day2 = new DayDb(
            Guid.NewGuid(), 
            eventEntity.Id, 
            menu.Id, 
            "Day 2", 
            2, 
            "Second day");
        DbContext.Days.AddRange(day1, day2);
        await DbContext.SaveChangesAsync();
        
        return (user, 
            location, 
            eventEntity, 
            new List<DayDb> { day1, day2 });
    }
    
    [TestMethod]
    public async Task AdminGetRegistrations_WithFilter_ReturnsList()
    {
        // Arrange
        var (user, _, eventEntity, days) = await CreateTestDataAsync();
        
        var dayIds = days.Select(d => d.Id).ToList();
        var existingDays = await DbContext!.Days.Where(
            d => dayIds.Contains(d.Id)).ToListAsync();

        var registration = new RegistrationDb(
            id: Guid.NewGuid(),
            eventId: eventEntity.Id,
            userId: user.Id,
            type: RegistrationTypeDb.Organizer,
            payment: true
        );
        DbContext.Registrations.Add(registration);
        await DbContext.SaveChangesAsync();

        foreach (var day in existingDays)
        {
            var participation = new ParticipationDb(day.Id, registration.Id);
            DbContext.Participations.Add(participation);
        }
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/admin/registrations?Type=Organizer&EventId={eventEntity.Id}");
        var registrations = await response.Content.ReadFromJsonAsync<List<Registration>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(registrations);
        Assert.HasCount(1, registrations);
        Assert.AreEqual(RegistrationType.Organizer, registrations[0].Type);
    }
}