using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class LocationConverter
{
    public static Location? ToDomain(LocationDb? db)
    {
        if (db == null) return null;

        return new Location(
            db.Id,
            db.Title,
            db.Description,
            db.Cost,
            db.Capacity
        );
    }

    public static LocationDb? ToDb(Location? location)
    {
        if (location == null) return null;

        return new LocationDb(
            location.Id,
            location.Title,
            location.Description,
            location.Cost,
            location.Capacity
        );
    }
}