namespace Domain.Filters;

public class PaginationFilter
{
    /// <summary>
    /// Номер страницы
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int? PageSize { get; set; }
}