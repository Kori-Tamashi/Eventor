using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Web.Dtos;

namespace Tests.E2E.Auth;

[TestClass]
[TestCategory("E2E")]
public class AuthE2ETests
{
    private HttpClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        var baseUrl =
            Environment.GetEnvironmentVariable("E2E_BASE_URL")
            ?? "http://localhost:5215";

        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
    }

    [TestMethod]
    public async Task Test()
    {
        // -----------------------
        // 1. REGISTER
        // -----------------------
        var registerRequest = new RegisterRequest
        {
            Name = "E2E User",
            Phone = "+10000000001",
            Gender = Gender.Male,
            Password = "Password123!"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            registerRequest);

        Assert.AreEqual(HttpStatusCode.Created, registerResponse.StatusCode);

        // -----------------------
        // 2. LOGIN
        // -----------------------
        
        var loginRequest = new LoginRequest
        {
            Phone = registerRequest.Phone,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            loginRequest);

        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.IsNotNull(loginBody);
        Assert.IsTrue(loginBody.ContainsKey("token"));

        var token = loginBody["token"];
        Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        
        // -----------------------
        // 3. AUTH REQUEST (/me)
        // -----------------------

        var meResponse = await _client.GetAsync("/api/v1/users/me");

        Assert.AreEqual(HttpStatusCode.OK, meResponse.StatusCode);

        var user = await meResponse.Content.ReadFromJsonAsync<User>();

        Assert.IsNotNull(user);
        Assert.AreEqual(registerRequest.Name, user.Name);
        Assert.AreEqual(registerRequest.Phone, user.Phone);
        
        var userId = user!.Id;
        
        // -----------------------
        // 4. CREATE LOCATION
        // -----------------------
        var createLocationRequest = new CreateLocationRequest
        {
            Title = "E2E Location",
            Description = "Test location",
            Cost = 100,
            Capacity = 50
        };

        var locationResponse = await _client.PostAsJsonAsync(
            "/api/v1/locations",
            createLocationRequest);

        Assert.AreEqual(HttpStatusCode.Created, locationResponse.StatusCode);

        var location = await locationResponse.Content
            .ReadFromJsonAsync<Location>();

        Assert.IsNotNull(location);

        var locationId = location!.Id;
        
        // -----------------------
        // 5. CREATE EVENT
        // -----------------------
        var createEventRequest = new CreateEventRequest
        {
            Title = "E2E Event",
            Description = "Demo event",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            LocationId = locationId,
            DaysCount = 2,
            Percent = 10,
            CreatedByUserId = userId
        };

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/v1/events",
            createEventRequest);

        Assert.AreEqual(HttpStatusCode.Created, eventResponse.StatusCode);

        var createdEvent = await eventResponse.Content
            .ReadFromJsonAsync<Event>();

        Assert.IsNotNull(createdEvent);
        Assert.AreEqual(createEventRequest.Title, createdEvent!.Title);
        
        // -----------------------
        // 6. CREATE MENU
        // -----------------------
        var createMenuRequest = new CreateMenuRequest
        {
            Title = "E2E Menu",
            Description = "Demo menu"
        };

        var menuResponse = await _client.PostAsJsonAsync(
            "/api/v1/admin/menus",
            createMenuRequest);

        Assert.AreEqual(HttpStatusCode.Created, menuResponse.StatusCode);

        var menu = await menuResponse.Content
            .ReadFromJsonAsync<Menu>();

        Assert.IsNotNull(menu);

        var menuId = menu!.Id;
        
        // -----------------------
        // 7. CREATE ITEMS
        // -----------------------
        var item1Request = new CreateItemRequest
        {
            Title = "Water",
            Cost = 10
        };

        var item2Request = new CreateItemRequest
        {
            Title = "Pizza",
            Cost = 25
        };

        var item1Response = await _client.PostAsJsonAsync(
            "/api/v1/items", item1Request);
        var item2Response = await _client.PostAsJsonAsync(
            "/api/v1/items", item2Request);

        Assert.AreEqual(HttpStatusCode.Created, item1Response.StatusCode);
        Assert.AreEqual(HttpStatusCode.Created, item2Response.StatusCode);

        var item1 = await item1Response.Content.ReadFromJsonAsync<Item>();
        var item2 = await item2Response.Content.ReadFromJsonAsync<Item>();

        Assert.IsNotNull(item1);
        Assert.IsNotNull(item2);
        
        // -----------------------
        // 8. ADD ITEMS TO MENU
        // -----------------------
        var addItem1 = await _client.PostAsync(
            $"/api/v1/admin/menus/{menuId}/items/{item1!.Id}?amount=2",
            null);

        var addItem2 = await _client.PostAsync(
            $"/api/v1/admin/menus/{menuId}/items/{item2!.Id}?amount=1",
            null);

        Assert.AreEqual(HttpStatusCode.Created, addItem1.StatusCode);
        Assert.AreEqual(HttpStatusCode.Created, addItem2.StatusCode);
        
        // -----------------------
        // 9. CREATE DAY
        // -----------------------
        var createDayRequest = new CreateDayRequest
        {
            Title = "Day 1",
            Description = "Opening day",
            SequenceNumber = 1,
            MenuId = menuId
        };

        var dayResponse = await _client.PostAsJsonAsync(
            $"/api/v1/events/{createdEvent!.Id}/days",
            createDayRequest);

        Assert.AreEqual(HttpStatusCode.Created, dayResponse.StatusCode);

        var day = await dayResponse.Content
            .ReadFromJsonAsync<Day>();

        Assert.IsNotNull(day);
        
        var daysResponse = await _client.GetAsync(
            $"/api/v1/events/{createdEvent!.Id}/days");

        Assert.AreEqual(HttpStatusCode.OK, daysResponse.StatusCode);
    }
}