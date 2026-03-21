using Domain.Enums;
using DataAccess.Enums;

namespace DataAccess.Converters;

public static class UserRoleConverter
{
    public static UserRoleDb ToDb(UserRole role) => role switch
    {
        UserRole.Admin => UserRoleDb.Admin,
        UserRole.User => UserRoleDb.User,
        UserRole.Guest => UserRoleDb.Guest,
        _ => throw new ArgumentOutOfRangeException(
            nameof(role),
            role,
            null)
    };

    public static UserRole ToDomain(UserRoleDb role) => role switch
    {
        UserRoleDb.Admin => UserRole.Admin,
        UserRoleDb.User => UserRole.User,
        UserRoleDb.Guest => UserRole.Guest,
        _ => throw new ArgumentOutOfRangeException(
            nameof(role),
            role,
            null)
    };
}