using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess.Models;

/// <summary>
/// Меню конкретного дня мероприятия
/// </summary>
[Table("menu")]
public class MenuDb
{
    public MenuDb(Guid id, string title, string description)
    {
        Id = id;
        Title = title;
        Description = description;
    }
    
    /// <summary>
    /// Идентификатор меню
    /// </summary>
    /// <example>f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6</example>
    [Key]
    [Column("menu_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    /// <example>Основное меню</example>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [Column("description", TypeName = "text")]
    public string Description { get; set; }

    /// <summary>
    /// Предметы меню
    /// </summary>
    public ICollection<MenuItemDb> MenuItems { get; set; }
}