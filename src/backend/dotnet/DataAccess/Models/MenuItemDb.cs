using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы позиций меню в базе данных
/// </summary>
[Table("menu_items")]
public class MenuItemDb
{
    public MenuItemDb(Guid menuId,
        Guid itemId,
        double amount)
    {
        MenuId = menuId;
        ItemId = itemId;
        Amount = amount;
    }

    /// <summary>
    /// Идентификатор меню
    /// </summary>
    [Column("menu_id", TypeName = "uuid")]
    public Guid MenuId { get; set; }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    [Column("item_id", TypeName = "uuid")]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Стоимость
    /// </summary>
    [Column("amount")]
    public double Amount { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с меню
    /// </summary>
    public MenuDb? Menu { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с предметом
    /// </summary>
    public ItemDb? Item { get; set; }
}