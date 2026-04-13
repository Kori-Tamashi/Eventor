using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/economy")]
public class EconomyController(ICalculationService calculationService) : ApiControllerBase
{
    [HttpGet("items/{itemId:guid}/cost")]
    public Task<IActionResult> GetItemCost([FromRoute] Guid itemId) => ExecuteAsync(async () => Ok(await calculationService.GetItemCostAsync(itemId)));

    [HttpGet("menus/{menuId:guid}/cost")]
    public Task<IActionResult> GetMenuCost([FromRoute] Guid menuId) => ExecuteAsync(async () => Ok(await calculationService.GetMenuCostAsync(menuId)));

    [HttpPost("days/cost")]
    public Task<IActionResult> GetDaysCost([FromBody] List<Guid> dayIds) => ExecuteAsync(async () => Ok(await calculationService.GetDaysCostAsync(dayIds)));

    [HttpGet("events/{eventId:guid}/cost")]
    public Task<IActionResult> GetEventCost([FromRoute] Guid eventId) => ExecuteAsync(async () => Ok(await calculationService.GetEventCostAsync(eventId)));

    [HttpGet("days/{dayId:guid}/price")]
    public Task<IActionResult> GetDayPrice([FromRoute] Guid dayId) => ExecuteAsync(async () => Ok(await calculationService.GetDayPriceAsync(dayId)));

    [HttpGet("days/{dayId:guid}/price/with-privileges")]
    public Task<IActionResult> GetDayPriceWithPrivileges([FromRoute] Guid dayId) => ExecuteAsync(async () => Ok(await calculationService.GetDayPriceWithPrivilegesAsync(dayId)));

    [HttpPost("days/price")]
    public Task<IActionResult> GetDaysPrice([FromBody] List<Guid> dayIds) => ExecuteAsync(async () => Ok(await calculationService.GetDaysPriceAsync(dayIds)));

    [HttpGet("events/{eventId:guid}/fundamental-price/1d")]
    public Task<IActionResult> GetFundamentalPriceForSingleDay([FromRoute] Guid eventId) => ExecuteAsync(async () => Ok(await calculationService.GetFundamentalPriceForSingleDayAsync(eventId)));

    [HttpGet("events/{eventId:guid}/fundamental-price/nd")]
    public Task<IActionResult> GetFundamentalPriceForMultiDay([FromRoute] Guid eventId) => ExecuteAsync(async () => Ok(await calculationService.GetFundamentalPriceForMultiDayAsync(eventId)));

    [HttpGet("events/{eventId:guid}/balance/1d")]
    public Task<IActionResult> IsSingleDayCaseBalanced([FromRoute] Guid eventId) => ExecuteAsync(async () => Ok(await calculationService.IsSingleDayCaseBalancedAsync(eventId)));

    [HttpGet("events/{eventId:guid}/balance/nd")]
    public Task<IActionResult> IsMultiDayCaseBalanced([FromRoute] Guid eventId) => ExecuteAsync(async () => Ok(await calculationService.IsMultiDayCaseBalancedAsync(eventId)));
}