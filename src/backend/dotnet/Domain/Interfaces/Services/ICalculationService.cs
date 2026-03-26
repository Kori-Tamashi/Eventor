namespace Domain.Interfaces.Services;

public interface ICalculationService
{
    Task<decimal> GetItemCostAsync(Guid itemId);
    Task<decimal> GetMenuCostAsync(Guid menuId);
    Task<decimal> GetDaysCostAsync(IReadOnlyCollection<Guid> dayIds);
    Task<decimal> GetEventCostAsync(Guid eventId);

    Task<decimal> GetDayPriceAsync(Guid dayId);
    Task<decimal> GetDayPriceWithPrivilegesAsync(Guid dayId);
    Task<decimal> GetDaysPriceAsync(IReadOnlyCollection<Guid> dayIds);

    Task<decimal> GetFundamentalPriceForSingleDayAsync(Guid eventId);
    Task<decimal> GetFundamentalPriceForMultiDayAsync(Guid eventId);

    Task<bool> IsSingleDayCaseBalancedAsync(Guid eventId);
    Task<bool> IsMultiDayCaseBalancedAsync(Guid eventId);
}