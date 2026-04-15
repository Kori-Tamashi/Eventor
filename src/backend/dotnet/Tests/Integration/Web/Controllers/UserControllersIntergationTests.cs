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
public class UserControllersIntergationTests : DatabaseIntegrationTestBase
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
    public async Task GetAsync_WhenUsersExist_ReturnsOkWithList()
    {
        // Arrange
        var user1 = new UserDb(
            id: Guid.NewGuid(),
            name: "Alice",
            phone: "+1234567890",
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "test_hash"
        );
    
        var user2 = new UserDb(
            id: Guid.NewGuid(),
            name: "Bob",
            phone: "+0987654321",
            gender: GenderDb.Female,
            role: UserRoleDb.Admin,
            passwordHash: "test_hash_2"
        );
        DbContext!.Users.Add(user1);
        DbContext!.Users.Add(user2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/admin/users");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(users);
        Assert.HasCount(2, users);
        Assert.IsTrue(users.Any(u => u.Name == "Alice"));
        Assert.IsTrue(users.Any(u => u.Name == "Bob"));
    }
    
    [TestMethod]
    public async Task GetAsync_WhenNoUsers_ReturnsEmptyList()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/admin/users");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(users);
        Assert.IsEmpty(users);
    }
    
    [TestMethod]
    public async Task GetAsync_WhenOneUserExists_ReturnsSingleUser()
    {
        // Arrange
        var user = new UserDb(
            id: Guid.NewGuid(),
            name: "Alice",
            phone: "+1111111111",
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "hash"
        );
        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/admin/users");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.HasCount(1, users);
        Assert.AreEqual("Alice", users[0].Name);
    }
    
    [TestMethod]
    public async Task GetAsync_FilterByName_ReturnsMatchingUsers()
    {
        // Arrange
        var alice1 = new UserDb(
            Guid.NewGuid(), 
            "Alice", 
            "+1", 
            GenderDb.Female, 
            UserRoleDb.User, 
            "hash");
        var alice2 = new UserDb(
            Guid.NewGuid(), 
            "Alice Ivanova", 
            "+2", 
            GenderDb.Female, 
            UserRoleDb.User, 
            "hash");
        var bob = new UserDb(
            Guid.NewGuid(), 
            "Bob", 
            "+3", 
            GenderDb.Male, 
            UserRoleDb.User, 
            "hash");
        DbContext!.Users.AddRange(alice1, alice2, bob);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            "/api/v1/admin/users?NameContains=Alice");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.HasCount(2, users);
        Assert.IsTrue(users.All(u => u.Name.Contains("Alice")));
    }
    
    [TestMethod]
    public async Task GetAsync_FilterByRole_ReturnsMatchingUsers()
    {
        // Arrange
        var admin = new UserDb(
            Guid.NewGuid(), 
            "Admin", 
            "+1", 
            GenderDb.Male, 
            UserRoleDb.Admin, 
            "hash");
        var user = new UserDb(
            Guid.NewGuid(), 
            "User", 
            "+2", 
            GenderDb.Female, 
            UserRoleDb.User, 
            "hash");
        DbContext!.Users.AddRange(admin, user);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync(
            "/api/v1/admin/users?Role=Admin");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.HasCount(1, users);
    }
    
    [TestMethod]
    public async Task GetById_WhenUserExists_ReturnsOkWithUser()
    {
        // Arrange
        var user = new UserDb(
            id: Guid.NewGuid(),
            name: "Test User",
            phone: "+1234567890",
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "hash123"
        );
        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/admin/users/{user.Id}");
        var result = await response.Content.ReadFromJsonAsync<User>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Name, result.Name);
        Assert.AreEqual(user.Phone, result.Phone);
    }

    [TestMethod]
    public async Task GetById_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/admin/users/{nonExistentId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task Create_ValidUser_ReturnsCreatedWithUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "New User",
            Phone = "+79991112233",
            Gender = Gender.Male,
            Role = UserRole.User,
            Password = "StrongPass123!"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/users", request);
        var createdUser = await response.Content.ReadFromJsonAsync<User>();

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(createdUser);
        Assert.AreNotEqual(Guid.Empty, createdUser.Id);
        Assert.AreEqual(request.Name, createdUser.Name);
        Assert.AreEqual(request.Phone, createdUser.Phone);
        Assert.AreEqual(request.Gender, createdUser.Gender);
        Assert.AreEqual(request.Role, createdUser.Role);
        Assert.IsNotNull(createdUser.PasswordHash); // хеш не должен быть null
        
        var userInDb = await DbContext!.Users.FindAsync(createdUser.Id);
        Assert.IsNotNull(userInDb);
        Assert.AreEqual(request.Name, userInDb.Name);
    }
    
    [TestMethod]
    public async Task Update_WhenUserExists_ReturnsNoContentAndUpdatesUser()
    {
        // Arrange
        var user = new UserDb(
            id: Guid.NewGuid(),
            name: "Old Name",
            phone: "+1111111111",
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "old_hash"
        );
        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateUserRequest
        {
            Name = "New Name",
            Phone = "+2222222222",
            Gender = Gender.Female,
            Role = UserRole.Admin,
            Password = "NewPassword123"
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/admin/users/{user.Id}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        
        DbContext.Entry(user).Reload();
    
        Assert.AreEqual("New Name", user.Name);
        Assert.AreEqual("+2222222222", user.Phone);
    }
    
    [TestMethod]
    public async Task Update_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Name = "Any Name",
            Phone = "+0000000000",
            Gender = Gender.Male,
            Role = UserRole.User
            // Password не передаём (null)
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/admin/users/{nonExistentId}", updateRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task Delete_WhenUserExists_ReturnsNoContentAndRemovesUser()
    {
        // Arrange
        var user = new UserDb(
            id: Guid.NewGuid(),
            name: "ToDelete",
            phone: "+1234567890",
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "hash"
        );
        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/admin/users/{user.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        DbContext.Entry(user).State = EntityState.Detached;
        var deletedUser = await DbContext.Users.FindAsync(user.Id);
        Assert.IsNull(deletedUser);
    }
    
    [TestMethod]
    public async Task Delete_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/admin/users/{nonExistentId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    public TestContext TestContext { get; set; }
}