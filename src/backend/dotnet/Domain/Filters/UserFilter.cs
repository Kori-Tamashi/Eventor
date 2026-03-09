using Domain.Enums;

namespace Domain.Filters;

/// <summary>
/// Фильтры для поиска пользователей
/// </summary>
public class UserFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по имени пользователя (частичное совпадение)
    /// </summary>
    public string? NameContains { get; set; }

    /// <summary>
    /// Фильтр по телефону
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Фильтр по роли
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Фильтр по полу
    /// </summary>
    public Gender? Gender { get; set; }
}