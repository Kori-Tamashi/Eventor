using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<User?> GetByIdAsync(Guid id) => userRepository.GetByIdAsync(id);

    public Task<List<User>> GetAsync(UserFilter? filter = null) => userRepository.GetUsersAsync(filter);

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            await userRepository.CreateAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            throw new UserCreateException("Failed to create user.", ex);
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            var existing = await userRepository.GetByIdAsync(user.Id);
            if (existing is null)
                throw new UserNotFoundException($"User '{user.Id}' was not found.");

            await userRepository.UpdateAsync(user);
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserUpdateException("Failed to update user.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await userRepository.GetByIdAsync(id);
            if (existing is null)
                throw new UserNotFoundException($"User '{id}' was not found.");

            await userRepository.DeleteAsync(id);
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserDeleteException("Failed to delete user.", ex);
        }
    }
}