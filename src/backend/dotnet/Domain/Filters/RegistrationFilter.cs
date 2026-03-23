using Domain.Enums;

namespace Domain.Filters;

public class RegistrationFilter : PaginationFilter
{
    /// <summary>
    /// Фильтр по идентификатору события
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Фильтр по идентификатору пользователя
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Фильтр по типу регистрации
    /// </summary>
    public RegistrationType? Type { get; set; }

    /// <summary>
    /// Фильтр по факту оплаты
    /// </summary>
    public bool? Payment { get; set; }
}