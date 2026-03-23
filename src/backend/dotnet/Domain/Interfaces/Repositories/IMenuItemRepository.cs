using Domain.Models;

namespace Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для управления связью Menu - (MenuItem) - Item
/// </summary>
public interface IMenuItemRepository
{
    /// <summary>
    /// Добавить предмет в меню
    /// </summary>
    Task AddAsync(Guid menuId, MenuItem menuItem);
    
    /// <summary>
    /// Обновить предмет в меню
    /// </summary>
    Task UpdateAsync(Guid menuId, MenuItem menuItem);
    
    /// <summary>
    /// Удалить предмет из меню
    /// </summary>
    Task RemoveAsync(Guid menuId, Guid itemId);
}