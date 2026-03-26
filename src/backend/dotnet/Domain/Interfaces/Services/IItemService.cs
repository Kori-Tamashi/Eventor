using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IItemService
{
    Task<Item?> GetByIdAsync(Guid id);
    Task<List<Item>> GetAsync(ItemFilter? filter = null);
    Task<Item> CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(Guid id);
}