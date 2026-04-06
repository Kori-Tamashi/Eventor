using System.Globalization;
using System.Security.Claims;
using Eventor.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkWithTotalCount<T>(IEnumerable<T> items, int totalCount)
    {
        Response.Headers["X-Total-Count"] = totalCount.ToString(CultureInfo.InvariantCulture);
        return Ok(items);
    }

    protected bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("userId");

        return Guid.TryParse(userIdValue, out userId);
    }

    protected async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    protected IActionResult HandleException(Exception exception) => exception switch
    {
        UserLoginAlreadyExistsException => Conflict(),
        UserLoginNotFoundException => Unauthorized(),
        IncorrectPasswordException => Unauthorized(),
        UserNotFoundException => NotFound(),
        LocationNotFoundException => NotFound(),
        ItemNotFoundException => NotFound(),
        MenuNotFoundException => NotFound(),
        EventNotFoundException => NotFound(),
        DayNotFoundException => NotFound(),
        FeedbackNotFoundException => NotFound(),
        RegistrationNotFoundException => NotFound(),
        DayConflictException => Conflict(),
        RegistrationAlreadyExistsException => Conflict(),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}