using Domain.Enums;
using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(string name, string phone, Gender gender, string password);
    Task<string> LoginAsync(string phone, string password);
}