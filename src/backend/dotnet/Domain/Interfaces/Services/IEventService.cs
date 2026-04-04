using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IEventService
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetAsync(EventFilter? filter = null);
    Task<Event> CreateAsync(Event ev);
    Task UpdateAsync(Event ev);
    Task DeleteAsync(Guid id);

    Task<List<Event>> GetByParticipantUserIdAsync(Guid userId, PaginationFilter? filter = null);
    Task<List<Event>> GetByOrganizerUserIdAsync(Guid userId, PaginationFilter? filter = null);

    Task<List<Day>> GetDaysAsync(Guid eventId, PaginationFilter? filter = null);
    Task<Day> AddDayAsync(Guid eventId, Day day);
}