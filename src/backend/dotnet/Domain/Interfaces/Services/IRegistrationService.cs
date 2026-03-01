using Domain.Enums;
using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IRegistrationService
{
    Task<Registration> CreateAsync(Registration registration);
    Task<Registration?> GetByIdAsync(Guid id);
    Task<IEnumerable<Registration>> GetByEventIdAsync(Guid eventId);
    Task<IEnumerable<Registration>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Registration>> GetByPaymentAsync(bool payment);
    Task<IEnumerable<Registration>> GetByRegistrationTypeAsync(RegistrationTypeEnum type);
    Task<IEnumerable<Registration>> GetAllAsync();
    Task UpdateAsync(Registration registration);
    Task DeleteAsync(Guid id);
}