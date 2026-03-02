using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess.Models;

/// <summary>
/// Предмет
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
    /// <example>f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6</example>
    [Key]
    [Column("item_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    /// <example>Бутылка воды (3л.)</example>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }
    
    /// <summary>
    /// Цена поездки
    /// </summary>
    /// <example>1000</example>
    [Column("cost", TypeName = "numeric")]
    public decimal Cost { get; set; }
    
    /// <summary>
    /// Использование предмета в меню
    /// </summary>
    public ICollection<MenuItemDb> MenuItems { get; set; }
}
