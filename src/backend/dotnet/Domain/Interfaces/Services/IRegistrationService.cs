using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IRegistrationService
{
    Task<Registration?> GetByIdAsync(Guid id, bool includeDays = true);
    Task<List<Registration>> GetAsync(RegistrationFilter? filter = null, bool includeDays = true);
    Task<List<Registration>> GetByUserIdAsync(Guid userId, PaginationFilter? filter = null, bool includeDays = true);
    Task<Registration> CreateAsync(Registration registration, IReadOnlyCollection<Guid> dayIds);
    Task UpdateAsync(Registration registration, IReadOnlyCollection<Guid>? dayIds = null);
    Task DeleteAsync(Guid id);
}