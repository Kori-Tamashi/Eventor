using NpgsqlTypes;

namespace Domain.Enums;

public enum RegistrationType
{
    [PgName("Cтандартный")]
    Standart,

    [PgName("VIP")]
    Vip,

    [PgName("Организатор")]
    Organizer,
}