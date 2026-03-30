using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class DayConverter
{
    public static Day? ToDomain(DayDb? db)
    {
        if (db == null) return null;

        return new Day(
            db.Id,
            db.EventId,
            db.MenuId,
            db.Title,
            db.SequenceNumber,
            db.Description
        );
    }

    public static DayDb? ToDb(Day? day)
    {
        if (day == null) return null;

        return new DayDb(
            day.Id,
            day.EventId,
            day.MenuId,
            day.Title,
            day.SequenceNumber,
            day.Description
        );
    }
}