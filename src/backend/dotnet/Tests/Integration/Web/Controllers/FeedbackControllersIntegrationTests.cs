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
public class FeedbackControllersIntegrationTests : DatabaseIntegrationTestBase
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
    
    private async Task<RegistrationDb> CreateTestRegistrationAsync()
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
            new DateOnly(2025, 6, 1), 
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

        var registration = new RegistrationDb(
            Guid.NewGuid(), 
            eventEntity.Id, 
            user.Id, 
            RegistrationTypeDb.Standard, 
            true);
        DbContext.Registrations.Add(registration);
        await DbContext.SaveChangesAsync();
        
        return registration;
    }
    
    [TestMethod]
    public async Task GetFeedbacks_ShouldReturnList()
    {
        // Arrange
        var registration = await CreateTestRegistrationAsync();
        var feedback1 = new FeedbackDb(Guid.NewGuid(), registration.Id, "Good event", 5);
        var feedback2 = new FeedbackDb(Guid.NewGuid(), registration.Id, "Not bad", 4);
        DbContext!.Feedbacks.AddRange(feedback1, feedback2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/feedbacks");
        var feedbacks = await response.Content.ReadFromJsonAsync<List<Feedback>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(feedbacks);
        Assert.HasCount(2, feedbacks);
        Assert.IsTrue(feedbacks.Any(f => f.Rate == 5));
        Assert.IsTrue(feedbacks.Any(f => f.Rate == 4));
    }
    
    [TestMethod]
    public async Task CreateFeedback_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var registration = await CreateTestRegistrationAsync();
        var request = new CreateFeedbackRequest
        {
            RegistrationId = registration.Id,
            Comment = "Great experience!",
            Rate = 5
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/v1/feedbacks", request);
        var created = await response.Content.ReadFromJsonAsync<Feedback>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(request.Comment, created.Comment);
        Assert.AreEqual(request.Rate, created.Rate);
        
        var feedbackInDb = await DbContext!.Feedbacks.FindAsync(created.Id);
        Assert.IsNotNull(feedbackInDb);
    }
    
    [TestMethod]
    public async Task GetFeedbackById_WhenExists_ReturnsOk()
    {
        // Arrange
        var registration = await CreateTestRegistrationAsync();
        var feedback = new FeedbackDb(Guid.NewGuid(), registration.Id, "Nice", 4);
        DbContext!.Feedbacks.Add(feedback);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/feedbacks/{feedback.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Feedback>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(dto);
        Assert.AreEqual(feedback.Id, dto.Id);
        Assert.AreEqual(feedback.Comment, dto.Comment);
        Assert.AreEqual(feedback.Rate, dto.Rate);
    }
    
    [TestMethod]
    public async Task DeleteFeedback_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var registration = await CreateTestRegistrationAsync();
        var feedback = new FeedbackDb(Guid.NewGuid(), registration.Id, "To delete", 2);
        DbContext!.Feedbacks.Add(feedback);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/feedbacks/{feedback.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        DbContext.Entry(feedback).State = EntityState.Detached;
        var deleted = await DbContext.Feedbacks.FindAsync(feedback.Id);
        Assert.IsNull(deleted);
    }
}