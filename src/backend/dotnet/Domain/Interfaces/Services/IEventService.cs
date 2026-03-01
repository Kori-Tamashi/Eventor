using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IEventService
{
    Task<Event> CreateAsync(Event ev);
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetAllAsync();
    Task<IEnumerable<Event>> GetByLocationIdAsync(Guid locationId);
    Task<IEnumerable<Event>> GetByDateRangeAsync(DateOnly from, DateOnly to);
    Task UpdateAsync(Event ev);
    Task DeleteAsync(Guid id);
}