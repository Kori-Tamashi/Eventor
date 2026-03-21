using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class MenuItemConverter
{
    public static MenuItem? ToDomain(MenuItemDb? db)
    {
        if (db == null) return null;

        return new MenuItem(
            db.ItemId,
            db.Amount
        );
    }

    public static MenuItemDb? ToDb(MenuItem? domain, Guid menuId)
    {
        if (domain == null) return null;

        return new MenuItemDb(
            menuId,
            domain.ItemId,
            domain.Amount
        );
    }
}