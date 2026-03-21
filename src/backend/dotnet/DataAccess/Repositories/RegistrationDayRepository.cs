using DataAccess.Context;
using DataAccess.Models;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class RegistrationDayRepository : IRegistrationDayRepository
{
    private readonly EventorDbContext _context;
    private readonly ILogger<RegistrationDayRepository> _logger;

    public RegistrationDayRepository(
        EventorDbContext context,
        ILogger<RegistrationDayRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddDayAsync(Guid registrationId, Guid dayId)
    {
        try
        {
            var entity = new ParticipationDb(dayId, registrationId);

            await _context.Participations.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "DataAccess.RegistrationDayRepository.AddDayAsync failed for RegistrationId {RegistrationId}, DayId {DayId}",
                registrationId, 
                dayId);
            throw;
        }
    }

    public async Task RemoveDayAsync(Guid registrationId, Guid dayId)
    {
        try
        {
            var entity = await _context.Participations
                .FirstOrDefaultAsync(p => p.RegistrationId == registrationId && p.DayId == dayId);

            if (entity == null)
                throw new KeyNotFoundException(
                    $"Participation not found for RegistrationId {registrationId} and DayId {dayId} in DataAccess.RegistrationRepository.DeleteAsync");

            _context.Participations.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex,
                "DataAccess.RegistrationDayRepository.RemoveDayAsync failed for RegistrationId {RegistrationId}, DayId {DayId}",
                registrationId, 
                dayId);
            throw;
        }
    }
}