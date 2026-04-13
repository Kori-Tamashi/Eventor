using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Converters;
using Web.Dtos;

namespace Web.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] RegisterRequest request) => ExecuteAsync(async () =>
    {
        var user = await authService.RegisterAsync(request.Name, request.Phone, request.Gender.ToDomain(), request.Password);
        return Created("/api/v1/auth/register", user.ToDto());
    });

    [HttpPost("login")]
    public Task<IActionResult> Login([FromBody] LoginRequest request) => ExecuteAsync(async () =>
    {
        var token = await authService.LoginAsync(request.Phone, request.Password);
        return Ok(new { token });
    });
}