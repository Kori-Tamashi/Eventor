using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<FeedbackRepository> _logger;

    public FeedbackRepository(
        EventorDbContext context,
        ILogger<FeedbackRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Feedback?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            return FeedbackConverter.ToDomain(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.FeedbackRepository.GetByIdAsync failed for FeedbackId {FeedbackId}", id);
            throw;
        }
    }

    public async Task<List<Feedback>> GetAsync(FeedbackFilter? filter = null)
    {
        try
        {
            IQueryable<FeedbackDb> query = _context.Feedbacks
                .AsNoTracking();

            if (filter != null)
            {
                if (filter.RegistrationId.HasValue)
                    query = query.Where(f => f.RegistrationId == filter.RegistrationId.Value);
                query = filter.SortByRate switch
                {
                    Domain.Enums.FeedbackSortByRate.Asc => query.OrderBy(f => f.Rate),
                    Domain.Enums.FeedbackSortByRate.Desc => query.OrderByDescending(f => f.Rate),
                    _ => query
                };
                if (filter is { PageNumber: > 0, PageSize: > 0 })
                {
                    query = query
                        .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                        .Take(filter.PageSize.Value);
                }
            }

            var entities = await query.ToListAsync();
            return entities
                .Select(FeedbackConverter.ToDomain)
                .OfType<Feedback>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.FeedbackRepository.GetAsync failed with filter {@Filter}", filter);
            throw;
        }
    }

    public async Task CreateAsync(Feedback feedback)
    {
        try
        {
            await _context.Feedbacks.AddAsync(FeedbackConverter.ToDb(feedback)!);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "DataAccess.FeedbackRepository.CreateAsync failed for FeedbackId {FeedbackId}", feedback.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        try
        {
            var entity = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedback.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Feedback {feedback.Id} not found in FeedbackRepository.UpdateAsync");

            entity.Comment = feedback.Comment;
            entity.Rate = feedback.Rate;
            entity.RegistrationId = feedback.RegistrationId;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.FeedbackRepository.UpdateAsync failed for FeedbackId {FeedbackId}", feedback.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Feedback {id} not found in FeedbackRepository.DeleteAsync");

            _context.Feedbacks.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, 
                "DataAccess.FeedbackRepository.DeleteAsync failed for FeedbackId {FeedbackId}", id);
            throw;
        }
    }
}