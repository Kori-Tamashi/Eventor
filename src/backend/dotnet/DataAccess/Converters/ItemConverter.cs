using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class ItemConverter
{
    public static Item? ToDomain(ItemDb? db)
    {
        if (db == null) return null;

        return new Item(
            db.Id,
            db.Title,
            db.Cost
        );
    }

    public static ItemDb? ToDb(Item? item)
    {
        if (item == null) return null;

        return new ItemDb(
            item.Id,
            item.Title,
            item.Cost
        );
    }
}