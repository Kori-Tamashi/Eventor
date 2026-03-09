namespace Domain.Filters;

public class ItemFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по названию (частичное совпадение)
    /// </summary>
    public string? TitleContains { get; set; }
}