using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class MenuConverter
{
    public static Menu? ToDomain(MenuDb? db)
    {
        if (db == null) return null;

        var menu = new Menu(
            db.Id,
            db.Title,
            db.Description
        )
        {
            MenuItems = db.MenuItems?
                .Select(MenuItemConverter.ToDomain)
                .OfType<MenuItem>()
                .ToList() ?? []
        };

        return menu;
    }

    public static MenuDb? ToDb(Menu? domain)
    {
        if (domain == null) return null;

        return new MenuDb(
            domain.Id,
            domain.Title,
            domain.Description
        );
    }
}