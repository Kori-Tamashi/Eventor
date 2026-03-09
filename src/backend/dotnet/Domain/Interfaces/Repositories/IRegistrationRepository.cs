using Domain.Filters;
using Eventor.Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IRegistrationRepository
{
    Task<Registration?> GetByIdAsync(Guid id, bool includeDays = false);
    Task<List<Registration>> GetRegistrationsAsync(
        RegistrationFilter? filter = null,
        bool includeDays = false);
    Task CreateAsync(Registration registration);
    Task UpdateAsync(Registration registration);
    Task DeleteAsync(Guid id);
}