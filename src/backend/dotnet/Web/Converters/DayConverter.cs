using DomainDay = Domain.Models.Day;
using Web.Dtos;

namespace Web.Converters;

public static class DayConverter
{
    public static Day ToDto(this DomainDay model) => new()
    {
        Id = model.Id,
        EventId = model.EventId,
        MenuId = model.MenuId,
        Title = model.Title,
        Description = model.Description,
        SequenceNumber = model.SequenceNumber
    };

    public static DomainDay ToDomain(this CreateDayRequest request, Guid eventId, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EventId = eventId,
        MenuId = request.MenuId,
        Title = request.Title,
        Description = request.Description ?? string.Empty,
        SequenceNumber = request.SequenceNumber
    };

    public static void ApplyToDomain(this UpdateDayRequest request, DomainDay model)
    {
        if (request.MenuId.HasValue)
            model.MenuId = request.MenuId.Value;
        if (request.Title is not null)
            model.Title = request.Title;
        if (request.Description is not null)
            model.Description = request.Description;
        if (request.SequenceNumber.HasValue)
            model.SequenceNumber = request.SequenceNumber.Value;
    }
}