using Domain.Filters;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Eventor.Services.Exceptions;

namespace Application.Services;

public class ItemService(IItemRepository itemRepository) : IItemService
{
    public Task<Item?> GetByIdAsync(Guid id) => itemRepository.GetByIdAsync(id);

    public Task<List<Item>> GetAsync(ItemFilter? filter = null) => itemRepository.GetAsync(filter);

    public async Task<Item> CreateAsync(Item item)
    {
        try
        {
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();

            await itemRepository.CreateAsync(item);
            return item;
        }
        catch (Exception ex)
        {
            throw new ItemCreateException("Failed to create item.", ex);
        }
    }

    public async Task UpdateAsync(Item item)
    {
        try
        {
            var existing = await itemRepository.GetByIdAsync(item.Id);
            if (existing is null)
                throw new ItemNotFoundException($"Item '{item.Id}' was not found.");

            await itemRepository.UpdateAsync(item);
        }
        catch (ItemServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ItemUpdateException("Failed to update item.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var existing = await itemRepository.GetByIdAsync(id);
            if (existing is null)
                throw new ItemNotFoundException($"Item '{id}' was not found.");

            await itemRepository.DeleteAsync(id);
        }
        catch (ItemServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ItemDeleteException("Failed to delete item.", ex);
        }
    }
}