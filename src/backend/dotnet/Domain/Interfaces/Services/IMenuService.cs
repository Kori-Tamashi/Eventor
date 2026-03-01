using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IMenuService
{
    Task<Menu> CreateAsync(Menu menu);
    Task<Menu?> GetByIdAsync(Guid id);
    Task<IEnumerable<Menu>> GetAllAsync();
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(Guid id);
}