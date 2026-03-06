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
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Тип регистрации
    /// </summary>
    public RegistrationType Type { get; set; }

    /// <summary>
    /// Факт оплаты
    /// </summary>
    public bool Payment { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятием
    /// </summary>
    public virtual EventDb Event { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с пользователем
    /// </summary>
    public virtual UserDb User { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с отзывами
    /// </summary>
    public virtual ICollection<FeedbackDb> Feedbacks { get; set; } = new List<FeedbackDb>();

    /// <summary>
    /// Навигационное свойство для связи с участиями
    /// </summary>
    public virtual ICollection<ParticipationDb> Participations { get; set; } = new List<ParticipationDb>();
}