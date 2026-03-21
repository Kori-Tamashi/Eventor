using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace DataAccess.Repositories;

public class LocationRepository: ILocationRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(
        EventorDbContext context, 
        ILogger<LocationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            return LocationConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DataAccess.LocationRepository.GetById failed for LocationId {LocationId}", id);
            throw;
        }
    }

    public async Task<List<Location>> GetAsync(LocationFilter? filter = null)
    {
        try
        {
            IQueryable<LocationDb> query = _context.Locations.AsNoTracking();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.TitleContains))
                    query = query.Where(l => EF.Functions.ILike(
                        l.Title, 
                        $"%{filter.TitleContains}%"));
                if (filter.CostFrom.HasValue)
                    query = query.Where(l => l.Cost >= filter.CostFrom.Value);
                if (filter.CostTo.HasValue)
                    query = query.Where(l => l.Cost <= filter.CostTo.Value);
                if (filter.CapacityFrom.HasValue)
                    query = query.Where(l => l.Capacity >= filter.CapacityFrom.Value);
                if (filter.CapacityTo.HasValue)
                    query = query.Where(l => l.Capacity <= filter.CapacityTo.Value);
            }
            query = query.OrderBy(l => l.Title);
            if (filter is { PageNumber: > 0, PageSize: > 0 })
            {
                query = query
                    .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                    .Take(filter.PageSize.Value);
            }
            
            var entities = await query.ToListAsync();
            
            return entities
                .Select(LocationConverter.ToDomain)
                .Where(l => l != null)
                .Cast<Location>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.LocationRepository.GetAsync failed with filter {@Filter}", filter);
            throw;
        }
    }

    public async Task CreateAsync(Location location)
    {
        try
        {
            await _context.Locations.AddAsync(LocationConverter.ToDb(location)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.LocationRepository.CreateAsync failed for LocationId {LocationId}", location.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Location location)
    {
        try
        {
            var entity = await _context.Locations.FirstOrDefaultAsync(l => l.Id == location.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Location {location.Id} not found in LocationRepository.UpdateAsync");

            entity.Title = location.Title;
            entity.Description = location.Description;
            entity.Cost = location.Cost;
            entity.Capacity = location.Capacity;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.LocationRepository.UpdateAsync failed for LocationId {LocationId}", location.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Location {id} not found in LocationRepository.DeleteAsync");

            _context.Locations.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.LocationRepository.DeleteAsync failed for LocationId {LocationId}", id);
            throw;
        }
    }
}