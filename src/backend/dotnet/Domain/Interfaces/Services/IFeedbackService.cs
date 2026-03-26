using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IFeedbackService
{
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<List<Feedback>> GetAsync(FeedbackFilter? filter = null);
    Task<List<Feedback>> GetByEventIdAsync(Guid eventId, PaginationFilter? filter = null);
    Task<Feedback> CreateAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(Guid id);
}