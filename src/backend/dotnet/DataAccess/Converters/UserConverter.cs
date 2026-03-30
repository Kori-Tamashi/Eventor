using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class UserConverter
{
    public static User? ToDomain(UserDb? db)
    {
        if (db == null) return null;
        
        return new User(
            db.Id,
            db.Name,
            db.Phone,
            GenderConverter.ToDomain(db.Gender),
            UserRoleConverter.ToDomain(db.Role),
            db.PasswordHash
        );
    }

    public static UserDb? ToDb(User? user)
    {
        if (user == null) return null;
        
        return new UserDb(
            user.Id,
            user.Name,
            user.Phone,
            GenderConverter.ToDb(user.Gender),
            UserRoleConverter.ToDb(user.Role), 
            user.PasswordHash);
    }
}