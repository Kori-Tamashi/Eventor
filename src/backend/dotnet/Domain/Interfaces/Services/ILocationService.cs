using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface ILocationService
{
    Task<Location> CreateAsync(Location location);
    Task<Location?> GetByIdAsync(Guid id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task UpdateAsync(Location location);
    Task DeleteAsync(Guid id);
}