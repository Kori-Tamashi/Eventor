using Domain.Filters;
using Eventor.Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<List<Feedback>> GetAsync(FeedbackFilter? filter = null);
    Task CreateAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(Guid id);
}