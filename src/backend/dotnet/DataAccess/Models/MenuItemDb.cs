namespace DataAccess.Models;

/// <summary>
/// Модель таблицы позиций меню в базе данных
/// </summary>
public class MenuItemDb
{
    public MenuItemDb(Guid menuId,
        Guid itemId,
        int amount)
    {
        MenuId = menuId;
        ItemId = itemId;
        Amount = amount;
    }

    /// <summary>
    /// Идентификатор меню
    /// </summary>
    public Guid MenuId { get; set; }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Количество
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с меню
    /// </summary>
    public virtual MenuDb Menu { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с предметом
    /// </summary>
    public virtual ItemDb Item { get; set; } = null!;
}