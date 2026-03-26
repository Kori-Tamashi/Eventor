using DomainFeedback = Domain.Models.Feedback;
using DomainRegistration = Domain.Models.Registration;
using Web.Dtos;

namespace Web.Converters;

public static class RegistrationAndFeedbackConverterExtensions
{
    public static Registration ToDto(this DomainRegistration model) => new()
    {
        Id = model.Id,
        EventId = model.EventId,
        UserId = model.UserId,
        Type = model.Type.ToDto(),
        Payment = model.Payment,
        Days = model.Days.Select(d => d.ToDto()).ToList()
    };

    public static DomainRegistration ToDomain(this CreateRegistrationRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EventId = request.EventId,
        UserId = request.UserId,
        Type = request.Type.ToDomain(),
        Payment = request.Payment
    };

    public static void ApplyToDomain(this UpdateRegistrationRequest request, DomainRegistration model)
    {
        if (request.Type.HasValue)
            model.Type = request.Type.Value.ToDomain();
        if (request.Payment.HasValue)
            model.Payment = request.Payment.Value;
    }

    public static Feedback ToDto(this DomainFeedback model) => new()
    {
        Id = model.Id,
        RegistrationId = model.RegistrationId,
        Comment = model.Comment,
        Rate = model.Rate
    };

    public static DomainFeedback ToDomain(this CreateFeedbackRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        RegistrationId = request.RegistrationId,
        Comment = request.Comment,
        Rate = request.Rate
    };

    public static void ApplyToDomain(this UpdateFeedbackRequest request, DomainFeedback model)
    {
        if (request.Comment is not null)
            model.Comment = request.Comment;
        if (request.Rate.HasValue)
            model.Rate = request.Rate.Value;
    }
}
