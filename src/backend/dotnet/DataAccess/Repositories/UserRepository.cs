using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        EventorDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return entity != null ? UserConverter.ToDomain(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "DataAccess.UserRepository.GetById failed for UserId {UserId}", id);
            throw new Exception($"Failed to fetch user {id} in UserRepository.GetById", ex);
        }
    }

    public async Task<List<User>> GetUsersAsync(UserFilter? filter = null)
    {
        try
        {
            IQueryable<UserDb> query = _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Name.Substring(0, 1));
            if (filter != null)
            {
                if (filter.NameContains != null)
                    query = query.Where(u => EF.Functions.ILike(
                        u.Name,
                        $"%{filter.NameContains}%"));
                if (filter.Phone != null)
                    query = query.Where(u => u.Phone == filter.Phone);
                if (filter.Role.HasValue)
                    query = query.Where(u => u.Role == UserRoleConverter.ToDb(filter.Role.Value));
                if (filter.Gender.HasValue)
                    query = query.Where(u => u.Gender == GenderConverter.ToDb(filter.Gender.Value));
                if (filter is { PageNumber: > 0, PageSize: > 0 })
                {
                    query = query
                        .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                        .Take(filter.PageSize.Value);
                }
            }

            var entities = await query.ToListAsync();
            return entities.Select(UserConverter.ToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DataAccess.UserRepository.GetUsersAsync failed with filter {@Filter}", filter);
            throw new Exception("Failed to fetch users in UserRepository.GetUsersAsync", ex);
        }
    }

    public async Task CreateAsync(User user)
    {
        try
        {
            var entity = UserConverter.ToDb(user);
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DataAccess.UserRepository.CreateAsync failed for User {@User}", user);
            throw new Exception($"Failed to create user in UserRepository.CreateAsync", ex);
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User {user.Id} not found in UserRepository.UpdateAsync");
            }

            entity.Name = user.Name;
            entity.Phone = user.Phone;
            entity.Gender = GenderConverter.ToDb(user.Gender);
            entity.Role = UserRoleConverter.ToDb(user.Role);
            entity.PasswordHash = user.PasswordHash;

            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "DataAccess.UserRepository.UpdateAsync failed for User {@User}", user);
            throw new Exception($"Failed to update user {user.Id} in UserRepository.UpdateAsync", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User {id} not found in UserRepository.DeleteAsync");
            }

            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "DataAccess.UserRepository.DeleteAsync failed for UserId {UserId}", id);
            throw new Exception($"Failed to delete user {id} in UserRepository.DeleteAsync", ex);
        }
    }
}