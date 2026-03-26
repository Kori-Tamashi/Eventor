using Domain.Filters;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAsync(UserFilter? filter = null);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}