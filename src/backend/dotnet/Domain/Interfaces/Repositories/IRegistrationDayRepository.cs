namespace Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для управления связью Registration - Day.
/// </summary>
public interface IRegistrationDayRepository
{
    /// <summary>
    /// Добавить день к регистрации
    /// </summary>
    Task AddDayAsync(Guid registrationId, Guid dayId);
    
    /// <summary>
    /// Удалить день из регистрации
    /// </summary>
    Task RemoveDayAsync(Guid registrationId, Guid dayId);
}