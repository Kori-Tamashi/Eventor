namespace Domain.Filters;

public class EventFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по локации
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Фильтр: дата начала >= StartDateFrom
    /// </summary>
    public DateOnly? StartDateFrom { get; set; }

    /// <summary>
    /// Фильтр: дата начала <= StartDateTo
    /// </summary>
    public DateOnly? StartDateTo { get; set; }

    /// <summary>
    /// Фильтр по названию (частичное совпадение)
    /// </summary>
    public string? TitleContains { get; set; }
}