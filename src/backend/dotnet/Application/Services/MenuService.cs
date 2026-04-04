using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class MenuService(
    IMenuRepository menuRepository,
    IMenuItemRepository menuItemRepository) : IMenuService
{
    public Task<Menu?> GetByIdAsync(Guid id, bool includeItems = true) => menuRepository.GetByIdAsync(id, includeItems);

    public Task<List<Menu>> GetAsync(MenuFilter? filter = null, bool includeItems = true) =>
        menuRepository.GetAsync(filter, includeItems);

    public async Task<Menu> CreateAsync(Menu menu)
    {
        try
        {
            if (menu.Id == Guid.Empty)
                menu.Id = Guid.NewGuid();

            await menuRepository.CreateAsync(menu);
            return menu;
        }
        catch (Exception ex)
        {
            throw new MenuCreateException("Failed to create menu.", ex);
        }
    }

    public async Task UpdateAsync(Menu menu)
    {
        try
        {
            var existing = await menuRepository.GetByIdAsync(menu.Id);
            if (existing is null)
                throw new MenuNotFoundException($"Menu '{menu.Id}' was not found.");

            await menuRepository.UpdateAsync(menu);
        }
        catch (MenuServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MenuUpdateException("Failed to update menu.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await menuRepository.GetByIdAsync(id);
            if (existing is null)
                throw new MenuNotFoundException($"Menu '{id}' was not found.");

            await menuRepository.DeleteAsync(id);
        }
        catch (MenuServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MenuDeleteException("Failed to delete menu.", ex);
        }
    }

    public async Task<List<MenuItem>> GetItemsAsync(Guid menuId, PaginationFilter? filter = null)
    {
        var menu = await menuRepository.GetByIdAsync(menuId, includeItems: true);
        if (menu is null)
            throw new MenuNotFoundException($"Menu '{menuId}' was not found.");

        var items = menu.MenuItems ?? [];
        return ApplyPagination(items, filter);
    }

    public async Task<int> GetItemAmountAsync(Guid menuId, Guid itemId)
    {
        var menu = await menuRepository.GetByIdAsync(menuId, includeItems: true);
        if (menu is null)
            throw new MenuNotFoundException($"Menu '{menuId}' was not found.");

        var menuItem = menu.MenuItems.FirstOrDefault(x => x.ItemId == itemId);
        if (menuItem is null)
            throw new MenuServiceException($"Item '{itemId}' is not in menu '{menuId}'.", new Exception("Menu item not found."));

        return menuItem.Amount;
    }

    public async Task AddItemAsync(Guid menuId, Guid itemId, int amount)
    {
        try
        {
            var menu = await menuRepository.GetByIdAsync(menuId, includeItems: true);
            if (menu is null)
                throw new MenuNotFoundException($"Menu '{menuId}' was not found.");

            if (menu.MenuItems.Any(x => x.ItemId == itemId))
                throw new MenuServiceException($"Item '{itemId}' already exists in menu '{menuId}'.", new Exception("Menu item already exists."));

            await menuItemRepository.AddAsync(menuId, new MenuItem(itemId, amount));
        }
        catch (MenuServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MenuServiceException("Failed to add item to menu.", ex);
        }
    }

    public async Task UpdateItemAmountAsync(Guid menuId, Guid itemId, int amount)
    {
        try
        {
            var menu = await menuRepository.GetByIdAsync(menuId, includeItems: true);
            if (menu is null)
                throw new MenuNotFoundException($"Menu '{menuId}' was not found.");

            if (menu.MenuItems.All(x => x.ItemId != itemId))
                throw new MenuServiceException($"Item '{itemId}' is not in menu '{menuId}'.", new Exception("Menu item not found."));

            await menuItemRepository.UpdateAsync(menuId, new MenuItem(itemId, amount));
        }
        catch (MenuServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MenuServiceException("Failed to update menu item amount.", ex);
        }
    }

    public async Task RemoveItemAsync(Guid menuId, Guid itemId)
    {
        try
        {
            var menu = await menuRepository.GetByIdAsync(menuId, includeItems: true);
            if (menu is null)
                throw new MenuNotFoundException($"Menu '{menuId}' was not found.");

            await menuItemRepository.RemoveAsync(menuId, itemId);
        }
        catch (MenuServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MenuServiceException("Failed to remove item from menu.", ex);
        }
    }

    private static List<T> ApplyPagination<T>(IReadOnlyCollection<T> source, PaginationFilter? filter)
    {
        if (filter?.PageNumber is null || filter.PageSize is null)
            return source.ToList();

        return source
            .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
            .Take(filter.PageSize.Value)
            .ToList();
    }
}