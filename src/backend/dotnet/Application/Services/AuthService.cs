using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Enums;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Application.Configuration;

namespace Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IOptions<JwtOptions> jwtOptions) : IAuthService
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

            return GenerateJwtToken(user);
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

    private string GenerateJwtToken(User user)
    {
        var options = jwtOptions.Value;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}