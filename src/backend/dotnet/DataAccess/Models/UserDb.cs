using DataAccess.Enums;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы пользователей в базе данных
/// </summary>
public class UserDb
{
    public UserDb(Guid id,
        string name,
        string phone,
        GenderDb gender,
        UserRoleDb role,
        string passwordHash)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Gender = gender;
        Role = role;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public GenderDb Gender { get; set; }

    /// <summary>
    /// Зашифрованный пароль
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    public UserRoleDb Role { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрациями
    /// </summary>
    public virtual ICollection<RegistrationDb> Registrations { get; set; } = new List<RegistrationDb>();
}