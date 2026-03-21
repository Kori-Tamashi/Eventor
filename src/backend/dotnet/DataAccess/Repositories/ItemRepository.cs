using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<ItemRepository> _logger;

    public ItemRepository(
        EventorDbContext context,
        ILogger<ItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Item?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            return ItemConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.ItemRepository.GetByIdAsync failed for ItemId {ItemId}", id);
            throw;
        }
    }

    public async Task<List<Item>> GetAsync(ItemFilter? filter = null)
    {
        try
        {
            IQueryable<ItemDb> query = _context.Items.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter?.TitleContains))
            {
                query = query.Where(i => EF.Functions.ILike(
                    i.Title, 
                    $"%{filter.TitleContains}%"));
            }
            query = query.OrderBy(i => i.Title);
            if (filter is { PageNumber: > 0, PageSize: > 0 })
            {
                var skip = (filter.PageNumber.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }
            
            var entities = await query.ToListAsync();
            
            return entities
                .Select(ItemConverter.ToDomain)
                .Where(i => i != null)
                .Cast<Item>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.ItemRepository.GetAsync failed with filter {@Filter}", filter);
            throw;
        }
    }

    public async Task CreateAsync(Item item)
    {
        try
        {
            await _context.Items.AddAsync(ItemConverter.ToDb(item)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.ItemRepository.CreateAsync failed for ItemId {ItemId}", item.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Item item)
    {
        try
        {
            var entity = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Item {item.Id} not found in ItemRepository.UpdateAsync");

            entity.Title = item.Title;
            entity.Cost = item.Cost;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.ItemRepository.UpdateAsync failed for ItemId {ItemId}", item.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Item {id} not found in ItemRepository.DeleteAsync");

            _context.Items.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.ItemRepository.DeleteAsync failed for ItemId {ItemId}", id);
            throw;
        }
    }
}