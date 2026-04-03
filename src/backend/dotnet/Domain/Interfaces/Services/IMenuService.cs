using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IMenuService
{
    Task<Menu?> GetByIdAsync(Guid id, bool includeItems = true);
    Task<List<Menu>> GetAsync(MenuFilter? filter = null, bool includeItems = true);
    Task<Menu> CreateAsync(Menu menu);
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(Guid id);

    Task<List<MenuItem>> GetItemsAsync(Guid menuId, PaginationFilter? filter = null);
    Task<int> GetItemAmountAsync(Guid menuId, Guid itemId);
    Task AddItemAsync(Guid menuId, Guid itemId, int amount);
    Task UpdateItemAmountAsync(Guid menuId, Guid itemId, int amount);
    Task RemoveItemAsync(Guid menuId, Guid itemId);
}