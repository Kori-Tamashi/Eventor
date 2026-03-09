using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetAsync(EventFilter? filter = null);
    Task CreateAsync(Event ev);
    Task UpdateAsync(Event ev);
    Task DeleteAsync(Guid id);
}