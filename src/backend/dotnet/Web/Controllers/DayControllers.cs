using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/days")]
public class DaysController(IDayService dayService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] DayFilter filter) => ExecuteAsync(async () =>
    {
        var days = await dayService.GetAsync(filter);
        var totalCount = (await dayService.GetAsync(new DayFilter
        {
            EventId = filter.EventId,
            MenuId = filter.MenuId
        })).Count;

        return OkWithTotalCount(days.Select(day => day.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{dayId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid dayId) => ExecuteAsync(async () =>
    {
        var day = await dayService.GetByIdAsync(dayId);
        return day is null ? NotFound() : Ok(day.ToDto());
    });

    [HttpPut("{dayId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid dayId, [FromBody] UpdateDayRequest request) => ExecuteAsync(async () =>
    {
        var day = await dayService.GetByIdAsync(dayId);
        if (day is null)
            return NotFound();

        request.ApplyToDomain(day);
        await dayService.UpdateAsync(day);
        return NoContent();
    });

    [HttpDelete("{dayId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid dayId) => ExecuteAsync(async () =>
    {
        await dayService.DeleteAsync(dayId);
        return NoContent();
    });
}