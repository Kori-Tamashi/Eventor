namespace Eventor.Domain.Interfaces.Services;

public interface ICalculationServer
{
    /// Цена одного дня
    Task<double> GetDayPriceAsync(Guid dayId);

    /// Цена одного дня с учётом льгот
    Task<double> GetDayPriceWithPrivilegesAsync(Guid dayId);

    /// Цена выбранного набора дней
    Task<double> GetDaysPriceAsync(IEnumerable<Guid> daysId);

    /// Цена набора дней с учётом льгот
    Task<double> GetDaysPriceWithPrivilegesAsync(IEnumerable<Guid> daysId);

    /// Цена всего мероприятия
    Task<double> GetEventPriceAsync(Guid eventId);

    /// Цена мероприятия с учётом льгот
    Task<double> GetEventPriceWithPrivilegesAsync(Guid eventId);
    
    /// Коэффициент для дня или набора дней
    Task<double> GetDayCoefficientAsync(IEnumerable<Guid> daysId);

    /// Базовая цена для однодневного случая
    Task<double> CalculateFundamentalPrice1DAsync(Guid eventId);

    /// Базовая цена для многодневного случая
    Task<double> CalculateFundamentalPriceNDAsync(Guid eventId);

    /// Базовая цена для однодневного случая с учётом льгот
    Task<double> CalculateFundamentalPriceWithPrivileges1DAsync(Guid eventId);

    /// Базовая цена для многодневного случая с учётом льгот
    Task<double> CalculateFundamentalPriceWithPrivilegesNDAsync(Guid eventId);
    
    /// Проверка баланса для однодневного случая
    Task<bool> CheckBalance1DAsync(Guid eventId);

    /// Проверка баланса для многодневного случая
    Task<bool> CheckBalanceNDAsync(Guid eventId);

    /// Текущий доход мероприятия по выбранным комбинациям дней
    Task<double> CalculateCurrentIncomeAsync(Guid eventId);
    
    /// Проверка, что для мероприятия можно посчитать корректную цену
    Task<bool> CheckSolutionExistenceAsync(Guid eventId);
    Task<bool> CheckSolutionExistenceWithPrivilegesAsync(Guid eventId);

    /// Минимум участников для покрытия расходов
    Task<int> CalculateCriticalParticipantsCountAsync(Guid eventId, double maxPrice);

    /// Максимально допустимая наценка
    Task<double> CalculateMaxMarkupAsync(Guid eventId, double maxPrice);

    /// Диапазон базовой цены
    Task<(double Min, double Max)> CalculateFundamentalPriceIntervalAsync(Guid eventId);
}
