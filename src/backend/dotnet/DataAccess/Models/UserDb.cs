using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы пользователей в базе данных
/// </summary>
[Table("users")]
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
    [Key]
    [Column("user_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    [Column("name", TypeName = "varchar(255)")]
    public string Name { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    [Column("phone", TypeName = "varchar(255)")]
    public string Phone { get; set; }

    /// <summary>
    /// Гендер
    /// </summary>
    [Column("gender", TypeName = "gender")]
    public Gender Gender { get; set; }

    /// <summary>
    /// Зашифрованный пароль
    /// </summary>
    [Column("password_hash", TypeName = "varchar(255)")]
    public string PasswordHash { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    [Column("role", TypeName = "user_role")]
    public UserRole Role { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрациями
    /// </summary>
    public ICollection<RegistrationDb>? Registrations { get; set; }
}