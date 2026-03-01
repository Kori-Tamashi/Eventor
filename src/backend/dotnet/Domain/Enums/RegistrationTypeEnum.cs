using NpgsqlTypes;

namespace Domain.Enums;

public enum RegistrationTypeEnum
{
    [PgName("Cтандартный")]
    Standart,

    [PgName("VIP")]
    Vip,

    [PgName("Организатор")]
    Organizer,
}