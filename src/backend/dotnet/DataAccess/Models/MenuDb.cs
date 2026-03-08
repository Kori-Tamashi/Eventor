namespace DataAccess.Models;

/// <summary>
/// Модель таблицы меню конкретного дня мероприятия в базе данных
/// </summary>
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
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public virtual DayDb Day { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с позициями меню
    /// </summary>
    public virtual ICollection<MenuItemDb> MenuItems { get; set; } = new List<MenuItemDb>();
}