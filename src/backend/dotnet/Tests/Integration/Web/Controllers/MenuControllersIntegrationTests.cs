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
public class MenuControllersIntegrationTests : DatabaseIntegrationTestBase
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
    public async Task GetMenus_ShouldReturnList()
    {
        // Arrange
        var menu1 = new MenuDb(
            Guid.NewGuid(), 
            "Breakfast", 
            "Morning meal");
        var menu2 = new MenuDb(
            Guid.NewGuid(), 
            "Lunch", 
            "Afternoon meal");
        DbContext!.Menus.AddRange(menu1, menu2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/menus");
        var menus = await response.Content.ReadFromJsonAsync<List<Menu>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(menus);
        Assert.HasCount(2, menus);
    }
    
    [TestMethod]
    public async Task GetMenus_WithTitleFilter_ShouldReturnFiltered()
    {
        // Arrange
        var menu1 = new MenuDb(
            Guid.NewGuid(), 
            "Breakfast", 
            "Morning");
        var menu2 = new MenuDb(
            Guid.NewGuid(), 
            "Brunch", 
            "Late morning");
        DbContext!.Menus.AddRange(menu1, menu2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            "/api/v1/menus?TitleContains=Break");
        var menus = await response.Content.ReadFromJsonAsync<List<Menu>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.HasCount(1, menus);
        Assert.AreEqual("Breakfast", menus[0].Title);
    }
    
    [TestMethod]
    public async Task GetMenuById_WhenExists_ReturnsOk()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Dinner", 
            "Evening meal");
        DbContext!.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/menus/{menu.Id}");
        var dto = await response.Content.ReadFromJsonAsync<Menu>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(dto);
        Assert.AreEqual(menu.Id, dto.Id);
        Assert.AreEqual(menu.Title, dto.Title);
    }

    [TestMethod]
    public async Task GetMenuById_WhenNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/menus/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetMenuItems_ReturnsList()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Test Menu", 
            "Desc");
        var item = new ItemDb(
            Guid.NewGuid(), 
            "Apple", 
            1.5m);
        DbContext!.Menus.Add(menu);
        DbContext.Items.Add(item);
        var menuItem = new MenuItemDb(menu.Id, item.Id, 10);
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/menus/{menu.Id}/items");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItem>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(items);
        Assert.HasCount(1, items);
        Assert.AreEqual(item.Id, items[0].ItemId);
        Assert.AreEqual(10, items[0].Amount);
    }
    
    [TestMethod]
    public async Task GetItemAmount_WhenExists_ReturnsAmount()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Menu", 
            "Desc");
        var item = new ItemDb(
            Guid.NewGuid(), 
            "Coffee", 
            2);
        DbContext!.Menus.Add(menu);
        DbContext.Items.Add(item);
        var menuItem = new MenuItemDb(menu.Id, item.Id, 7);
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/menus/{menu.Id}/items/{item.Id}/amount");
        var amount = await response.Content.ReadFromJsonAsync<int>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(7, amount);
    }
    
    [TestMethod]
    public async Task AdminCreateMenu_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateMenuRequest
        {
            Title = "Admin Menu",
            Description = "Test description"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/menus", request);
        var created = await response.Content.ReadFromJsonAsync<Menu>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(request.Title, created.Title);
        Assert.AreEqual(request.Description, created.Description);
        
        var menuInDb = await DbContext!.Menus.FindAsync(created.Id);
        Assert.IsNotNull(menuInDb);
    }
    
    [TestMethod]
    public async Task AdminCreateMenu_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMenuRequest
        {
            Title = "",
            Description = "Desc"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/menus", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task AdminUpdateMenu_WhenExists_ReturnsNoContentAndUpdates()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Old Title", 
            "Old Desc");
        DbContext!.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateMenuRequest
        {
            Title = "New Title",
            Description = "New Desc"
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/admin/menus/{menu.Id}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        DbContext.Entry(menu).Reload();
        Assert.AreEqual("New Title", menu.Title);
        Assert.AreEqual("New Desc", menu.Description);
    }
    
    [TestMethod]
    public async Task AdminUpdateMenu_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateMenuRequest
        {
            Title = "Ghost", 
            Description = "Ghost"
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/admin/menus/{Guid.NewGuid()}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task AdminDeleteMenu_WhenExists_ReturnsNoContentAndRemoves()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "ToDelete", 
            "Desc");
        DbContext!.Menus.Add(menu);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/menus/{menu.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        DbContext.Entry(menu).State = EntityState.Detached;
        var deleted = await DbContext.Menus.FindAsync(menu.Id);
        Assert.IsNull(deleted);
    }
    
    [TestMethod]
    public async Task AdminDeleteMenu_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/menus/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task AdminAddItem_Valid_ReturnsCreated()
    {
        // Arrange
        var menu = new MenuDb(Guid.NewGuid(), "Menu", "Desc");
        var item = new ItemDb(Guid.NewGuid(), "Item", 10);
        DbContext!.Menus.Add(menu);
        DbContext.Items.Add(item);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.PostAsync(
            $"/api/v1/admin/menus/{menu.Id}/items/{item.Id}?amount=5", 
            null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        
        var menuItem = await DbContext.MenuItems
            .FirstOrDefaultAsync(mi => mi.MenuId == menu.Id && mi.ItemId == item.Id);
        Assert.IsNotNull(menuItem);
        Assert.AreEqual(5, menuItem.Amount);
    }
    
    [TestMethod]
    public async Task AdminUpdateItemAmount_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var menu = new MenuDb(
            Guid.NewGuid(), 
            "Menu", 
            "Desc");
        var item = new ItemDb(
            Guid.NewGuid(), 
            "Item", 
            20);
        DbContext!.Menus.Add(menu);
        DbContext.Items.Add(item);
        var menuItem = new MenuItemDb(menu.Id, item.Id, 3);
        DbContext.MenuItems.Add(menuItem);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.PutAsync(
            $"/api/v1/admin/menus/{menu.Id}/items/{item.Id}?amount=15", 
            null);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        DbContext.Entry(menuItem).Reload();
        Assert.AreEqual(15, menuItem.Amount);
    }
}