using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
public class UserRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private UserRepository _sutRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<UserRepository>.Instance;
        _sutRepository = new UserRepository(DbContext!, logger);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSaveUserToDatabase()
    {
        var user = UserFixture.Default().Build();
        
        await _sutRepository.CreateAsync(user);

        var savedUser = await _sutRepository.GetByIdAsync(user.Id);
        
        savedUser.Should().NotBeNull();
        savedUser!.Id.Should().Be(user.Id);
        savedUser.Name.Should().Be(user.Name);
        savedUser.Phone.Should().Be(user.Phone);
        savedUser.Gender.Should().Be(user.Gender);
        savedUser.Role.Should().Be(user.Role);
        savedUser.PasswordHash.Should().Be(user.PasswordHash);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _sutRepository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        var users = new[]
        {
            UserFixture.Default()
                .WithName("Test User A")
                .WithPhone("+1111111111")
                .Build(),

            UserFixture.Default()
                .WithName("Test User B")
                .WithPhone("+2222222222")
                .Build()
        };

        foreach (var user in users)
            await _sutRepository.CreateAsync(user);

        var result = await _sutRepository.GetUsersAsync();

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByName()
    {
        var users = new[]
        {
            UserFixture.Default()
                .WithName("Иван Иванов")
                .WithPhone("+1111111111")
                .Build(),
            
            UserFixture.Default()
                .WithName("Петр Петров")
                .WithPhone("+2222222222")
                .Build(),

            UserFixture.Default()
                .WithName("Иван Петров")
                .WithPhone("+3333333333")
                .Build()
        };

        foreach (var user in users)
            await _sutRepository.CreateAsync(user);

        var filter = new UserFilter
        {
            NameContains = "Пет"
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().HaveCount(2);

        result.Select(x => x.Name)
            .Should()
            .Contain([
                "Петр Петров",
                "Иван Петров"
            ]);

        result.Select(x => x.Name)
            .Should()
            .NotContain("Иван Иванов");
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByPhone()
    {
        var users = new[]
        {
            UserFixture.Default()
                .WithName("User A")
                .WithPhone("+1111111111")
                .Build(),

            UserFixture.Default()
                .WithName("User B")
                .WithPhone("+2222222222")
                .Build()
        };

        foreach (var user in users)
            await _sutRepository.CreateAsync(user);

        var filter = new UserFilter
        {
            Phone = "+2222222222"
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().HaveCount(1);
        result[0].Phone.Should().Be("+2222222222");
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByRole()
    {
        var admin = UserFixture.Default()
            .WithName("Admin")
            .WithPhone("+1111111111")
            .WithRole(UserRole.Admin)
            .Build();

        var user = UserFixture.Default()
            .WithName("User")
            .WithPhone("+2222222222")
            .WithRole(UserRole.User)
            .Build();

        await _sutRepository.CreateAsync(admin);
        await _sutRepository.CreateAsync(user);

        var filter = new UserFilter
        {
            Role = UserRole.Admin
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().ContainSingle();
        result[0].Role.Should().Be(UserRole.Admin);
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByGender()
    {
        var male = UserFixture.Default()
            .WithName("Male User")
            .WithPhone("+1111111111")
            .WithGender(Gender.Male)
            .Build();

        var female = UserFixture.Default()
            .WithName("Female User")
            .WithPhone("+2222222222")
            .WithGender(Gender.Female)
            .Build();

        await _sutRepository.CreateAsync(male);
        await _sutRepository.CreateAsync(female);

        var filter = new UserFilter
        {
            Gender = Gender.Female
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().ContainSingle();
        result[0].Gender.Should().Be(Gender.Female);
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByMultipleCriteria()
    {
        var target = UserFixture.Default()
            .WithName("Иван Петров")
            .WithPhone("+9999999999")
            .WithRole(UserRole.Admin)
            .WithGender(Gender.Male)
            .Build();

        var other = UserFixture.Default()
            .WithName("Иван Иванов")
            .WithPhone("+8888888888")
            .WithRole(UserRole.User)
            .WithGender(Gender.Male)
            .Build();

        await _sutRepository.CreateAsync(target);
        await _sutRepository.CreateAsync(other);

        var filter = new UserFilter
        {
            NameContains = "Пет",
            Role = UserRole.Admin,
            Gender = Gender.Male
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Иван Петров");
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldApplyPagination()
    {
        var users = Enumerable.Range(1, 5)
            .Select(i => UserFixture.Default()
                .WithName($"User {i}")
                .WithPhone($"+100000000{i}")
                .Build())
            .ToArray();

        foreach (var user in users)
            await _sutRepository.CreateAsync(user);

        var filter = new UserFilter
        {
            PageNumber = 2,
            PageSize = 2
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().HaveCount(2);
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldReturnUsersOrderedByName()
    {
        var user1 = UserFixture.Default()
            .WithName("Charlie")
            .WithPhone("+1111111111")
            .Build();

        var user2 = UserFixture.Default()
            .WithName("Alice")
            .WithPhone("+2222222222")
            .Build();

        var user3 = UserFixture.Default()
            .WithName("Bob")
            .WithPhone("+3333333333")
            .Build();

        await _sutRepository.CreateAsync(user1);
        await _sutRepository.CreateAsync(user2);
        await _sutRepository.CreateAsync(user3);

        var result = await _sutRepository.GetUsersAsync();

        result.Select(x => x.Name)
            .Should()
            .BeInAscendingOrder();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var user = UserFixture.Default().Build();
        await _sutRepository.CreateAsync(user);

        var updated = UserFixture.Default()
            .WithId(user.Id)
            .WithName("Updated Name")
            .WithPhone("+9999999999")
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Phone.Should().Be("+9999999999");
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenUserNotFound()
    {
        var user = UserFixture.Default().Build();

        var act = async () => await _sutRepository.UpdateAsync(user);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var user = UserFixture.Default().Build();
        await _sutRepository.CreateAsync(user);

        await _sutRepository.DeleteAsync(user.Id);

        var result = await _sutRepository.GetByIdAsync(user.Id);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenUserNotFound()
    {
        var act = async () => 
            await _sutRepository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldReturnAll_WhenFilterIsNull()
    {
        var user = UserFixture.Default()
            .WithPhone("+1111111111")
            .Build();

        await _sutRepository.CreateAsync(user);

        var result = await _sutRepository.GetUsersAsync(null);

        result.Should().ContainSingle();
    }
    
    [TestMethod]
    public async Task GetUsersAsync_ShouldReturnEmpty_WhenPageOutOfRange()
    {
        var users = Enumerable.Range(1, 3)
            .Select(i => UserFixture.Default()
                .WithName($"User {i}")
                .WithPhone($"+100000000{i}")
                .Build());

        foreach (var user in users)
            await _sutRepository.CreateAsync(user);

        var filter = new UserFilter
        {
            PageNumber = 10,
            PageSize = 2
        };

        var result = await _sutRepository.GetUsersAsync(filter);

        result.Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateAllFields()
    {
        var user = UserFixture.Default().Build();
        await _sutRepository.CreateAsync(user);

        var updated = UserFixture.Default()
            .WithId(user.Id)
            .WithName("Updated Name")
            .WithPhone("+9999999999")
            .WithRole(UserRole.Admin)
            .WithGender(Gender.Female)
            .Build();

        await _sutRepository.UpdateAsync(updated);

        var result = await _sutRepository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Phone.Should().Be("+9999999999");
        result.Role.Should().Be(UserRole.Admin);
        result.Gender.Should().Be(Gender.Female);
    }
    
    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveUserFromList()
    {
        var user = UserFixture.Default().Build();
        await _sutRepository.CreateAsync(user);

        await _sutRepository.DeleteAsync(user.Id);

        var users = await _sutRepository.GetUsersAsync();

        users.Should().NotContain(x => x.Id == user.Id);
    }
}