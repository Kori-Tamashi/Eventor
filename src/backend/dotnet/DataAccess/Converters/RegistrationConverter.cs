using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class RegistrationConverter
{
    public static Registration? ToDomain(RegistrationDb? db)
    {
        if (db == null) return null;

        var registration = new Registration(
            db.Id,
            db.EventId,
            db.UserId,
            RegistrationTypeConverter.ToDomain(db.Type),
            db.Payment
        );
        
        registration.Days = db.Participations?
            .Select(p => DayConverter.ToDomain(p.Day))
            .OfType<Day>()
            .ToList() ?? [];

        return registration;
    }

    public static RegistrationDb? ToDb(Registration? registration)
    {
        if (registration == null) return null;

        return new RegistrationDb(
            registration.Id,
            registration.EventId,
            registration.UserId,
            RegistrationTypeConverter.ToDb(registration.Type),
            registration.Payment
        );
    }
}