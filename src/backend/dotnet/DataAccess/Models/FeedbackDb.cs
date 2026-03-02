using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы отзывов в базе данных
/// </summary>
[Table("feedbacks")]
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
    [Key]
    [Column("feedback_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор регистрации
    /// </summary>
    [Column("registration_id", TypeName = "uuid")]
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    [Column("comment", TypeName = "text")]
    [MaxLength(TextConstraints.MaxCommentLength)]
    public string Comment { get; set; }

    /// <summary>
    /// Рейтинг (1–5)
    /// </summary>
    [Column("rate", TypeName = "int")]
    public int Rate { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрацией
    /// </summary>
    public RegistrationDb? Registration { get; set; }
}