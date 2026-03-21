using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;
    
public class MenuRepository : IMenuRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<MenuRepository> _logger;

    public MenuRepository(
        EventorDbContext context, 
        ILogger<MenuRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Menu?> GetByIdAsync(Guid id, bool includeItems = false)
    {
        try
        {
            IQueryable<MenuDb> query = _context.Menus.AsNoTracking();
            
            if (includeItems)
                query = query.Include(m => m.MenuItems);
            
            var entity = await query.FirstOrDefaultAsync(m => m.Id == id);
            
            return MenuConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.MenuRepository.GetByIdAsync failed for MenuId {MenuId}", 
                id);
            throw;
        }
    }

    public async Task<List<Menu>> GetAsync(MenuFilter? filter = null, bool includeItems = false)
    {
        try
        {
            IQueryable<MenuDb> query = _context.Menus.AsNoTracking();
            
            if (includeItems)
                query = query.Include(m => m.MenuItems);

            if (!string.IsNullOrWhiteSpace(filter?.TitleContains)) 
                query = query.Where(m => EF.Functions.ILike(
                    m.Title, $"%{filter.TitleContains}%"));
            query = query.OrderBy(m => m.Id);
            if (filter is { PageNumber: > 0, PageSize: > 0 })
                query = query
                    .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                    .Take(filter.PageSize.Value);

            var entities = await query.ToListAsync();
            
            return entities
                .Select(MenuConverter.ToDomain)
                .Where(m => m != null)
                .Cast<Menu>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.MenuRepository.GetAsync failed with filter {@Filter}", 
                filter);
            throw;
        }
    }

    public async Task CreateAsync(Menu menu)
    {
        try
        {
            await _context.Menus.AddAsync(MenuConverter.ToDb(menu)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.MenuRepository.CreateAsync failed for MenuId {menuId}", 
                menu.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Menu menu)
    {
        try
        {
            var entity = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == menu.Id);
            
            if (entity == null)
                throw new KeyNotFoundException(
                    $"Menu {menu.Id} not found in DataAccess.MenuRepository.UpdateAsync");
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.MenuRepository.UpdateAsync failed for Menu MenuId {MenuId}", 
                menu.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == id);;
            
            if (entity == null)
                throw new KeyNotFoundException(
                    $"Menu {id} not found in DataAccess.MenuRepository.DeleteAsync");

            _context.Menus.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.MenuRepository.DeleteAsync failed for MenuId {MenuId}", 
                id);
            throw;
        }
    }
}