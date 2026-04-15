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
public class ItemControllersIntegrationTests : DatabaseIntegrationTestBase
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
    
    [TestMethod]
    public async Task GetItems_ShouldReturnList()
    {
        // Arrange
        var item1 = new ItemDb(
            Guid.NewGuid(), 
            "Water", 
            1500);
        var item2 = new ItemDb(
            Guid.NewGuid(), 
            "Pizzs", 
            25);
        DbContext!.Items.AddRange(item1, item2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/items");
        var items = await response.Content.ReadFromJsonAsync<List<Item>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(items);
        Assert.HasCount(2, items);
    }
    
    [TestMethod]
    public async Task GetItemById_WhenExists_ReturnsOk()
    {
        // Arrange
        var item = new ItemDb(
            Guid.NewGuid(), 
            "A", 
            80);
        DbContext!.Items.Add(item);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/items/{item.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Item>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(dto);
        Assert.AreEqual(item.Id, dto.Id);
        Assert.AreEqual(item.Title, dto.Title);
    }

    [TestMethod]
    public async Task GetItemById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/items/{Guid.NewGuid()}");
        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CreateItem_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "Item",
            Cost = 350
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/items", request);
        var created = await response.Content.ReadFromJsonAsync<Item>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(request.Title, created.Title);
        Assert.AreEqual(request.Cost, created.Cost);

        // Проверка в БД
        var itemInDb = await DbContext!.Items.FindAsync(created.Id);
        Assert.IsNotNull(itemInDb);
    }

    [TestMethod]
    public async Task CreateItem_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "",
            Cost = -10
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/items", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task AdminCreateItem_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "Admin Item",
            Cost = 999
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/items", request);
        var created = await response.Content.ReadFromJsonAsync<Item>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(request.Title, created.Title);
        Assert.AreEqual(request.Cost, created.Cost);

        var itemInDb = await DbContext!.Items.FindAsync(created.Id);
        Assert.IsNotNull(itemInDb);
    }
    
     [TestMethod]
    public async Task AdminCreateItem_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "",
            Cost = -5
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/items", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task AdminUpdateItem_WhenExists_ReturnsNoContentAndUpdates()
    {
        // Arrange
        var item = new ItemDb(Guid.NewGuid(), "Old Name", 100);
        DbContext!.Items.Add(item);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateItemRequest
        {
            Title = "New Name",
            Cost = 200
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/items/{item.Id}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        DbContext.Entry(item).Reload();
        Assert.AreEqual("New Name", item.Title);
        Assert.AreEqual(200, item.Cost);
    }

    [TestMethod]
    public async Task AdminUpdateItem_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateItemRequest { Title = "Ghost", Cost = 0 };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/items/{Guid.NewGuid()}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AdminDeleteItem_WhenExists_ReturnsNoContentAndRemoves()
    {
        // Arrange
        var item = new ItemDb(Guid.NewGuid(), "ToDelete", 50);
        DbContext!.Items.Add(item);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/items/{item.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        DbContext.Entry(item).State = EntityState.Detached;
        var deleted = await DbContext.Items.FindAsync(item.Id);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public async Task AdminDeleteItem_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/items/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}