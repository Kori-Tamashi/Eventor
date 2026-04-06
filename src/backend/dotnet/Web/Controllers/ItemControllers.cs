using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/items")]
public class ItemsController(IItemService itemService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] ItemFilter filter) => ExecuteAsync(async () =>
    {
        var items = await itemService.GetAsync(filter);
        var totalCount = (await itemService.GetAsync(new ItemFilter
        {
            TitleContains = filter.TitleContains
        })).Count;

        return OkWithTotalCount(items.Select(item => item.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{itemId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid itemId) => ExecuteAsync(async () =>
    {
        var item = await itemService.GetByIdAsync(itemId);
        return item is null ? NotFound() : Ok(item.ToDto());
    });

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateItemRequest request) => ExecuteAsync(async () =>
    {
        var item = await itemService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/items/{item.Id}", item.ToDto());
    });
}

[ApiController]
[Route("api/v1/admin/items")]
public class AdminItemsController(IItemService itemService) : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateItemRequest request) => ExecuteAsync(async () =>
    {
        var item = await itemService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/admin/items/{item.Id}", item.ToDto());
    });

    [HttpPut("{itemId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid itemId, [FromBody] UpdateItemRequest request) => ExecuteAsync(async () =>
    {
        var item = await itemService.GetByIdAsync(itemId);
        if (item is null)
            return NotFound();

        request.ApplyToDomain(item);
        await itemService.UpdateAsync(item);
        return NoContent();
    });

    [HttpDelete("{itemId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid itemId) => ExecuteAsync(async () =>
    {
        await itemService.DeleteAsync(itemId);
        return NoContent();
    });
}