using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class FeedbackService(
    IFeedbackRepository feedbackRepository,
    IRegistrationRepository registrationRepository) : IFeedbackService
{
    public Task<Feedback?> GetByIdAsync(Guid id) => feedbackRepository.GetByIdAsync(id);

    public Task<List<Feedback>> GetAsync(FeedbackFilter? filter = null) => feedbackRepository.GetAsync(filter);

    public async Task<List<Feedback>> GetByEventIdAsync(Guid eventId, PaginationFilter? filter = null)
    {
        var registrations = await registrationRepository.GetRegistrationsAsync(new RegistrationFilter { EventId = eventId });
        var registrationIds = registrations.Select(r => r.Id).ToHashSet();

        var feedbacks = await feedbackRepository.GetAsync();
        var filtered = feedbacks.Where(f => registrationIds.Contains(f.RegistrationId)).ToList();

        if (filter?.PageNumber is null || filter.PageSize is null)
            return filtered;

        return filtered
            .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
            .Take(filter.PageSize.Value)
            .ToList();
    }

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        try
        {
            if (feedback.Id == Guid.Empty)
                feedback.Id = Guid.NewGuid();

            await feedbackRepository.CreateAsync(feedback);
            return feedback;
        }
        catch (Exception ex)
        {
            throw new FeedbackCreateException("Failed to create feedback.", ex);
        }
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        try
        {
            var existing = await feedbackRepository.GetByIdAsync(feedback.Id);
            if (existing is null)
                throw new FeedbackNotFoundException($"Feedback '{feedback.Id}' was not found.");

            await feedbackRepository.UpdateAsync(feedback);
        }
        catch (FeedbackServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FeedbackUpdateException("Failed to update feedback.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await feedbackRepository.GetByIdAsync(id);
            if (existing is null)
                throw new FeedbackNotFoundException($"Feedback '{id}' was not found.");

            await feedbackRepository.DeleteAsync(id);
        }
        catch (FeedbackServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FeedbackDeleteException("Failed to delete feedback.", ex);
        }
    }
}