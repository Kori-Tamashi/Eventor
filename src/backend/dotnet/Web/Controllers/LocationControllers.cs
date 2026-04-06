using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/locations")]
public class LocationsController(ILocationService locationService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] LocationFilter filter) => ExecuteAsync(async () =>
    {
        var locations = await locationService.GetAsync(filter);
        var totalCount = (await locationService.GetAsync(new LocationFilter
        {
            TitleContains = filter.TitleContains,
            CostFrom = filter.CostFrom,
            CostTo = filter.CostTo,
            CapacityFrom = filter.CapacityFrom,
            CapacityTo = filter.CapacityTo
        })).Count;

        return OkWithTotalCount(locations.Select(location => location.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{locationId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid locationId) => ExecuteAsync(async () =>
    {
        var location = await locationService.GetByIdAsync(locationId);
        return location is null ? NotFound() : Ok(location.ToDto());
    });

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateLocationRequest request) => ExecuteAsync(async () =>
    {
        var location = await locationService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/locations/{location.Id}", location.ToDto());
    });
}

[ApiController]
[Route("api/v1/admin/locations")]
public class AdminLocationsController(ILocationService locationService) : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateLocationRequest request) => ExecuteAsync(async () =>
    {
        var location = await locationService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/admin/locations/{location.Id}", location.ToDto());
    });

    [HttpPut("{locationId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid locationId, [FromBody] UpdateLocationRequest request) => ExecuteAsync(async () =>
    {
        var location = await locationService.GetByIdAsync(locationId);
        if (location is null)
            return NotFound();

        request.ApplyToDomain(location);
        await locationService.UpdateAsync(location);
        return NoContent();
    });

    [HttpDelete("{locationId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid locationId) => ExecuteAsync(async () =>
    {
        await locationService.DeleteAsync(locationId);
        return NoContent();
    });
}