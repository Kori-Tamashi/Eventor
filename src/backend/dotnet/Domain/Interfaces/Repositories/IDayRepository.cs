using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IDayRepository
{
    Task<Day?> GetByIdAsync(Guid dayId);
    Task<List<Day>> GetAsync(DayFilter? filter = null);
    Task CreateAsync(Day day);
    Task UpdateAsync(Day day);
    Task DeleteAsync(Guid dayId);
}