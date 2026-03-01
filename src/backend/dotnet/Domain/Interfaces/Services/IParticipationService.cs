using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IParticipationService
{
    Task<Participation> CreateAsync(Participation participation);
    Task<Participation?> GetByIdsAsync(Guid dayId, Guid registrationId);
    Task<IEnumerable<Participation>> GetByDayIdAsync(Guid dayId);
    Task<IEnumerable<Participation>> GetByRegistrationIdAsync(Guid registrationId);
    Task<IEnumerable<Participation>> GetAllAsync();
    Task UpdateAsync(Participation participation);
    Task DeleteAsync(Guid dayId, Guid registrationId);
}