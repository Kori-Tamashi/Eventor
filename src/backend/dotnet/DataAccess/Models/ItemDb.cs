namespace DataAccess.Models;

/// <summary>
/// Модель таблицы предметов в базе данных
/// </summary>
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
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с позицией меню
    /// </summary>
    public virtual ICollection<MenuItemDb> MenuItems { get; set; } = new List<MenuItemDb>();
}