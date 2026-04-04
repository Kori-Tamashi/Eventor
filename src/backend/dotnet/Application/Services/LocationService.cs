using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class LocationService(ILocationRepository locationRepository) : ILocationService
{
    public Task<Location?> GetByIdAsync(Guid id) => locationRepository.GetByIdAsync(id);

    public Task<List<Location>> GetAsync(LocationFilter? filter = null) => locationRepository.GetAsync(filter);

    public async Task<Location> CreateAsync(Location location)
    {
        try
        {
            if (location.Id == Guid.Empty)
                location.Id = Guid.NewGuid();

            await locationRepository.CreateAsync(location);
            return location;
        }
        catch (Exception ex)
        {
            throw new LocationCreateException("Failed to create location.", ex);
        }
    }

    public async Task UpdateAsync(Location location)
    {
        try
        {
            var existing = await locationRepository.GetByIdAsync(location.Id);
            if (existing is null)
                throw new LocationNotFoundException($"Location '{location.Id}' was not found.");

            await locationRepository.UpdateAsync(location);
        }
        catch (LocationServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LocationUpdateException("Failed to update location.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await locationRepository.GetByIdAsync(id);
            if (existing is null)
                throw new LocationNotFoundException($"Location '{id}' was not found.");

            await locationRepository.DeleteAsync(id);
        }
        catch (LocationServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LocationDeleteException("Failed to delete location.", ex);
        }
    }
}