using NpgsqlTypes;

namespace Domain.Enums;

public enum UserRole
{
    [PgName("Администратор")]
    Admin,

    [PgName("Зарегистрированный пользователь")]
    User,

    [PgName("Гость")]
    Guest
}