using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Models;
using Eventor.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.Application.Services;

[TestClass]
[TestCategory("Integration")]
public class FeedbackServiceIntegrationTests : DatabaseIntegrationTestBase
{
    private FeedbackService _sutService = null!;

    #region Вспомогательные методы создания связанных сущностей

    private async Task<Guid> CreateLocationAsync()
    {
        var location = LocationFixture.Default()
            .WithTitle("Test Location")
            .WithDescription("Location for feedback tests")
            .WithCost(100m)
            .WithCapacity(50)
            .Build();

        var locationRepo = new LocationRepository(DbContext!, NullLogger<LocationRepository>.Instance);
        var locationService = new LocationService(locationRepo);
        var created = await locationService.CreateAsync(location);
        return created.Id;
    }

    private async Task<Guid> CreateEventAsync(Guid locationId)
    {
        var ev = EventFixture.Default()
            .WithLocationId(locationId)
            .WithTitle("Test Event")
            .WithDescription("Event for feedback")
            .WithStartDate(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithDaysCount(1)
            .WithPercent(0)
            .Build();

        var eventRepo = new EventRepository(DbContext!, NullLogger<EventRepository>.Instance);
        var dayRepo = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        var registrationRepo = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        var eventService = new EventService(eventRepo, registrationRepo, dayRepo);
        var created = await eventService.CreateAsync(ev);
        return created.Id;
    }

    private async Task<Guid> CreateMenuAsync()
    {
        var menu = MenuFixture.Default()
            .WithTitle("Test Menu")
            .WithDescription("Menu for day")
            .Build();

        var menuRepo = new MenuRepository(DbContext!, NullLogger<MenuRepository>.Instance);
        var menuItemRepo = new MenuItemRepository(DbContext!, NullLogger<MenuItemRepository>.Instance);
        var menuService = new MenuService(menuRepo, menuItemRepo);
        var created = await menuService.CreateAsync(menu);
        return created.Id;
    }

    private async Task<Guid> CreateDayAsync(Guid eventId, Guid menuId, int sequenceNumber = 1)
    {
        var day = DayFixture.Default()
            .WithEventId(eventId)
            .WithMenuId(menuId)
            .WithTitle($"Day {sequenceNumber}")
            .WithSequenceNumber(sequenceNumber)
            .WithDescription("Day description")
            .Build();

        var dayRepo = new DayRepository(DbContext!, NullLogger<DayRepository>.Instance);
        var dayService = new DayService(dayRepo);
        var created = await dayService.CreateAsync(day);
        return created.Id;
    }

    private async Task<Guid> CreateUserAsync(string phone)
    {
        var user = UserFixture.Default()
            .WithName("Test User")
            .WithPhone(phone)
            .WithGender(Gender.Male)
            .WithRole(UserRole.User)
            .WithPasswordHash("hash")
            .Build();

        var userRepo = new UserRepository(DbContext!, NullLogger<UserRepository>.Instance);
        var userService = new UserService(userRepo);
        var created = await userService.CreateAsync(user);
        return created.Id;
    }

    private async Task<Guid> CreateRegistrationAsync(Guid eventId, Guid userId, bool payment = true, params Guid[] dayIds)
    {
        var registration = RegistrationFixture.Default()
            .WithEventId(eventId)
            .WithUserId(userId)
            .WithType(RegistrationType.Standard)
            .WithPayment(payment)
            .Build();

        var regRepo = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        var regDayRepo = new RegistrationDayRepository(DbContext!, NullLogger<RegistrationDayRepository>.Instance);
        var regService = new RegistrationService(regRepo, regDayRepo);
        var created = await regService.CreateAsync(registration, dayIds);
        return created.Id;
    }

    private async Task<Feedback> CreateValidFeedbackAsync()
    {
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();
        var dayId = await CreateDayAsync(eventId, menuId);
        var userId = await CreateUserAsync($"+{Guid.NewGuid():N}");
        var registrationId = await CreateRegistrationAsync(eventId, userId, true, dayId);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Great event!")
            .WithRate(5)
            .Build();

        return feedback;
    }

    #endregion

    [TestInitialize]
    public void Setup()
    {
        var feedbackRepo = new FeedbackRepository(DbContext!, NullLogger<FeedbackRepository>.Instance);
        var registrationRepo = new RegistrationRepository(DbContext!, NullLogger<RegistrationRepository>.Instance);
        _sutService = new FeedbackService(feedbackRepo, registrationRepo);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersistFeedback_AndGenerateId_WhenIdIsEmpty()
    {
        // Arrange
        var feedback = await CreateValidFeedbackAsync();
        feedback.Id = Guid.Empty;

        // Act
        var result = await _sutService.CreateAsync(feedback);

        // Assert
        result.Id.Should().NotBeEmpty();
        var saved = await _sutService.GetByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Comment.Should().Be(feedback.Comment);
        saved.Rate.Should().Be(feedback.Rate);
        saved.RegistrationId.Should().Be(feedback.RegistrationId);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepId_WhenIdIsProvided()
    {
        // Arrange
        var fixedId = Guid.NewGuid();
        var feedback = await CreateValidFeedbackAsync();
        feedback.Id = fixedId;

        // Act
        var result = await _sutService.CreateAsync(feedback);

        // Assert
        result.Id.Should().Be(fixedId);
        var saved = await _sutService.GetByIdAsync(fixedId);
        saved.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnFeedback_WhenExists()
    {
        // Arrange
        var feedback = await CreateValidFeedbackAsync();
        await _sutService.CreateAsync(feedback);

        // Act
        var result = await _sutService.GetByIdAsync(feedback.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(feedback.Id);
        result.Comment.Should().Be(feedback.Comment);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _sutService.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllFeedbacks_WhenNoFilter()
    {
        // Arrange
        var feedback1 = await CreateValidFeedbackAsync();
        var feedback2 = await CreateValidFeedbackAsync();
        await _sutService.CreateAsync(feedback1);
        await _sutService.CreateAsync(feedback2);

        // Act
        var result = await _sutService.GetAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Select(f => f.Id).Should().Contain([feedback1.Id, feedback2.Id]);
    }

    [TestMethod]
    public async Task GetAsync_ShouldFilterByRegistrationId()
    {
        // Arrange
        var feedback1 = await CreateValidFeedbackAsync();
        var feedback2 = await CreateValidFeedbackAsync();
        await _sutService.CreateAsync(feedback1);
        await _sutService.CreateAsync(feedback2);

        var filter = new FeedbackFilter { RegistrationId = feedback1.RegistrationId };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result[0].RegistrationId.Should().Be(feedback1.RegistrationId);
    }

    [TestMethod]
    public async Task GetAsync_ShouldSortByRateAscending()
    {
        // Arrange
        var feedbackLow = await CreateValidFeedbackAsync();
        feedbackLow.Rate = 2;
        var feedbackHigh = await CreateValidFeedbackAsync();
        feedbackHigh.Rate = 5;

        await _sutService.CreateAsync(feedbackLow);
        await _sutService.CreateAsync(feedbackHigh);

        var filter = new FeedbackFilter { SortByRate = FeedbackSortByRate.Asc };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Select(f => f.Rate).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldSortByRateDescending()
    {
        // Arrange
        var feedbackLow = await CreateValidFeedbackAsync();
        feedbackLow.Rate = 2;
        var feedbackHigh = await CreateValidFeedbackAsync();
        feedbackHigh.Rate = 5;

        await _sutService.CreateAsync(feedbackLow);
        await _sutService.CreateAsync(feedbackHigh);

        var filter = new FeedbackFilter { SortByRate = FeedbackSortByRate.Desc };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Select(f => f.Rate).Should().BeInDescendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldApplyPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var f = await CreateValidFeedbackAsync();
            await _sutService.CreateAsync(f);
        }

        var filter = new FeedbackFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetByEventIdAsync_ShouldReturnFeedbacksForGivenEvent()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();
        var dayId = await CreateDayAsync(eventId, menuId);

        var userId1 = await CreateUserAsync("+111111111");
        var userId2 = await CreateUserAsync("+222222222");

        var regId1 = await CreateRegistrationAsync(eventId, userId1, true, dayId);
        var regId2 = await CreateRegistrationAsync(eventId, userId2, true, dayId);

        var feedback1 = FeedbackFixture.Default().WithRegistrationId(regId1).WithComment("Good").WithRate(4).Build();
        var feedback2 = FeedbackFixture.Default().WithRegistrationId(regId2).WithComment("Excellent").WithRate(5).Build();

        await _sutService.CreateAsync(feedback1);
        await _sutService.CreateAsync(feedback2);

        // Act
        var result = await _sutService.GetByEventIdAsync(eventId);

        // Assert
        result.Should().HaveCount(2);
        result.Select(f => f.RegistrationId).Should().Contain([regId1, regId2]);
    }

    [TestMethod]
    public async Task GetByEventIdAsync_ShouldApplyPagination()
    {
        // Arrange
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var menuId = await CreateMenuAsync();
        var dayId = await CreateDayAsync(eventId, menuId);

        for (int i = 1; i <= 5; i++)
        {
            var userId = await CreateUserAsync($"+{i:D10}");
            var regId = await CreateRegistrationAsync(eventId, userId, true, dayId);
            var feedback = FeedbackFixture.Default()
                .WithRegistrationId(regId)
                .WithComment($"Feedback {i}")
                .WithRate(i)
                .Build();
            await _sutService.CreateAsync(feedback);
        }

        var filter = new PaginationFilter { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sutService.GetByEventIdAsync(eventId, filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateExistingFeedback()
    {
        // Arrange
        var feedback = await CreateValidFeedbackAsync();
        await _sutService.CreateAsync(feedback);

        var updated = new Feedback
        {
            Id = feedback.Id,
            RegistrationId = feedback.RegistrationId,
            Comment = "Updated comment",
            Rate = 1
        };

        // Act
        await _sutService.UpdateAsync(updated);

        // Assert
        var result = await _sutService.GetByIdAsync(feedback.Id);
        result.Should().NotBeNull();
        result!.Comment.Should().Be("Updated comment");
        result.Rate.Should().Be(1);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowFeedbackNotFoundException_WhenFeedbackDoesNotExist()
    {
        // Arrange
        var feedback = await CreateValidFeedbackAsync();

        // Act
        Func<Task> act = async () => await _sutService.UpdateAsync(feedback);

        // Assert
        await act.Should().ThrowAsync<FeedbackNotFoundException>()
            .WithMessage($"Feedback '{feedback.Id}' was not found.");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldRemoveExistingFeedback()
    {
        // Arrange
        var feedback = await CreateValidFeedbackAsync();
        await _sutService.CreateAsync(feedback);

        // Act
        await _sutService.DeleteAsync(feedback.Id);

        // Assert
        var result = await _sutService.GetByIdAsync(feedback.Id);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowFeedbackNotFoundException_WhenFeedbackDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sutService.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<FeedbackNotFoundException>()
            .WithMessage($"Feedback '{id}' was not found.");
    }
}