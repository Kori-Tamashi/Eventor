using NpgsqlTypes;

namespace DataAccess.Enums;

[PgName("user_role")]
public enum UserRoleDb
{
    [PgName("Администратор")]
    Admin,

    [PgName("Зарегистрированный пользователь")]
    User,

    [PgName("Гость")]
    Guest
}