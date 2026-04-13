using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/registrations")]
public class RegistrationsController(IRegistrationService registrationService) : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateRegistrationRequest request) => ExecuteAsync(async () =>
    {
        var registration = await registrationService.CreateAsync(request.ToDomain(), request.DayIds);
        var created = await registrationService.GetByIdAsync(registration.Id, includeDays: true);
        return Created($"/api/v1/registrations/{registration.Id}", (created ?? registration).ToDto());
    });

    [HttpGet("user/{userId:guid}")]
    public Task<IActionResult> GetByUserId([FromRoute] Guid userId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var registrations = await registrationService.GetByUserIdAsync(userId, filter, includeDays: true);
        var totalCount = (await registrationService.GetByUserIdAsync(userId, includeDays: true)).Count;
        return OkWithTotalCount(registrations.Select(registration => registration.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{registrationId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid registrationId) => ExecuteAsync(async () =>
    {
        var registration = await registrationService.GetByIdAsync(registrationId, includeDays: true);
        return registration is null ? NotFound() : Ok(registration.ToDto());
    });

    [HttpPut("{registrationId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid registrationId, [FromBody] UpdateRegistrationRequest request) => ExecuteAsync(async () =>
    {
        var registration = await registrationService.GetByIdAsync(registrationId, includeDays: true);
        if (registration is null)
            return NotFound();

        request.ApplyToDomain(registration);
        await registrationService.UpdateAsync(registration, request.DayIds);
        return NoContent();
    });

    [HttpDelete("{registrationId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid registrationId) => ExecuteAsync(async () =>
    {
        await registrationService.DeleteAsync(registrationId);
        return NoContent();
    });
}

[ApiController]
[Route("api/v1/admin/registrations")]
public class AdminRegistrationsController(IRegistrationService registrationService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] RegistrationFilter filter) => ExecuteAsync(async () =>
    {
        var registrations = await registrationService.GetAsync(filter, includeDays: true);
        var totalCount = (await registrationService.GetAsync(new RegistrationFilter
        {
            EventId = filter.EventId,
            UserId = filter.UserId,
            Type = filter.Type,
            Payment = filter.Payment
        }, includeDays: true)).Count;

        return OkWithTotalCount(registrations.Select(registration => registration.ToDto()).ToList(), totalCount);
    });
}