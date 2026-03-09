using NpgsqlTypes;

namespace DataAccess.Enums;

[PgName("registration_type")]
public enum RegistrationTypeDb
{
    [PgName("Cтандартный")]
    Standart,

    [PgName("VIP")]
    Vip,

    [PgName("Организатор")]
    Organizer,
}