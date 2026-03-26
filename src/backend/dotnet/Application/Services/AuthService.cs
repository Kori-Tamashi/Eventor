using System.Security.Cryptography;
using System.Text;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public async Task<User> RegisterAsync(string name, string phone, Gender gender, string password)
    {
        try
        {
            var existingUser = (await userRepository.GetUsersAsync(new UserFilter { Phone = phone })).FirstOrDefault();
            if (existingUser is not null)
                throw new UserLoginAlreadyExistsException($"User with phone '{phone}' already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Phone = phone,
                Gender = gender,
                Role = UserRole.User,
                PasswordHash = HashPassword(password)
            };

            await userRepository.CreateAsync(user);
            return user;
        }
        catch (AuthServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthServiceException("Failed to register user.", ex);
        }
    }

    public async Task<string> LoginAsync(string phone, string password)
    {
        try
        {
            var user = (await userRepository.GetUsersAsync(new UserFilter { Phone = phone })).FirstOrDefault();
            if (user is null)
                throw new UserLoginNotFoundException($"User with phone '{phone}' not found.", new Exception("User not found."));

            var passwordHash = HashPassword(password);
            if (!string.Equals(user.PasswordHash, passwordHash, StringComparison.Ordinal))
                throw new IncorrectPasswordException("Incorrect password.");

            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
        catch (AuthServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthServiceException("Failed to login user.", ex);
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}