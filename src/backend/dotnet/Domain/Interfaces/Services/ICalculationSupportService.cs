using Domain.Interfaces.Services;

namespace Application.Services;

public interface ICalculationSupportService
{
    Task<decimal> GetSingleDayCostAsync(Guid dayId);
    Task<decimal> GetDayCoefficientAsync(IEnumerable<Guid> dayIds);
    Task EnsureDaysFromSameEventAsync(IEnumerable<Guid> dayIds);
    Task<int> GetParticipantsByDayAsync(Guid dayId, bool includePrivileged);
    Task<int> GetParticipantsCountExactAsync(IReadOnlyCollection<Guid> dayIds, bool includePrivileged);
    Task<List<IReadOnlyCollection<Guid>>> GetCurrentDayCombinationsAsync(Guid eventId, bool includePrivileged);
}
