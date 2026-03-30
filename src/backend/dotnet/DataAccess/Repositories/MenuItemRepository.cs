using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<MenuItemRepository> _logger;

    public MenuItemRepository(EventorDbContext context, ILogger<MenuItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Guid menuId, MenuItem menuItem)
    {
        try
        {
            await _context.MenuItems.AddAsync(MenuItemConverter.ToDb(menuItem, menuId)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "DataAccess.MenuItemRepository.AddAsync failed for MenuId {MenuId}, ItemId {ItemId}",
                menuId, 
                menuItem.ItemId);
            throw;
        }
    }

    public async Task UpdateAsync(Guid menuId, MenuItem menuItem)
    {
        try
        {
            var entity = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.MenuId == menuId && mi.ItemId == menuItem.ItemId);

            if (entity == null)
                throw new KeyNotFoundException(
                    $"MenuItem not found for MenuId {menuId} and ItemId {menuItem.ItemId}");

            entity.Amount = menuItem.Amount;
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex,
                "DataAccess.MenuItemRepository.UpdateAsync failed for MenuId {MenuId}, ItemId {ItemId}",
                menuId, 
                menuItem.ItemId);
            throw;
        }
    }

    public async Task RemoveAsync(Guid menuId, Guid itemId)
    {
        try
        {
            var entity = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.MenuId == menuId && mi.ItemId == itemId);

            if (entity == null)
                throw new KeyNotFoundException(
                    $"MenuItem not found for MenuId {menuId} and ItemId {itemId} in DataAccess.MenuItemRepository.DeleteAsync");

            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex,
                "DataAccess.MenuItemRepository.RemoveAsync failed for MenuId {MenuId}, ItemId {ItemId}",
                menuId, 
                itemId);
            throw;
        }
    }
}