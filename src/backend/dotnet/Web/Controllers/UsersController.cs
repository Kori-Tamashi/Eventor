using Domain.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet("me")]
    public Task<IActionResult> GetMe() => ExecuteAsync(async () =>
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var user = await userService.GetByIdAsync(userId);
        return user is null ? NotFound() : Ok(user.ToDto());
    });

    [HttpPut("me")]
    public Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request) => ExecuteAsync(async () =>
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var user = await userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        var passwordHash = request.Password is null ? null : AuthService.HashPassword(request.Password);
        request.ApplyToDomain(user, passwordHash);
        await userService.UpdateAsync(user);

        return NoContent();
    });

    [HttpDelete("me")]
    public Task<IActionResult> DeleteMe() => ExecuteAsync(async () =>
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        await userService.DeleteAsync(userId);
        return NoContent();
    });
}