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
using DataAccess.Converters;

namespace Tests.Unit.DataAccess.Repositories;

[TestClass]
[TestCategory("Unit")]
public class FeedbackRepositoryUnitTests
{
    private EventorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EventorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EventorDbContext(options);
    }

    private async Task<(Guid userId, Guid eventId, Guid registrationId)> CreateRegistrationAsync(EventorDbContext context)
    {
        // Создаём Location
        var location = new LocationDb(
            Guid.NewGuid(),
            "Test Location",
            "Description",
            100,
            50
        );
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        // Создаём Event
        var ev = new EventDb(
            Guid.NewGuid(),
            "Test Event",
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            location.Id,
            1,
            0);
        context.Events.Add(ev);
        await context.SaveChangesAsync();

        // Создаём User
        var user = new UserDb(
            Guid.NewGuid(),
            "Test User",
            "+1234567890",
            GenderConverter.ToDb(Gender.Male),
            UserRoleConverter.ToDb(UserRole.User),
            "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Создаём Registration
        var registration = new RegistrationDb(
            Guid.NewGuid(),
            ev.Id,
            user.Id,
            RegistrationTypeConverter.ToDb(RegistrationType.Standard),
            false);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        return (user.Id, ev.Id, registration.Id);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistFeedback()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Great event!")
            .WithRate(5)
            .Build();

        await repository.CreateAsync(feedback);

        var result = await repository.GetByIdAsync(feedback.Id);
        result.Should().NotBeNull();
        result!.Comment.Should().Be("Great event!");
        result.Rate.Should().Be(5);
        result.RegistrationId.Should().Be(registrationId);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnFeedback_WhenExists()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .Build();
        await repository.CreateAsync(feedback);

        var result = await repository.GetByIdAsync(feedback.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(feedback.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllFeedbacks_WhenNoFilter()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId1) = await CreateRegistrationAsync(context);
        var (_, _, registrationId2) = await CreateRegistrationAsync(context);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId1)
            .WithRate(5)
            .Build();
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId2)
            .WithRate(4)
            .Build();

        await repository.CreateAsync(feedback1);
        await repository.CreateAsync(feedback2);

        var result = await repository.GetAsync();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByRegistrationId()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId1) = await CreateRegistrationAsync(context);
        var (_, _, registrationId2) = await CreateRegistrationAsync(context);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId1)
            .Build();
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId2)
            .Build();

        await repository.CreateAsync(feedback1);
        await repository.CreateAsync(feedback2);

        var filter = new FeedbackFilter { RegistrationId = registrationId1 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(1);
        result.All(f => f.RegistrationId == registrationId1).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetAsync_ShouldSortByRateAscending()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(3)
            .Build();
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(5)
            .Build();
        var feedback3 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(1)
            .Build();

        await repository.CreateAsync(feedback1);
        await repository.CreateAsync(feedback2);
        await repository.CreateAsync(feedback3);

        var filter = new FeedbackFilter { SortByRate = FeedbackSortByRate.Asc };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(3);
        result.Select(f => f.Rate).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldSortByRateDescending()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(3)
            .Build();
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(5)
            .Build();
        var feedback3 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithRate(1)
            .Build();

        await repository.CreateAsync(feedback1);
        await repository.CreateAsync(feedback2);
        await repository.CreateAsync(feedback3);

        var filter = new FeedbackFilter { SortByRate = FeedbackSortByRate.Desc };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(3);
        result.Select(f => f.Rate).Should().BeInDescendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        for (int i = 1; i <= 5; i++)
        {
            var feedback = FeedbackFixture.Default()
                .WithRegistrationId(registrationId)
                .WithRate(i)
                .Build();
            await repository.CreateAsync(feedback);
        }

        var filter = new FeedbackFilter { PageNumber = 2, PageSize = 2 };
        var result = await repository.GetAsync(filter);

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateFeedback()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Old comment")
            .WithRate(3)
            .Build();
        await repository.CreateAsync(feedback);

        var updated = FeedbackFixture.Default()
            .WithId(feedback.Id)
            .WithRegistrationId(registrationId)
            .WithComment("New comment")
            .WithRate(5)
            .Build();

        await repository.UpdateAsync(updated);

        var result = await repository.GetByIdAsync(feedback.Id);
        result.Should().NotBeNull();
        result!.Comment.Should().Be("New comment");
        result.Rate.Should().Be(5);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .Build();

        Func<Task> act = async () => await repository.UpdateAsync(feedback);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveFeedback()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);
        var (_, _, registrationId) = await CreateRegistrationAsync(context);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .Build();
        await repository.CreateAsync(feedback);

        await repository.DeleteAsync(feedback.Id);

        var result = await repository.GetByIdAsync(feedback.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context, NullLogger<FeedbackRepository>.Instance);

        Func<Task> act = async () => await repository.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}