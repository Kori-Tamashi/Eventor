using Web.Dtos;

namespace Web.Converters;

public static class EnumConverter
{
    public static Gender ToDto(this Domain.Enums.Gender value) => (Gender)(int)value;
    public static Domain.Enums.Gender ToDomain(this Gender value) => (Domain.Enums.Gender)(int)value;

    public static UserRole ToDto(this Domain.Enums.UserRole value) => (UserRole)(int)value;
    public static Domain.Enums.UserRole ToDomain(this UserRole value) => (Domain.Enums.UserRole)(int)value;

    public static RegistrationType ToDto(this Domain.Enums.RegistrationType value) => (RegistrationType)(int)value;
    public static Domain.Enums.RegistrationType ToDomain(this RegistrationType value) => (Domain.Enums.RegistrationType)(int)value;
}