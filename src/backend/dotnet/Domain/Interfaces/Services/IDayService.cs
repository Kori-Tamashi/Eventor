using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IDayService
{
    Task<Day> CreateAsync(Day day);
    Task<Day?> GetByIdAsync(Guid id);
    Task<IEnumerable<Day>> GetAllAsync();
    Task<IEnumerable<Day>> GetByEventIdAsync(Guid eventId);
    Task<IEnumerable<Day>> GetByMenuIdAsync(Guid menuId);
    Task UpdateAsync(Day day);
    Task DeleteAsync(Guid id);
}
