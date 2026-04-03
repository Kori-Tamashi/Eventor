using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class RegistrationService(
    IRegistrationRepository registrationRepository,
    IRegistrationDayRepository registrationDayRepository) : IRegistrationService
{
    public Task<Registration?> GetByIdAsync(Guid id, bool includeDays = true) =>
        registrationRepository.GetByIdAsync(id, includeDays);

    public Task<List<Registration>> GetAsync(RegistrationFilter? filter = null, bool includeDays = true) =>
        registrationRepository.GetRegistrationsAsync(filter, includeDays);

    public Task<List<Registration>> GetByUserIdAsync(Guid userId, PaginationFilter? filter = null, bool includeDays = true) =>
        registrationRepository.GetRegistrationsAsync(
            new RegistrationFilter
            {
                UserId = userId,
                PageNumber = filter?.PageNumber,
                PageSize = filter?.PageSize
            },
            includeDays);

    public async Task<Registration> CreateAsync(Registration registration, IReadOnlyCollection<Guid> dayIds)
    {
        try
        {
            if (registration.Id == Guid.Empty)
                registration.Id = Guid.NewGuid();

            await registrationRepository.CreateAsync(registration);

            foreach (var dayId in dayIds.Distinct())
                await registrationDayRepository.AddDayAsync(registration.Id, dayId);

            registration.Days = registration.Days;
            return registration;
        }
        catch (Exception ex)
        {
            throw new RegistrationServiceException("Failed to create registration.", ex);
        }
    }

    public async Task UpdateAsync(Registration registration, IReadOnlyCollection<Guid>? dayIds = null)
    {
        try
        {
            var existing = await registrationRepository.GetByIdAsync(registration.Id, includeDays: true);
            if (existing is null)
                throw new RegistrationServiceException($"Registration '{registration.Id}' was not found.", new Exception("Registration not found."));

            await registrationRepository.UpdateAsync(registration);

            if (dayIds is null)
                return;

            var existingDayIds = existing.Days.Select(d => d.Id).ToHashSet();
            var targetDayIds = dayIds.Distinct().ToHashSet();

            foreach (var dayId in targetDayIds.Except(existingDayIds))
                await registrationDayRepository.AddDayAsync(registration.Id, dayId);

            foreach (var dayId in existingDayIds.Except(targetDayIds))
                await registrationDayRepository.RemoveDayAsync(registration.Id, dayId);
        }
        catch (RegistrationServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RegistrationServiceException("Failed to update registration.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await registrationRepository.GetByIdAsync(id);
            if (existing is null)
                throw new RegistrationServiceException($"Registration '{id}' was not found.", new Exception("Registration not found."));

            await registrationRepository.DeleteAsync(id);
        }
        catch (RegistrationServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RegistrationServiceException("Failed to delete registration.", ex);
        }
    }
}