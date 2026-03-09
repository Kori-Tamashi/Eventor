using Domain.Enums;

namespace Domain.Filters;

public class FeedbackFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по регистрации
    /// </summary>
    public Guid? RegistrationId { get; set; }

    /// <summary>
    /// Сортировка по рейтингу
    /// </summary>
    public FeedbackSortByRate SortByRate { get; set; } = FeedbackSortByRate.None;
}