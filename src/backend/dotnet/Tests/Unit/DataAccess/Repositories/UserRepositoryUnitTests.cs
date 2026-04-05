using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class UserRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistUser()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = UserFixture.Default()
            .WithName("John Doe")
            .WithPhone("+123456789")
            .WithGender(Gender.Male)
            .WithRole(UserRole.User)
            .WithPasswordHash("hash123")
            .Build();

        await repository.CreateAsync(user);

        var result = await repository.GetByIdAsync(user.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
        result.Phone.Should().Be("+123456789");
        result.Gender.Should().Be(Gender.Male);
        result.Role.Should().Be(UserRole.User);
        result.PasswordHash.Should().Be("hash123");
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = UserFixture.Default().Build();
        await repository.CreateAsync(user);

        var result = await repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [TestMethod]
    public async Task GetUsersAsync_ShouldReturnAllUsers_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user1 = UserFixture.Default().WithName("User1").Build();
        var user2 = UserFixture.Default().WithName("User2").Build();
        await repository.CreateAsync(user1);
        await repository.CreateAsync(user2);

        var result = await repository.GetUsersAsync();

        result.Should().HaveCount(2);
        result.Select(u => u.Name).Should().Contain(new[] { "User1", "User2" });
    }

    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByPhone()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user1 = UserFixture.Default().WithPhone("111111").Build();
        var user2 = UserFixture.Default().WithPhone("222222").Build();
        await repository.CreateAsync(user1);
        await repository.CreateAsync(user2);

        var filter = new UserFilter { Phone = "111111" };
        var result = await repository.GetUsersAsync(filter);

        result.Should().HaveCount(1);
        result.First().Phone.Should().Be("111111");
    }

    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByRole()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var admin = UserFixture.Default().WithRole(UserRole.Admin).Build();
        var user = UserFixture.Default().WithRole(UserRole.User).Build();
        await repository.CreateAsync(admin);
        await repository.CreateAsync(user);

        var filter = new UserFilter { Role = UserRole.Admin };
        var result = await repository.GetUsersAsync(filter);

        result.Should().HaveCount(1);
        result.First().Role.Should().Be(UserRole.Admin);
    }

    [TestMethod]
    public async Task GetUsersAsync_ShouldFilterByGender()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var male = UserFixture.Default().WithGender(Gender.Male).Build();
        var female = UserFixture.Default().WithGender(Gender.Female).Build();
        await repository.CreateAsync(male);
        await repository.CreateAsync(female);

        var filter = new UserFilter { Gender = Gender.Female };
        var result = await repository.GetUsersAsync(filter);

        result.Should().HaveCount(1);
        result.First().Gender.Should().Be(Gender.Female);
    }

    [TestMethod]
    public async Task GetUsersAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        for (int i = 1; i <= 5; i++)
        {
            var user = UserFixture.Default().WithName($"User{i}").Build();
            await repository.CreateAsync(user);
        }

        var filter = new UserFilter { PageNumber = 2, PageSize = 2 };
        var result = await repository.GetUsersAsync(filter);

        result.Should().HaveCount(2);
        // Проверка, что это вторая страница (имена должны быть User3 и User4, если сортировка по имени)
        result.Select(u => u.Name).Should().Contain(new[] { "User3", "User4" });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = UserFixture.Default()
            .WithName("Old Name")
            .WithPhone("111")
            .WithGender(Gender.Male)
            .WithRole(UserRole.User)
            .WithPasswordHash("oldhash")
            .Build();
        await repository.CreateAsync(user);

        var updated = UserFixture.Default()
            .WithId(user.Id)
            .WithName("New Name")
            .WithPhone("222")
            .WithGender(Gender.Female)
            .WithRole(UserRole.Admin)
            .WithPasswordHash("newhash")
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(user.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Phone.Should().Be("222");
        result.Gender.Should().Be(Gender.Female);
        result.Role.Should().Be(UserRole.Admin);
        result.PasswordHash.Should().Be("newhash");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = UserFixture.Default().Build();

        Func<Task> act = async () => await repository.UpdateAsync(user);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = UserFixture.Default().Build();
        await repository.CreateAsync(user);

        await repository.DeleteAsync(user.Id);

        var result = await repository.GetByIdAsync(user.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}