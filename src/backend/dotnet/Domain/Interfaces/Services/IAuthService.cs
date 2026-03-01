using Eventor.Domain.Models;

namespace Eventor.Domain.Interfaces.Services;

public interface IAuthService
{
    Task Register(User user, string password);
    Task<User> Login(string phone, string password);
}
