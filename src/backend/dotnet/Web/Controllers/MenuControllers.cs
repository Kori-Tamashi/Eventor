using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/menus")]
public class MenusController(IMenuService menuService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] MenuFilter filter) => ExecuteAsync(async () =>
    {
        var menus = await menuService.GetAsync(filter);
        var totalCount = (await menuService.GetAsync(new MenuFilter
        {
            TitleContains = filter.TitleContains
        })).Count;

        return OkWithTotalCount(menus.Select(menu => menu.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{menuId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid menuId) => ExecuteAsync(async () =>
    {
        var menu = await menuService.GetByIdAsync(menuId);
        return menu is null ? NotFound() : Ok(menu.ToDto());
    });

    [HttpGet("{menuId:guid}/items")]
    public Task<IActionResult> GetItems([FromRoute] Guid menuId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var items = await menuService.GetItemsAsync(menuId, filter);
        var totalCount = (await menuService.GetItemsAsync(menuId)).Count;
        return OkWithTotalCount(items.Select(item => item.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{menuId:guid}/items/{itemId:guid}/amount")]
    public Task<IActionResult> GetItemAmount([FromRoute] Guid menuId, [FromRoute] Guid itemId) => ExecuteAsync(async () =>
    {
        var amount = await menuService.GetItemAmountAsync(menuId, itemId);
        return Ok(amount);
    });
}

[ApiController]
[Route("api/v1/admin/menus")]
public class AdminMenusController(IMenuService menuService) : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateMenuRequest request) => ExecuteAsync(async () =>
    {
        var menu = await menuService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/admin/menus/{menu.Id}", menu.ToDto());
    });

    [HttpPut("{menuId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid menuId, [FromBody] UpdateMenuRequest request) => ExecuteAsync(async () =>
    {
        var menu = await menuService.GetByIdAsync(menuId);
        if (menu is null)
            return NotFound();

        request.ApplyToDomain(menu);
        await menuService.UpdateAsync(menu);
        return NoContent();
    });

    [HttpDelete("{menuId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid menuId) => ExecuteAsync(async () =>
    {
        await menuService.DeleteAsync(menuId);
        return NoContent();
    });

    [HttpPost("{menuId:guid}/items/{itemId:guid}")]
    public Task<IActionResult> AddItem([FromRoute] Guid menuId, [FromRoute] Guid itemId, [FromQuery] int amount) => ExecuteAsync(async () =>
    {
        await menuService.AddItemAsync(menuId, itemId, amount);
        return Created($"/api/v1/admin/menus/{menuId}/items/{itemId}", null);
    });

    [HttpPut("{menuId:guid}/items/{itemId:guid}")]
    public Task<IActionResult> UpdateItemAmount([FromRoute] Guid menuId, [FromRoute] Guid itemId, [FromQuery] int amount) => ExecuteAsync(async () =>
    {
        await menuService.UpdateItemAmountAsync(menuId, itemId, amount);
        return NoContent();
    });

    [HttpDelete("{menuId:guid}/items/{itemId:guid}")]
    public Task<IActionResult> RemoveItem([FromRoute] Guid menuId, [FromRoute] Guid itemId) => ExecuteAsync(async () =>
    {
        await menuService.RemoveItemAsync(menuId, itemId);
        return NoContent();
    });
}