using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IEventReviewService
{
    Task<IEnumerable<Feedback>> GetByEventIdAsync(Guid eventId);
    Task<IEnumerable<Feedback>> GetByUserIdAsync(Guid userId);
    Task<double> GetEventAverageRateAsync(Guid eventId); 
}