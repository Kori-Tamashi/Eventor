using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class DayRepository : IDayRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<DayRepository> _logger;

    public DayRepository(
        EventorDbContext context, 
        ILogger<DayRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Day?> GetByIdAsync(Guid dayId)
    {
        try
        {
            var entity = await _context.Days
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dayId);

            return DayConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.DayRepository.GetByIdAsync failed for DayId {DayId}", dayId);
            throw;
        }
    }

    public async Task<List<Day>> GetAsync(DayFilter? filter = null)
    {
        try
        {
            IQueryable<DayDb> query = _context.Days.AsNoTracking();

            if (filter != null)
            {
                if (filter.EventId.HasValue)
                    query = query.Where(d => d.EventId == filter.EventId.Value);
                if (filter.MenuId.HasValue)
                    query = query.Where(d => d.MenuId == filter.MenuId.Value);
            }
            query = query.OrderBy(d => d.Id);
            if (filter is { PageNumber: > 0, PageSize: > 0 })
            {
                query = query
                    .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                    .Take(filter.PageSize.Value);
            }
            var entities = await query.ToListAsync();
            
            return entities
                .Select(DayConverter.ToDomain)
                .Where(d => d != null)
                .Cast<Day>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.DayRepository.GetAsync failed with filter {@Filter}", filter);
            throw;
        }
    }

    public async Task CreateAsync(Day day)
    {
        try
        {
            await _context.Days.AddAsync(DayConverter.ToDb(day)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.DayRepository.CreateAsync failed for DayId {DayId}", day.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Day day)
    {
        try
        {
            var entity = await _context.Days.FirstOrDefaultAsync(d => d.Id == day.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Day {day.Id} not found in DayRepository.UpdateAsync");

            entity.Title = day.Title;
            entity.Description = day.Description;
            entity.SequenceNumber = day.SequenceNumber;
            entity.EventId = day.EventId;
            entity.MenuId = day.MenuId;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.DayRepository.UpdateAsync failed for DayId {DayId}", day.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid dayId)
    {
        try
        {
            var entity = await _context.Days.FirstOrDefaultAsync(d => d.Id == dayId);
            if (entity == null)
                throw new KeyNotFoundException($"Day {dayId} not found in DayRepository.DeleteAsync");

            _context.Days.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.DayRepository.DeleteAsync failed for DayId {DayId}", dayId);
            throw;
        }
    }
}