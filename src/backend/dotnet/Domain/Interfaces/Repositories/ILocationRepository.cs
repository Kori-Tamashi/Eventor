using Domain.Filters;
using Eventor.Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id);
    Task<List<Location>> GetAsync(LocationFilter? filter = null);
    Task CreateAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(Guid id);
}