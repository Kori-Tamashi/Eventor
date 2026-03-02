using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы дней в базе данных
/// </summary>
[Table("days")]
public class DayDb
{
    public DayDb(
        Guid id,
        Guid eventId,
        Guid menuId,
        string title,
        int number,
        string description)
    {
        Id = id;
        EventId = eventId;
        MenuId = menuId;
        Title = title;
        Number = number;
        Description = description;
    }

    /// <summary>
    /// Идентификатор дня мероприятия
    /// </summary>
    [Key]
    [Column("day_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    [Column("event_id", TypeName = "uuid")]
    public Guid EventId { get; set; }

    /// <summary>
    /// Идентификатор меню
    /// </summary>
    [Column("menu_id", TypeName = "uuid")]
    public Guid MenuId { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Порядковый номер
    /// </summary>
    [Column("number", TypeName = "integer")]
    public int Number { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [Column("description", TypeName = "text")]
    [MaxLength(TextConstraints.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятием
    /// </summary>
    public EventDb? Event { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с меню
    /// </summary>
    public MenuDb? Menu { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с участием в мероприятии
    /// </summary>
    public ICollection<ParticipationDb>? Participations { get; set; }
}