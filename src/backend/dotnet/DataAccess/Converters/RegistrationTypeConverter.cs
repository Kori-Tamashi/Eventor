using Domain.Enums;
using DataAccess.Enums;

namespace DataAccess.Converters;

public static class RegistrationTypeConverter
{
    public static RegistrationTypeDb ToDb(RegistrationType type)
    {
        return type switch
        {
            RegistrationType.Standart => RegistrationTypeDb.Standart,
            RegistrationType.Vip => RegistrationTypeDb.Vip,
            RegistrationType.Organizer => RegistrationTypeDb.Organizer,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), 
                type, 
                null)
        };
    }

    public static RegistrationType ToDomain(RegistrationTypeDb type)
    {
        return type switch
        {
            RegistrationTypeDb.Standart => RegistrationType.Standart,
            RegistrationTypeDb.Vip => RegistrationType.Vip,
            RegistrationTypeDb.Organizer => RegistrationType.Organizer,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), 
                type, 
                null)
        };
    }
}