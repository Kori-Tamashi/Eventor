using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы регистраций в базе данных
/// </summary>
public class RegistrationDb
{
    public RegistrationDb(Guid id,
        Guid eventId,
        Guid userId,
        RegistrationType type,
        bool payment)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Type = type;
        Payment = payment;
    }

    /// <summary>
    /// Идентификатор регистрации
    /// </summary>
    [Key]
    [Column("registration_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    [Column("event_id", TypeName = "uuid")]
    public Guid EventId { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Тип регистрации
    /// </summary>
    [Column("type", TypeName = "registration_type")]
    public RegistrationType Type { get; set; }

    /// <summary>
    /// Факт оплаты
    /// </summary>
    [Column("payment", TypeName = "boolean")]
    public bool Payment { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятием
    /// </summary>
    public EventDb? Event { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с пользователем
    /// </summary>
    public UserDb? User { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с отзывами
    /// </summary>
    public ICollection<FeedbackDb>? Feedbacks { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с участиями
    /// </summary>
    public ICollection<ParticipationDb>? Participations { get; set; }
}