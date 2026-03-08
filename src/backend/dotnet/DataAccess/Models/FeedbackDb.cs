namespace DataAccess.Models;

/// <summary>
/// Модель таблицы отзывов в базе данных
/// </summary>
public class FeedbackDb
{
    public FeedbackDb(Guid id,
        Guid registrationId,
        string comment,
        int rate)
    {
        Id = id;
        RegistrationId = registrationId;
        Comment = comment;
        Rate = rate;
    }

    /// <summary>
    /// Идентификатор отзыва
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор регистрации
    /// </summary>
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Рейтинг (1–5)
    /// </summary>
    public int Rate { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public virtual RegistrationDb Registration { get; set; } = null!;
}