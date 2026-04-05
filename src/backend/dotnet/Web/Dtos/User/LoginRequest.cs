using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class LoginRequest
{
    [Required]
    public required string Phone { get; set; }

    [Required]
    public required string Password { get; set; }
}