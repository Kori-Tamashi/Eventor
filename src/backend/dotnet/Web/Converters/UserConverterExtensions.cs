using DomainModel = Domain.Models.User;
using Web.Dtos;

namespace Web.Converters;

public static class UserConverterExtensions
{
    public static User ToDto(this DomainModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Phone = model.Phone,
        Gender = model.Gender.ToDto(),
        Role = model.Role.ToDto(),
        PasswordHash = model.PasswordHash
    };

    public static DomainModel ToDomain(this CreateUserRequest request, string passwordHash, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = request.Name,
        Phone = request.Phone,
        Gender = request.Gender.ToDomain(),
        Role = request.Role.ToDomain(),
        PasswordHash = passwordHash
    };

    public static DomainModel ToDomain(this RegisterRequest request, string passwordHash, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = request.Name,
        Phone = request.Phone,
        Gender = request.Gender.ToDomain(),
        Role = Domain.Enums.UserRole.User,
        PasswordHash = passwordHash
    };

    public static void ApplyToDomain(this UpdateUserRequest request, DomainModel user, string? passwordHash = null)
    {
        if (request.Name is not null)
            user.Name = request.Name;
        if (request.Phone is not null)
            user.Phone = request.Phone;
        if (request.Gender.HasValue)
            user.Gender = request.Gender.Value.ToDomain();
        if (request.Role.HasValue)
            user.Role = request.Role.Value.ToDomain();
        if (passwordHash is not null)
            user.PasswordHash = passwordHash;
    }
}
