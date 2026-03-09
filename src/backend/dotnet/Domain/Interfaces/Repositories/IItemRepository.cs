using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id);
    Task<List<Item>> GetAsync(ItemFilter? filter = null);
    Task CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(Guid id);
}