using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/feedbacks")]
public class FeedbacksController(IFeedbackService feedbackService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] FeedbackFilter filter) => ExecuteAsync(async () =>
    {
        var feedbacks = await feedbackService.GetAsync(filter);
        var totalCount = (await feedbackService.GetAsync(new FeedbackFilter
        {
            RegistrationId = filter.RegistrationId,
            SortByRate = filter.SortByRate
        })).Count;

        return OkWithTotalCount(feedbacks.Select(feedback => feedback.ToDto()).ToList(), totalCount);
    });

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateFeedbackRequest request) => ExecuteAsync(async () =>
    {
        var feedback = await feedbackService.CreateAsync(request.ToDomain());
        return Created($"/api/v1/feedbacks/{feedback.Id}", feedback.ToDto());
    });

    [HttpGet("{feedbackId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid feedbackId) => ExecuteAsync(async () =>
    {
        var feedback = await feedbackService.GetByIdAsync(feedbackId);
        return feedback is null ? NotFound() : Ok(feedback.ToDto());
    });

    [HttpPut("{feedbackId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid feedbackId, [FromBody] UpdateFeedbackRequest request) => ExecuteAsync(async () =>
    {
        var feedback = await feedbackService.GetByIdAsync(feedbackId);
        if (feedback is null)
            return NotFound();

        request.ApplyToDomain(feedback);
        await feedbackService.UpdateAsync(feedback);
        return NoContent();
    });

    [HttpDelete("{feedbackId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid feedbackId) => ExecuteAsync(async () =>
    {
        await feedbackService.DeleteAsync(feedbackId);
        return NoContent();
    });

    [HttpGet("event/{eventId:guid}")]
    public Task<IActionResult> GetByEventId([FromRoute] Guid eventId, [FromQuery] PaginationFilter filter) => ExecuteAsync(async () =>
    {
        var feedbacks = await feedbackService.GetByEventIdAsync(eventId, filter);
        var totalCount = (await feedbackService.GetByEventIdAsync(eventId)).Count;
        return OkWithTotalCount(feedbacks.Select(feedback => feedback.ToDto()).ToList(), totalCount);
    });
}

[ApiController]
[Route("api/v1/admin/feedbacks")]
public class AdminFeedbacksController(IFeedbackService feedbackService) : ApiControllerBase
{
    [HttpDelete("{feedbackId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid feedbackId) => ExecuteAsync(async () =>
    {
        await feedbackService.DeleteAsync(feedbackId);
        return NoContent();
    });
}