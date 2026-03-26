using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Services;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class CalculationSupportService(
    IItemService itemService,
    IMenuService menuService,
    IDayService dayService,
    IEventService eventService,
    ILocationService locationService,
    IRegistrationService registrationService) : ICalculationSupportService
{
    public async Task<decimal> GetSingleDayCostAsync(Guid dayId)
    {
        var day = await dayService.GetByIdAsync(dayId);
        if (day is null)
            throw new EconomyServiceException($"Day '{dayId}' was not found.");

        var menu = await menuService.GetByIdAsync(day.MenuId, includeItems: true);
        if (menu is null)
            throw new EconomyServiceException($"Menu '{day.MenuId}' was not found.");

        decimal menuCost = 0;
        foreach (var menuItem in menu.MenuItems)
        {
            var item = await itemService.GetByIdAsync(menuItem.ItemId);
            if (item is null)
                continue;

            menuCost += item.Cost * menuItem.Amount;
        }

        var ev = await eventService.GetByIdAsync(day.EventId);
        if (ev is null)
            throw new EconomyServiceException($"Event '{day.EventId}' was not found.");

        var location = await locationService.GetByIdAsync(ev.LocationId);
        var locationCost = location?.Cost ?? 0m;

        return menuCost + locationCost;
    }

    public async Task<decimal> GetDayCoefficientAsync(IEnumerable<Guid> dayIds)
    {
        var requested = dayIds.Distinct().ToList();
        if (requested.Count == 0)
            return 0;

        var firstDay = await dayService.GetByIdAsync(requested[0]);
        if (firstDay is null)
            throw new EconomyServiceException("Invalid day IDs provided.");

        var allEventDays = await dayService.GetAsync(new DayFilter { EventId = firstDay.EventId });
        if (allEventDays.Count == 0)
            throw new EconomyServiceException("Event has no days.");

        var allEventDayIds = allEventDays.Select(d => d.Id).ToHashSet();
        if (requested.Any(id => !allEventDayIds.Contains(id)))
            throw new EconomyServiceException("Some days do not belong to the event.");

        var costs = new Dictionary<Guid, decimal>(allEventDays.Count);
        foreach (var day in allEventDays)
        {
            var cost = await GetSingleDayCostAsync(day.Id);
            if (cost <= 0)
                throw new EconomyServiceException($"Day '{day.Id}' has invalid cost value.");

            costs[day.Id] = cost;
        }

        var minCost = costs.Values.Min();
        var selectedCost = requested.Sum(id => costs[id]);

        return selectedCost / minCost;
    }

    public async Task EnsureDaysFromSameEventAsync(IEnumerable<Guid> dayIds)
    {
        Guid? eventId = null;

        foreach (var dayId in dayIds.Distinct())
        {
            var day = await dayService.GetByIdAsync(dayId);
            if (day is null)
                throw new EconomyServiceException($"Day '{dayId}' was not found.");

            if (eventId is null)
            {
                eventId = day.EventId;
                continue;
            }

            if (eventId != day.EventId)
                throw new EconomyServiceException("All days must belong to the same event.");
        }
    }

    public async Task<int> GetParticipantsByDayAsync(Guid dayId, bool includePrivileged)
    {
        var registrations = await registrationService.GetAsync(new RegistrationFilter(), includeDays: true);
        return registrations
            .Where(r => r.Payment)
            .Where(r => includePrivileged || (r.Type != RegistrationType.Organizer && r.Type != RegistrationType.Vip))
            .Count(r => r.Days.Any(d => d.Id == dayId));
    }

    public async Task<int> GetParticipantsCountExactAsync(IReadOnlyCollection<Guid> dayIds, bool includePrivileged)
    {
        if (dayIds.Count == 0)
            return 0;

        await EnsureDaysFromSameEventAsync(dayIds);
        var dayIdSet = dayIds.ToHashSet();

        var firstDay = await dayService.GetByIdAsync(dayIds.First());
        if (firstDay is null)
            return 0;

        var eventRegistrations = await registrationService.GetAsync(
            new RegistrationFilter { EventId = firstDay.EventId }, includeDays: true);

        return eventRegistrations
            .Where(r => r.Payment)
            .Where(r => includePrivileged || (r.Type != RegistrationType.Organizer && r.Type != RegistrationType.Vip))
            .Count(r => r.Days.Select(d => d.Id).ToHashSet().SetEquals(dayIdSet));
    }

    public async Task<List<IReadOnlyCollection<Guid>>> GetCurrentDayCombinationsAsync(Guid eventId, bool includePrivileged)
    {
        var registrations = await registrationService.GetAsync(
            new RegistrationFilter { EventId = eventId }, includeDays: true);

        var query = registrations
            .Where(r => r.Payment)
            .Where(r => includePrivileged || (r.Type != RegistrationType.Organizer && r.Type != RegistrationType.Vip))
            .Select(r => r.Days.Select(d => d.Id).Distinct().OrderBy(id => id).ToList())
            .Where(days => days.Count > 0)
            .Select(days => string.Join('|', days));

        var uniqueKeys = query.Distinct().ToList();
        var result = new List<IReadOnlyCollection<Guid>>(uniqueKeys.Count);

        foreach (var key in uniqueKeys)
        {
            var ids = key.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .ToList();
            result.Add(ids);
        }

        return result;
    }
}
