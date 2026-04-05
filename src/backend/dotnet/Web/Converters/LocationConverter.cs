using DomainModel = Domain.Models.Location;
using Web.Dtos;

namespace Web.Converters;

public static class LocationConverter
{
    public static Location ToDto(this DomainModel model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
        Cost = (double)model.Cost,
        Capacity = model.Capacity
    };

    public static DomainModel ToDomain(this CreateLocationRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description ?? string.Empty,
        Cost = (decimal)request.Cost,
        Capacity = request.Capacity
    };

    public static void ApplyToDomain(this UpdateLocationRequest request, DomainModel model)
    {
        if (request.Title is not null)
            model.Title = request.Title;
        if (request.Description is not null)
            model.Description = request.Description;
        if (request.Cost.HasValue)
            model.Cost = (decimal)request.Cost.Value;
        if (request.Capacity.HasValue)
            model.Capacity = request.Capacity.Value;
    }
}
