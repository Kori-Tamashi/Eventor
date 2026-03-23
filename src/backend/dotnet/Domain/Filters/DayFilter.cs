namespace Domain.Filters;

public class DayFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по событию
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Фильтр по меню
    /// </summary>
    public Guid? MenuId { get; set; }
}