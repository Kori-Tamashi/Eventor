using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IMenuItemsService
{
    Task<MenuItems> CreateAsync(MenuItems menuItem);
    Task<MenuItems?> GetByIdsAsync(Guid menuId, Guid itemId); 
    Task<IEnumerable<MenuItems>> GetByMenuIdAsync(Guid menuId);
    Task<IEnumerable<Item>> GetAllItemsByMenuAsync(Guid menuId);
    Task<Menu?> GetMenuByDayAsync(Guid dayId);
    Task<IEnumerable<MenuItems>> GetByItemIdAsync(Guid itemId);
    Task<double> GetAmountOfItemAsync(Guid menuId, Guid itemId);
    Task UpdateAsync(MenuItems menuItem);
    Task DeleteAsync(Guid menuId, Guid itemId);
}
