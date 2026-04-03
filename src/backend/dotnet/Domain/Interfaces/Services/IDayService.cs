using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IDayService
{
    Task<Day?> GetByIdAsync(Guid id);
    Task<List<Day>> GetAsync(DayFilter? filter = null);
    Task<Day> CreateAsync(Day day);
    Task UpdateAsync(Day day);
    Task DeleteAsync(Guid id);
}