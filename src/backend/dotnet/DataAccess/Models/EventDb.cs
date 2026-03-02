using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess.Models;

/// <summary>
/// Модель таблицы участников в базе данных
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
    /// <example>f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6</example>
    [Key]
    [Column("event_id", TypeName = "uuid")]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    /// <example>Фестиваль</example>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }
    
    /// <summary>
    /// Описание
    /// </summary>
    /// <example>Фестиваль урожая 2025 года</example>
    [Column("description", TypeName = "text")]
    public string Description { get; set; }
    
    /// <summary>
    /// Дата начала
    /// </summary>
    [Column("start_date", TypeName = "date")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Идентификатор локации мероприятия
    /// </summary>
    /// <example>f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6</example>
    [ForeignKey("Location")]
    [Column("location_id", TypeName = "uuid")]
    public Guid LocationId { get; set; }
    
    /// <summary>
    /// Количество дней
    /// </summary>
    /// <example>3</example>
    [Column("days_count", TypeName = "integer")]
    public int DaysCount { get; set; }

    /// <summary>
    /// Наценка в процентах
    /// </summary>
    /// <example>10</example>
    [Column("percent", TypeName = "numeric")]
    public double Percent { get; set; }
    
    /// <summary>
    /// Локация
    /// </summary>
    public LocationDb Location { get; set; }
}