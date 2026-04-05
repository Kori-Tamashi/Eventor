using DomainMenu = Domain.Models.Menu;
using DomainMenuItem = Domain.Models.MenuItem;
using Web.Dtos;

namespace Web.Converters;

public static class MenuConverter
{
    public static Menu ToDto(this DomainMenu model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
        MenuItems = model.MenuItems.Select(ToDto).ToList()
    };

    public static MenuItem ToDto(this DomainMenuItem model) => new()
    {
        ItemId = model.ItemId,
        Amount = model.Amount
    };

    public static DomainMenu ToDomain(this CreateMenuRequest request, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description ?? string.Empty
    };

    public static void ApplyToDomain(this UpdateMenuRequest request, DomainMenu model)
    {
        if (request.Title is not null)
            model.Title = request.Title;
        if (request.Description is not null)
            model.Description = request.Description;
    }

    public static DomainMenuItem ToDomain(this MenuItem item) => new(item.ItemId, item.Amount);
}
