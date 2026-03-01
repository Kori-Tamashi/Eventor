using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IItemService
{
    Task<Item> CreateAsync(Item item);
    Task<Item?> GetByIdAsync(Guid id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task UpdateAsync(Item item);
    Task DeleteAsync(Guid id);
}