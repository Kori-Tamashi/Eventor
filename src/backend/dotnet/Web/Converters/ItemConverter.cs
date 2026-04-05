using DomainModel = Domain.Models.Item;
using Web.Dtos;

namespace Web.Converters;

public static class ItemConverter
{
    public static Item ToDto(this DomainModel model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Cost = (double)model.Cost
    };

    public static DomainModel ToDomain(this CreateItemRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = request.Title,
        Cost = (decimal)request.Cost
    };

    public static void ApplyToDomain(this UpdateItemRequest request, DomainModel model)
    {
        if (request.Title is not null)
            model.Title = request.Title;
        if (request.Cost.HasValue)
            model.Cost = (decimal)request.Cost.Value;
    }
}
