using Domain.Filters;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
public class AdminUsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAsync([FromQuery] UserFilter filter) => ExecuteAsync(async () =>
    {
        var users = await userService.GetAsync(filter);
        var totalCount = (await userService.GetAsync(new UserFilter
        {
            NameContains = filter.NameContains,
            Phone = filter.Phone,
            Role = filter.Role,
            Gender = filter.Gender
        })).Count;

        return OkWithTotalCount(users.Select(user => user.ToDto()).ToList(), totalCount);
    });

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateUserRequest request) => ExecuteAsync(async () =>
    {
        var user = await userService.CreateAsync(request.ToDomain(PasswordHasher.Hash(request.Password)));
        return Created($"/api/v1/admin/users/{user.Id}", user.ToDto());
    });

    [HttpGet("{userId:guid}")]
    public Task<IActionResult> GetById([FromRoute] Guid userId) => ExecuteAsync(async () =>
    {
        var user = await userService.GetByIdAsync(userId);
        return user is null ? NotFound() : Ok(user.ToDto());
    });

    [HttpPut("{userId:guid}")]
    public Task<IActionResult> Update([FromRoute] Guid userId, [FromBody] UpdateUserRequest request) => ExecuteAsync(async () =>
    {
        var user = await userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        var passwordHash = request.Password is null ? null : PasswordHasher.Hash(request.Password);
        request.ApplyToDomain(user, passwordHash);
        await userService.UpdateAsync(user);
        return NoContent();
    });

    [HttpDelete("{userId:guid}")]
    public Task<IActionResult> Delete([FromRoute] Guid userId) => ExecuteAsync(async () =>
    {
        await userService.DeleteAsync(userId);
        return NoContent();
    });
}