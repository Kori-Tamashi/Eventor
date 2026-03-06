using Domain.Enums;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы пользователей в базе данных
/// </summary>
public class UserDb
{
    public UserDb(Guid id,
        string name,
        string phone,
        Gender gender,
        string passwordHash,
        UserRole role)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Gender = gender;
        PasswordHash = passwordHash;
        Role = role;
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
    public Gender Gender { get; set; }

    /// <summary>
    /// Зашифрованный пароль
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрациями
    /// </summary>
    public virtual ICollection<RegistrationDb> Registrations { get; set; } = new List<RegistrationDb>();
}