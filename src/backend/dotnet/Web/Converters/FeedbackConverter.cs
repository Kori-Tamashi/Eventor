using DomainFeedback = Domain.Models.Feedback;
using DomainRegistration = Domain.Models.Registration;
using Web.Dtos;

namespace Web.Converters;

public static class FeedbackConverter
{
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
