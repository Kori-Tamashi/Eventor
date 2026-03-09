namespace Domain.Filters;

public class MenuFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по названию меню (частичное совпадение)
    /// </summary>
    public string? TitleContains { get; set; }
}