namespace DataAccess.Models;

/// <summary>
/// Модель таблицы локаций в базе данных
/// </summary>
public class LocationDb
{
    public LocationDb(Guid id,
        string title,
        string description,
        decimal cost,
        int capacity)
    {
        Id = id;
        Title = title;
        Description = description;
        Cost = cost;
        Capacity = capacity;
    }

    /// <summary>
    /// Идентификатор локации
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
    /// Цена аренды на 1 день
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Вместимость
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятиями
    /// </summary>
    public virtual ICollection<EventDb> Events { get; set; } = new List<EventDb>();
}