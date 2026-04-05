using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class RegisterRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Phone { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}