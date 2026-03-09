namespace Domain.Filters;

public class LocationFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по названию (частичное совпадение)
    /// </summary>
    public string? TitleContains { get; set; }

    /// <summary>
    /// Фильтр по минимальной стоимости
    /// </summary>
    public decimal? CostFrom { get; set; }

    /// <summary>
    /// Фильтр по максимальной стоимости
    /// </summary>
    public decimal? CostTo { get; set; }

    /// <summary>
    /// Фильтр по минимальной вместимости
    /// </summary>
    public int? CapacityFrom { get; set; }

    /// <summary>
    /// Фильтр по максимальной вместимости
    /// </summary>
    public int? CapacityTo { get; set; }
}