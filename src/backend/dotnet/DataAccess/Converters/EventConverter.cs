using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class EventConverter
{
    public static Event? ToDomain(EventDb? db)
    {
        if (db == null) return null;

        return new Event(
            db.Id,
            db.LocationId, 
            db.Title,
            db.Description,
            db.StartDate,
            db.DaysCount,
            db.Percent
        );
    }

    public static EventDb? ToDb(Event? ev)
    {
        if (ev == null) return null;

        return new EventDb(
            ev.Id,
            ev.Title,
            ev.Description,
            ev.StartDate,
            ev.LocationId,
            ev.DaysCount,
            ev.Percent
        );
    }
}