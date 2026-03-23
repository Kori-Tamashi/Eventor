namespace DataAccess.Models;

/// <summary>
/// Модель таблицы мероприятий в базе данных
/// </summary>
public class EventDb
{
    public EventDb(Guid id,
        string title,
        string description,
        DateOnly startDate,
        Guid locationId,
        int daysCount,
        double percent)
    {
        Id = id;
        Title = title;
        Description = description;
        StartDate = startDate;
        LocationId = locationId;
        DaysCount = daysCount;
        Percent = percent;
    }

    /// <summary>
    /// Идентификатор мероприятия
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
    /// Дата начала
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Идентификатор локации мероприятия
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Количество дней
    /// </summary>
    public int DaysCount { get; set; }

    /// <summary>
    /// Наценка в процентах
    /// </summary>
    public double Percent { get; set; }

    /// <summary>
    /// Локация
    /// </summary>
    public virtual LocationDb Location { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public virtual ICollection<RegistrationDb> Registrations { get; set; } = new List<RegistrationDb>();

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public virtual ICollection<DayDb> Days { get; set; } = new List<DayDb>();
}