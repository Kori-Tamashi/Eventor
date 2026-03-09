using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(Guid id, bool includeItems = false);
    Task<List<Menu>> GetAsync(MenuFilter? filter = null, bool includeItems = false);
    Task CreateAsync(Menu menu);
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(Guid id);
}