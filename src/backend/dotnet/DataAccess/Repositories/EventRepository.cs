using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(
        EventorDbContext context, 
        ILogger<EventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            return EventConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.EventRepository.GetByIdAsync failed for EventId {EventId}", id);
            throw;
        }
    }

    public async Task<List<Event>> GetAsync(EventFilter? filter = null)
    {
        try
        {
            IQueryable<EventDb> query = _context.Events.AsNoTracking();

            if (filter != null)
            {
                if (filter.LocationId.HasValue)
                    query = query.Where(e => e.LocationId == filter.LocationId.Value);
                if (filter.StartDateFrom.HasValue)
                    query = query.Where(e => e.StartDate >= filter.StartDateFrom.Value);
                if (filter.StartDateTo.HasValue)
                    query = query.Where(e => e.StartDate <= filter.StartDateTo.Value);
                if (!string.IsNullOrWhiteSpace(filter.TitleContains))
                    query = query.Where(e => EF.Functions.ILike(
                        e.Title, $"%{filter.TitleContains}%"));
            }
            query = query.OrderBy(e => e.StartDate).ThenBy(e => e.Id);
            if (filter is { PageNumber: > 0, PageSize: > 0 })
            {
                query = query
                    .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                    .Take(filter.PageSize.Value);
            }
            var entities = await query.ToListAsync();
            
            return entities
                .Select(EventConverter.ToDomain)
                .Where(e => e != null)
                .Cast<Event>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DataAccess.EventRepository.GetAsync failed with filter {@Filter}", filter);
            throw;
        }
    }

    public async Task CreateAsync(Event ev)
    {
        try
        {
            await _context.Events.AddAsync(EventConverter.ToDb(ev)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.EventRepository.CreateAsync failed for EventId {EventId}", ev.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Event ev)
    {
        try
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == ev.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Event {ev.Id} not found in EventRepository.UpdateAsync");

            entity.Title = ev.Title;
            entity.Description = ev.Description;
            entity.StartDate = ev.StartDate;
            entity.LocationId = ev.LocationId;
            entity.DaysCount = ev.DaysCount;
            entity.Percent = ev.Percent;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.EventRepository.UpdateAsync failed for EventId {EventId}", ev.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Event {id} not found in EventRepository.DeleteAsync");

            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.EventRepository.DeleteAsync failed for EventId {EventId}", id);
            throw;
        }
    }
}