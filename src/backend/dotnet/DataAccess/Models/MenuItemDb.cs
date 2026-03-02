using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess.Models;

/// <summary>
/// Модель таблицы меню и предметов в базе данных
/// </summary>
[Table("menu_items")]
public class MenuItemDb
{
    public MenuItemDb(Guid menuId, Guid itemId, double amount)
    {
        MenuId = menuId;
        ItemId = itemId;
        Amount = amount;
    }

    [Column("menu_id", TypeName = "uuid")]
    public Guid MenuId { get; set; }

    [Column("item_id", TypeName = "uuid")]
    public Guid ItemId { get; set; }

    [Column("amount")]
    public double Amount { get; set; }
    
    public MenuDb Menu { get; set; }
    
    public ItemDb Item { get; set; }
}