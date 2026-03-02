using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы меню конкретного дня мероприятия в базе данных
/// </summary>
[Table("menu")]
public class MenuDb
{
    public MenuDb(Guid id,
        string title,
        string description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    /// <summary>
    /// Идентификатор меню
    /// </summary>
    [Key]
    [Column("menu_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [Column("description", TypeName = "text")]
    [MaxLength(TextConstraints.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public DayDb? Day { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с позициями меню
    /// </summary>
    public ICollection<MenuItemDb>? MenuItems { get; set; }
}