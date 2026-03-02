using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы мероприятий в базе данных
/// </summary>
[Table("events")]
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
    [Key]
    [Column("event_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [Column("description", TypeName = "text")]
    [MaxLength(TextConstraints.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    [Column("start_date", TypeName = "date")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Идентификатор локации мероприятия
    /// </summary>
    [ForeignKey("Location")]
    [Column("location_id", TypeName = "uuid")]
    public Guid LocationId { get; set; }

    /// <summary>
    /// Количество дней
    /// </summary>
    [Column("days_count", TypeName = "integer")]
    public int DaysCount { get; set; }

    /// <summary>
    /// Наценка в процентах
    /// </summary>
    [Column("percent", TypeName = "numeric")]
    public double Percent { get; set; }

    /// <summary>
    /// Локация
    /// </summary>
    public LocationDb? Location { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public ICollection<RegistrationDb>? Registrations { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public ICollection<DayDb>? Days { get; set; }
}