using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы предметов в базе данных
/// </summary>
[Table("items")]
public class ItemDb
{
    public ItemDb(Guid id, string title, decimal cost)
    {
        Id = id;
        Title = title;
        Cost = cost;
    }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    [Key]
    [Column("item_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Цена поездки
    /// </summary>
    [Column("cost", TypeName = "numeric")]
    public decimal Cost { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с позицией меню
    /// </summary>
    public ICollection<MenuItemDb>? MenuItems { get; set; }
}