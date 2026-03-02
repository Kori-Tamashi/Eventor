using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы участий в базе данных
/// </summary>
[Table("participation")]
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
    [Column("day_id", TypeName = "uuid")]
    public Guid DayId { get; set; }

    /// <summary>
    /// Идентификатор регистрации
    /// </summary>
    [Column("registration_id", TypeName = "uuid")]
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с днем мероприятия
    /// </summary>
    public DayDb? Day { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public RegistrationDb? Registration { get; set; }
}