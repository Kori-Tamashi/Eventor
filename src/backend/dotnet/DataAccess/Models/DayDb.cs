using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы дней в базе данных
/// </summary>
public class DayDb
{
    public DayDb(Guid id,
        Guid eventId,
        Guid menuId,
        string title,
        int sequenceNumber,
        string description)
    {
        Id = id;
        EventId = eventId;
        MenuId = menuId;
        Title = title;
        SequenceNumber = sequenceNumber;
        Description = description;
    }

    /// <summary>
    /// Идентификатор дня мероприятия
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Идентификатор меню
    /// </summary>
    public Guid MenuId { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Порядковый номер
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятием
    /// </summary>
    public virtual EventDb Event { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с меню
    /// </summary>
    public virtual MenuDb Menu { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с участием в мероприятии
    /// </summary>
    public virtual ICollection<ParticipationDb> Participations { get; set; } = new List<ParticipationDb>();
}