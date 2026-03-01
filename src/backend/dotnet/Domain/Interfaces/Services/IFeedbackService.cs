using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IFeedbackService
{
    Task<Feedback> CreateAsync(Feedback feedback);
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<IEnumerable<Feedback>> GetByRegistrationIdAsync(Guid registationId);
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(Guid id);
}
