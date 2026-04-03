using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IRegistrationRepository registrationRepository,
    IDayRepository dayRepository) : IEventService
{
    public Task<Event?> GetByIdAsync(Guid id) => eventRepository.GetByIdAsync(id);

    public Task<List<Event>> GetAsync(EventFilter? filter = null) => eventRepository.GetAsync(filter);

    public async Task<Event> CreateAsync(Event ev)
    {
        try
        {
            if (ev.Id == Guid.Empty)
                ev.Id = Guid.NewGuid();

            await eventRepository.CreateAsync(ev);
            return ev;
        }
        catch (Exception ex)
        {
            throw new EventCreateException("Failed to create event.", ex);
        }
    }

    public async Task UpdateAsync(Event ev)
    {
        try
        {
            var existing = await eventRepository.GetByIdAsync(ev.Id);
            if (existing is null)
                throw new EventNotFoundException($"Event '{ev.Id}' was not found.");

            await eventRepository.UpdateAsync(ev);
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new EventUpdateException("Failed to update event.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await eventRepository.GetByIdAsync(id);
            if (existing is null)
                throw new EventNotFoundException($"Event '{id}' was not found.");

            await eventRepository.DeleteAsync(id);
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new EventDeleteException("Failed to delete event.", ex);
        }
    }

    public async Task<List<Event>> GetByParticipantUserIdAsync(Guid userId, PaginationFilter? filter = null)
    {
        var registrations = await registrationRepository.GetRegistrationsAsync(new RegistrationFilter { UserId = userId });
        var eventIds = registrations.Select(r => r.EventId).Distinct().ToList();

        var events = new List<Event>(eventIds.Count);
        foreach (var eventId in eventIds)
        {
            var ev = await eventRepository.GetByIdAsync(eventId);
            if (ev is not null)
                events.Add(ev);
        }

        return ApplyPagination(events, filter);
    }

    public async Task<List<Event>> GetByOrganizerUserIdAsync(Guid userId, PaginationFilter? filter = null)
    {
        var registrations = await registrationRepository.GetRegistrationsAsync(new RegistrationFilter
        {
            UserId = userId,
            Type = RegistrationType.Organizer
        });

        var eventIds = registrations.Select(r => r.EventId).Distinct().ToList();
        var events = new List<Event>(eventIds.Count);
        foreach (var eventId in eventIds)
        {
            var ev = await eventRepository.GetByIdAsync(eventId);
            if (ev is not null)
                events.Add(ev);
        }

        return ApplyPagination(events, filter);
    }

    public async Task<List<Day>> GetDaysAsync(Guid eventId, PaginationFilter? filter = null)
    {
        var ev = await eventRepository.GetByIdAsync(eventId);
        if (ev is null)
            throw new EventNotFoundException($"Event '{eventId}' was not found.");

        var days = await dayRepository.GetAsync(new DayFilter { EventId = eventId });
        return ApplyPagination(days, filter);
    }

    public async Task<Day> AddDayAsync(Guid eventId, Day day)
    {
        var ev = await eventRepository.GetByIdAsync(eventId);
        if (ev is null)
            throw new EventNotFoundException($"Event '{eventId}' was not found.");

        day.EventId = eventId;
        if (day.Id == Guid.Empty)
            day.Id = Guid.NewGuid();

        await dayRepository.CreateAsync(day);
        return day;
    }

    private static List<T> ApplyPagination<T>(IReadOnlyCollection<T> source, PaginationFilter? filter)
    {
        if (filter?.PageNumber is null || filter.PageSize is null)
            return source.ToList();

        return source
            .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
            .Take(filter.PageSize.Value)
            .ToList();
    }
}