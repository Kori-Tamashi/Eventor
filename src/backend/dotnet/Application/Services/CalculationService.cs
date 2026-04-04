using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Services;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class CalculationService(
    IItemService itemService,
    IMenuService menuService,
    IDayService dayService,
    IEventService eventService,
    ICalculationSupportService calculationSupportService) : ICalculationService
{
    public async Task<decimal> GetItemCostAsync(Guid itemId)
    {
        var item = await itemService.GetByIdAsync(itemId);
        if (item is null)
            throw new EconomyServiceException($"Item '{itemId}' was not found.");

        return item.Cost;
    }

    public async Task<decimal> GetMenuCostAsync(Guid menuId)
    {
        var menu = await menuService.GetByIdAsync(menuId, includeItems: true);
        if (menu is null)
            throw new EconomyServiceException($"Menu '{menuId}' was not found.");

        decimal total = 0;
        foreach (var menuItem in menu.MenuItems)
        {
            var item = await itemService.GetByIdAsync(menuItem.ItemId);
            if (item is null)
                continue;

            total += item.Cost * menuItem.Amount;
        }

        return total;
    }

    public async Task<decimal> GetDaysCostAsync(IReadOnlyCollection<Guid> dayIds)
    {
        if (dayIds.Count == 0)
            return 0;

        decimal total = 0;
        foreach (var dayId in dayIds.Distinct())
            total += await calculationSupportService.GetSingleDayCostAsync(dayId);

        return total;
    }

    public async Task<decimal> GetEventCostAsync(Guid eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
            throw new EconomyServiceException($"Event '{eventId}' was not found.");

        var days = await dayService.GetAsync(new Domain.Filters.DayFilter { EventId = eventId });
        return await GetDaysCostAsync(days.Select(d => d.Id).ToList());
    }

    public async Task<decimal> GetDayPriceAsync(Guid dayId)
    {
        var day = await dayService.GetByIdAsync(dayId);
        if (day is null)
            throw new EconomyServiceException($"Day '{dayId}' was not found.");

        var ev = await eventService.GetByIdAsync(day.EventId);
        if (ev is null)
            throw new EconomyServiceException($"Event '{day.EventId}' was not found.");

        var fundamental = await CalculateFundamentalPriceNDAsync(ev.Id);
        var coefficient = await calculationSupportService.GetDayCoefficientAsync([dayId]);

        return (1 + (decimal)(ev.Percent / 100.0)) * fundamental * coefficient;
    }

    public async Task<decimal> GetDayPriceWithPrivilegesAsync(Guid dayId)
    {
        var day = await dayService.GetByIdAsync(dayId);
        if (day is null)
            throw new EconomyServiceException($"Day '{dayId}' was not found.");

        var ev = await eventService.GetByIdAsync(day.EventId);
        if (ev is null)
            throw new EconomyServiceException($"Event '{day.EventId}' was not found.");

        var fundamental = await CalculateFundamentalPriceWithPrivilegesNDAsync(ev.Id);
        var coefficient = await calculationSupportService.GetDayCoefficientAsync([dayId]);

        return (1 + (decimal)(ev.Percent / 100.0)) * fundamental * coefficient;
    }

    public async Task<decimal> GetDaysPriceAsync(IReadOnlyCollection<Guid> dayIds)
    {
        if (dayIds.Count == 0)
            return 0;

        await calculationSupportService.EnsureDaysFromSameEventAsync(dayIds);
        var firstDay = await dayService.GetByIdAsync(dayIds.First());
        if (firstDay is null)
            throw new EconomyServiceException("Invalid day IDs provided.");

        var ev = await eventService.GetByIdAsync(firstDay.EventId);
        if (ev is null)
            throw new EconomyServiceException($"Event '{firstDay.EventId}' was not found.");

        var fundamental = await CalculateFundamentalPriceNDAsync(ev.Id);
        var coefficient = await calculationSupportService.GetDayCoefficientAsync(dayIds);

        return (1 + (decimal)(ev.Percent / 100.0)) * fundamental * coefficient;
    }

    public async Task<decimal> GetFundamentalPriceForSingleDayAsync(Guid eventId)
    {
        var totalCost = await GetEventCostAsync(eventId);
        if (totalCost < 0)
            throw new EconomyServiceException("Event cost cannot be negative.");

        var days = await dayService.GetAsync(new DayFilter { EventId = eventId });
        if (days.Count == 0)
            throw new EconomyServiceException("Event must contain at least one day.");

        decimal sum = 0;
        foreach (var day in days)
        {
            var coefficient = await calculationSupportService.GetDayCoefficientAsync([day.Id]);
            var participants = await calculationSupportService.GetParticipantsByDayAsync(day.Id, includePrivileged: true);
            sum += coefficient * participants;
        }

        if (sum <= 0)
            throw new EconomyServiceException("Fundamental price denominator must be positive.");

        return totalCost / sum;
    }

    public async Task<decimal> GetFundamentalPriceForMultiDayAsync(Guid eventId)
    {
        return await CalculateFundamentalPriceNDAsync(eventId);
    }

    public async Task<bool> IsSingleDayCaseBalancedAsync(Guid eventId)
    {
        var days = await dayService.GetAsync(new DayFilter { EventId = eventId });
        if (days.Count == 0)
            return false;

        var totalCost = await GetEventCostAsync(eventId);
        decimal income = 0;

        foreach (var day in days)
        {
            var price = await GetDayPriceAsync(day.Id);
            var participants = await calculationSupportService.GetParticipantsByDayAsync(day.Id, includePrivileged: true);
            income += price * participants;
        }

        return income >= totalCost;
    }

    public async Task<bool> IsMultiDayCaseBalancedAsync(Guid eventId)
    {
        var combinations = await calculationSupportService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: true);
        if (combinations.Count == 0)
            return false;

        var totalCost = await GetEventCostAsync(eventId);
        decimal income = 0;

        foreach (var combination in combinations)
        {
            var price = await GetDaysPriceAsync(combination);
            var participants = await calculationSupportService.GetParticipantsCountExactAsync(combination, includePrivileged: true);
            income += price * participants;
        }

        return income >= totalCost;
    }

    private async Task<decimal> CalculateFundamentalPriceNDAsync(Guid eventId)
    {
        var totalCost = await GetEventCostAsync(eventId);
        if (totalCost < 0)
            throw new EconomyServiceException("Event cost cannot be negative.");

        var combinations = await calculationSupportService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: true);
        if (combinations.Count == 0)
            throw new EconomyServiceException("No day combinations found for event.");

        decimal sum = 0;
        foreach (var combination in combinations)
        {
            var coefficient = await calculationSupportService.GetDayCoefficientAsync(combination);
            if (coefficient <= 0)
                continue;

            var participants = await calculationSupportService.GetParticipantsCountExactAsync(combination, includePrivileged: true);
            if (participants <= 0)
                continue;

            sum += coefficient * participants;
        }

        if (sum <= 0)
            throw new EconomyServiceException("Fundamental price denominator must be positive.");

        return totalCost / sum;
    }

    private async Task<decimal> CalculateFundamentalPriceWithPrivilegesNDAsync(Guid eventId)
    {
        var totalCost = await GetEventCostAsync(eventId);
        if (totalCost < 0)
            throw new EconomyServiceException("Event cost cannot be negative.");

        var combinations = await calculationSupportService.GetCurrentDayCombinationsAsync(eventId, includePrivileged: false);
        if (combinations.Count == 0)
            throw new EconomyServiceException("No day combinations found for event.");

        decimal sum = 0;
        foreach (var combination in combinations)
        {
            var coefficient = await calculationSupportService.GetDayCoefficientAsync(combination);
            var participants = await calculationSupportService.GetParticipantsCountExactAsync(combination, includePrivileged: false);
            sum += coefficient * participants;
        }

        if (sum <= 0)
            throw new EconomyServiceException("Fundamental price denominator must be positive.");

        return totalCost / sum;
    }
}