using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController(IEventService eventService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] EventFilter filter) => ExecuteAsync(async () =>
    {
        var events = await eventService.GetAsync(filter);
        var totalCount = (await eventService.GetAsync(new EventFilter
        {
            LocationId = filter.LocationId,
            StartDateFrom = filter.StartDateFrom,
            StartDateTo = filter.StartDateTo,
            TitleContains = filter.TitleContains
        })).Count;

        return OkWithTotalCount(events.Select(@event => @event.ToDto()).ToList(), totalCount);
    });

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateEventRequest request) => ExecuteAsync(async () =>
    {
        var createdEvent = await eventService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/events/{createdEvent.Id}", createdEvent.ToDto());
    });

    [HttpGet("{eventId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid eventId) => ExecuteAsync(async () =>
    {
        var @event = await eventService.GetByIdAsync(eventId);
        return @event is null ? NotFound() : Ok(@event.ToDto());
    });

    [HttpPut("{eventId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid eventId, [FromBody] UpdateEventRequest request) => ExecuteAsync(async () =>
    {
        var @event = await eventService.GetByIdAsync(eventId);
        if (@event is null)
            return NotFound();

        request.ApplyToDomain(@event);
        await eventService.UpdateAsync(@event);
        return NoContent();
    });

    [HttpDelete("{eventId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid eventId) => ExecuteAsync(async () =>
    {
        await eventService.DeleteAsync(eventId);
        return NoContent();
    });

    [HttpGet("user/{userId:guid}")]
    public Task<IActionResult> GetByParticipantUserId([FromRoute] Guid userId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var events = await eventService.GetByParticipantUserIdAsync(userId, filter);
        var totalCount = (await eventService.GetByParticipantUserIdAsync(userId)).Count;
        return OkWithTotalCount(events.Select(@event => @event.ToDto()).ToList(), totalCount);
    });

    [HttpGet("organized/{userId:guid}")]
    public Task<IActionResult> GetByOrganizerUserId([FromRoute] Guid userId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var events = await eventService.GetByOrganizerUserIdAsync(userId, filter);
        var totalCount = (await eventService.GetByOrganizerUserIdAsync(userId)).Count;
        return OkWithTotalCount(events.Select(@event => @event.ToDto()).ToList(), totalCount);
    });

    [HttpGet("{eventId:guid}/days")]
    public Task<IActionResult> GetDays([FromRoute] Guid eventId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var days = await eventService.GetDaysAsync(eventId, filter);
        var totalCount = (await eventService.GetDaysAsync(eventId)).Count;
        return OkWithTotalCount(days.Select(day => day.ToDto()).ToList(), totalCount);
    });

    [HttpPost("{eventId:guid}/days")]
    public Task<IActionResult> AddDay([FromRoute] Guid eventId, [FromBody] CreateDayRequest request) => ExecuteAsync(async () =>
    {
        var day = await eventService.AddDayAsync(eventId, request.ToDomain(eventId));
        return Created($"/api/v1/events/{eventId}/days/{day.Id}", day.ToDto());
    });
}

[ApiController]
[Route("api/v1/admin/events")]
public class AdminEventsController(IEventService eventService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] EventFilter filter) => ExecuteAsync(async () =>
    {
        var events = await eventService.GetAsync(filter);
        var totalCount = (await eventService.GetAsync(new EventFilter
        {
            LocationId = filter.LocationId,
            StartDateFrom = filter.StartDateFrom,
            StartDateTo = filter.StartDateTo,
            TitleContains = filter.TitleContains
        })).Count;

        return OkWithTotalCount(events.Select(@event => @event.ToDto()).ToList(), totalCount);
    });

    [HttpDelete("{eventId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid eventId) => ExecuteAsync(async () =>
    {
        await eventService.DeleteAsync(eventId);
        return NoContent();
    });
}