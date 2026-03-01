using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByPhoneAsync(string phone);
    Task<IEnumerable<User>> GetAllAsync();
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}