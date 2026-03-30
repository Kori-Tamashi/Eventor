using DataAccess.Converters;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Enums;
using Domain.Filters;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Core.DatabaseIntegration;
using Tests.Core.Fixtures;

namespace Tests.Integration.DataAccess.Repositories;

[TestClass]
[TestCategory("Integration")]
public class FeedbackRepositoryIntegrationTests : DatabaseIntegrationTestBase
{
    private FeedbackRepository _sutRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = NullLogger<FeedbackRepository>.Instance;
        _sutRepository = new FeedbackRepository(DbContext!, logger);
    }

    private async Task<Guid> CreateLocationAsync()
    {
        var location = new LocationDb(
            Guid.NewGuid(),
            "Test Location",
            "Description",
            100,
            50
        );

        DbContext!.Locations.Add(location);
        await DbContext.SaveChangesAsync();

        return location.Id;
    }

    private async Task<Guid> CreateEventAsync(Guid locationId)
    {
        var ev = new EventDb(
            Guid.NewGuid(),
            "Test Event",
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            locationId,
            1,
            0);

        DbContext!.Events.Add(ev);
        await DbContext.SaveChangesAsync();

        return ev.Id;
    }

    private async Task<Guid> CreateUserAsync(string phoneNumber = "+10000000000")
    {
        var user = new UserDb(
            Guid.NewGuid(),
            name: "Test User",
            phone: phoneNumber,
            gender: GenderDb.Male,
            role: UserRoleDb.User,
            passwordHash: "hashed_password"
        );

        DbContext!.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return user.Id;
    }
    
    private async Task<Guid> CreateRegistrationAsync(Guid userId, Guid eventId)
    {
        var registration = new RegistrationDb(
            Guid.NewGuid(),
            eventId,
            userId,
            RegistrationTypeConverter.ToDb(RegistrationType.Standard),
            true
        );

        DbContext!.Registrations.Add(registration);
        await DbContext.SaveChangesAsync();

        return registration.Id;
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPersist()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var registrationId = await CreateRegistrationAsync(userId, eventId);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Nice")
            .WithRate(4)
            .Build();

        await _sutRepository.CreateAsync(feedback);

        var db = await _sutRepository.GetByIdAsync(feedback.Id);

        db.Should().NotBeNull();
        db!.Comment.Should().Be("Nice");
        db.Rate.Should().Be(4);
        db.RegistrationId.Should().Be(registrationId);
    }
    
    [TestMethod]
    public async Task GetAsync_ShouldReturnAll()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var registrationId = await CreateRegistrationAsync(userId, eventId);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Nice")
            .WithRate(4)
            .Build();

        await _sutRepository.CreateAsync(feedback);

        var result = await _sutRepository.GetAsync();

        result.Should().HaveCountGreaterOrEqualTo(1);
    }
 
    [TestMethod]
    public async Task GetAsync_ShouldFilter_ByRegistrationId()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var registrationId = await CreateRegistrationAsync(userId, eventId);

        var feedback = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Nice")
            .WithRate(4)
            .Build();

        await _sutRepository.CreateAsync(feedback);

        var filter = new FeedbackFilter
        {
            RegistrationId = registrationId
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Should().OnlyContain(f => f.RegistrationId == registrationId);
    }

    [TestMethod]
    public async Task GetAsync_ShouldSort_Asc()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var registrationId = await CreateRegistrationAsync(userId, eventId);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Nice")
            .WithRate(4)
            .Build();
        
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Not bad")
            .WithRate(3)
            .Build();
        
        var feedback3 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Bad")
            .WithRate(2)
            .Build();

        await _sutRepository.CreateAsync(feedback1);
        await _sutRepository.CreateAsync(feedback2);
        await _sutRepository.CreateAsync(feedback3);

        var filter = new FeedbackFilter
        {
            SortByRate = FeedbackSortByRate.Asc
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Select(x => x.Rate).Should().BeInAscendingOrder();
    }

    [TestMethod]
    public async Task GetAsync_ShouldSort_Desc()
    {
        var userId = await CreateUserAsync();
        var locationId = await CreateLocationAsync();
        var eventId = await CreateEventAsync(locationId);
        var registrationId = await CreateRegistrationAsync(userId, eventId);

        var feedback1 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Nice")
            .WithRate(4)
            .Build();
        
        var feedback2 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Not bad")
            .WithRate(3)
            .Build();
        
        var feedback3 = FeedbackFixture.Default()
            .WithRegistrationId(registrationId)
            .WithComment("Bad")
            .WithRate(2)
            .Build();

        await _sutRepository.CreateAsync(feedback1);
        await _sutRepository.CreateAsync(feedback2);
        await _sutRepository.CreateAsync(feedback3);

        var filter = new FeedbackFilter
        {
            SortByRate = FeedbackSortByRate.Desc
        };

        var result = await _sutRepository.GetAsync(filter);

        result.Select(x => x.Rate).Should().BeInDescendingOrder();
    }
    
   [TestMethod]
   public async Task UpdateAsync_ShouldModify()
   {
       var userId = await CreateUserAsync();
       var locationId = await CreateLocationAsync();
       var eventId = await CreateEventAsync(locationId);
       var registrationId = await CreateRegistrationAsync(userId, eventId);

       var feedback = FeedbackFixture.Default()
           .WithRegistrationId(registrationId)
           .WithComment("Nice")
           .WithRate(4)
           .Build();

       await _sutRepository.CreateAsync(feedback);
       
       feedback.Rate = 2;
       feedback.Comment = "Updated";

       await _sutRepository.UpdateAsync(feedback);

       var db = await _sutRepository.GetByIdAsync(feedback.Id);

       db.Should().NotBeNull();
       db!.Comment.Should().Be("Updated");
       db.Rate.Should().Be(2);
   }

     [TestMethod]
     public async Task DeleteAsync_ShouldRemove()
     {
         var userId = await CreateUserAsync();
         var locationId = await CreateLocationAsync();
         var eventId = await CreateEventAsync(locationId);
         var registrationId = await CreateRegistrationAsync(userId, eventId);

         var feedback = FeedbackFixture.Default()
             .WithRegistrationId(registrationId)
             .WithComment("Nice")
             .WithRate(4)
             .Build();

         await _sutRepository.CreateAsync(feedback);

         await _sutRepository.DeleteAsync(feedback.Id);

         var db = await _sutRepository.GetByIdAsync(feedback.Id);

         db.Should().BeNull();
     }

     [TestMethod]
     public async Task DeleteAsync_ShouldThrow_WhenNotFound()
     {
         var id = Guid.NewGuid();

         var act = async () =>
             await _sutRepository.DeleteAsync(id);

         await act.Should().ThrowAsync<KeyNotFoundException>();
     }
}
