using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class DayService(IDayRepository dayRepository) : IDayService
{
    public Task<Day?> GetByIdAsync(Guid id) => dayRepository.GetByIdAsync(id);

    public Task<List<Day>> GetAsync(DayFilter? filter = null) => dayRepository.GetAsync(filter);

    public async Task<Day> CreateAsync(Day day)
    {
        try
        {
            if (day.Id == Guid.Empty)
                day.Id = Guid.NewGuid();

            await dayRepository.CreateAsync(day);
            return day;
        }
        catch (Exception ex)
        {
            throw new DayCreateException("Failed to create day.", ex);
        }
    }

    public async Task UpdateAsync(Day day)
    {
        try
        {
            var existing = await dayRepository.GetByIdAsync(day.Id);
            if (existing is null)
                throw new DayNotFoundException($"Day '{day.Id}' was not found.");

            await dayRepository.UpdateAsync(day);
        }
        catch (DayServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DayUpdateException("Failed to update day.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await dayRepository.GetByIdAsync(id);
            if (existing is null)
                throw new DayNotFoundException($"Day '{id}' was not found.");

            await dayRepository.DeleteAsync(id);
        }
        catch (DayServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DayDeleteException("Failed to delete day.", ex);
        }
    }
}