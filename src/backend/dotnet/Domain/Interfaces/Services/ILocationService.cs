using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface ILocationService
{
    Task<Location?> GetByIdAsync(Guid id);
    Task<List<Location>> GetAsync(LocationFilter? filter = null);
    Task<Location> CreateAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(Guid id);
}