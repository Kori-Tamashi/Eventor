using DomainDay = Domain.Models.Day;
using DomainEvent = Domain.Models.Event;
using Web.Dtos;

namespace Web.Converters;

public static class EventAndDayConverterExtensions
{
    public static Event ToDto(this DomainEvent model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
        StartDate = model.StartDate,
        LocationId = model.LocationId,
        DaysCount = model.DaysCount,
        Percent = model.Percent,
        CreatedByUserId = Guid.Empty,
        Rating = 0,
        PersonCount = 0
    };

    public static DomainEvent ToDomain(this CreateEventRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description ?? string.Empty,
        StartDate = request.StartDate,
        LocationId = request.LocationId,
        DaysCount = request.DaysCount,
        Percent = request.Percent
    };

    public static void ApplyToDomain(this UpdateEventRequest request, DomainEvent model)
    {
        if (request.Title is not null)
            model.Title = request.Title;
        if (request.Description is not null)
            model.Description = request.Description;
        if (request.StartDate.HasValue)
            model.StartDate = request.StartDate.Value;
        if (request.LocationId.HasValue)
            model.LocationId = request.LocationId.Value;
        if (request.DaysCount.HasValue)
            model.DaysCount = request.DaysCount.Value;
        if (request.Percent.HasValue)
            model.Percent = request.Percent.Value;
    }

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
