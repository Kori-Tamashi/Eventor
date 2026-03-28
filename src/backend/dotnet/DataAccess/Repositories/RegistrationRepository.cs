using DataAccess.Context;
using DataAccess.Converters;
using DataAccess.Models;
using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly EventorDbContext _context;
        private readonly ILogger<RegistrationRepository> _logger;

        public RegistrationRepository(
            EventorDbContext context, 
            ILogger<RegistrationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Registration?> GetByIdAsync(Guid id, bool includeDays = false)
        {
            try
            {
                IQueryable<RegistrationDb> query = _context.Registrations.AsNoTracking();
                
                if (includeDays)
                    query = query
                        .Include(r => r.Participations)
                        .ThenInclude(p => p.Day);
                
                var entity = await query.FirstOrDefaultAsync(r => r.Id == id);
                
                return RegistrationConverter.ToDomain(entity, includeDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "DataAccess.RegistrationRepository.GetByIdAsync failed for RegistrationId {RegistrationId}", 
                    id);
                throw;
            }
        }

        public async Task<List<Registration>> GetRegistrationsAsync(
            RegistrationFilter? filter = null, 
            bool includeDays = false)
        {
            try
            {
                IQueryable<RegistrationDb> query = _context.Registrations.AsNoTracking();
                
                if (includeDays)
                    query = query
                        .Include(r => r.Participations)
                        .ThenInclude(p => p.Day);
                
                if (filter != null)
                {
                    if (filter.EventId.HasValue)
                        query = query.Where(r => r.EventId == filter.EventId.Value);
                    if (filter.UserId.HasValue)
                        query = query.Where(r => r.UserId == filter.UserId.Value);
                    if (filter.Type.HasValue)
                        query = query.Where(
                            r => r.Type == RegistrationTypeConverter.ToDb(filter.Type.Value));
                    if (filter.Payment.HasValue)
                        query = query.Where(r => r.Payment == filter.Payment.Value);
                }
                query = query.OrderBy(r => r.Id);
                if (filter is { PageNumber: > 0, PageSize: > 0 })
                    query = query
                        .Skip((filter.PageNumber.Value - 1) * filter.PageSize.Value)
                        .Take(filter.PageSize.Value);
                
                var entities = await query.ToListAsync();
                
                return entities
                    .Select(e => RegistrationConverter.ToDomain(e, includeDays))
                    .Where(r => r != null)
                    .Cast<Registration>()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "DataAccess.RegistrationRepository.GetRegistrationsAsync failed with filter {@Filter}", 
                    filter);
                throw;
            }
        }

        public async Task CreateAsync(Registration registration)
        {
            try
            {
                await _context.Registrations.AddAsync(RegistrationConverter.ToDb(registration)!);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "DataAccess.RegistrationRepository.CreateAsync failed for RegistrationId {RegistrationId}", 
                    registration.Id);
                throw;
            }
        }

        public async Task UpdateAsync(Registration registration)
        {
            try
            {
                var entity = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.Id == registration.Id);
                
                if (entity == null)
                    throw new KeyNotFoundException(
                        $"Registration {registration.Id} not found in DataAccess.RegistrationRepository.UpdateAsync");

                entity.EventId = registration.EventId;
                entity.UserId = registration.UserId;
                entity.Type = RegistrationTypeConverter.ToDb(registration.Type);
                entity.Payment = registration.Payment;
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, 
                    "DataAccess.RegistrationRepository.UpdateAsync failed for RegistrationId {RegistrationId}", 
                    registration.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.Id == id);
                
                if (entity == null)
                    throw new KeyNotFoundException(
                        $"Registration {id} not found in DataAccess.RegistrationRepository.DeleteAsync");

                _context.Registrations.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, 
                    "DataAccess.RegistrationRepository.DeleteAsync failed for RegistrationId {RegistrationId}", 
                    id);
                throw;
            }
        }
    }
}