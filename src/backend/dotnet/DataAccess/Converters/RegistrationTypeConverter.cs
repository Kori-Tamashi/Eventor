using Domain.Enums;
using DataAccess.Enums;

namespace DataAccess.Converters;

public static class RegistrationTypeConverter
{
    public static RegistrationTypeDb ToDb(RegistrationType type) => type switch
    {
        RegistrationType.Standard => RegistrationTypeDb.Standard,
        RegistrationType.Vip => RegistrationTypeDb.Vip,
        RegistrationType.Organizer => RegistrationTypeDb.Organizer,
        _ => throw new ArgumentOutOfRangeException(
            nameof(type),
            type,
            null)
    };

    public static RegistrationType ToDomain(RegistrationTypeDb type) => type switch
    {
        RegistrationTypeDb.Standard => RegistrationType.Standard,
        RegistrationTypeDb.Vip => RegistrationType.Vip,
        RegistrationTypeDb.Organizer => RegistrationType.Organizer,
        _ => throw new ArgumentOutOfRangeException(
            nameof(type),
            type,
            null)
    };
}