namespace DataAccess.Models;

/// <summary>
/// Модель таблицы участий в базе данных
/// </summary>
public class ParticipationDb
{
    public ParticipationDb(Guid dayId,
        Guid registrationId)
    {
        DayId = dayId;
        RegistrationId = registrationId;
    }

    /// <summary>
    /// Идентификатор дня мероприятия
    /// </summary>
    public Guid DayId { get; set; }

    /// <summary>
    /// Идентификатор регистрации
    /// </summary>
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public virtual DayDb Day { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public virtual RegistrationDb Registration { get; set; } = null!;
}