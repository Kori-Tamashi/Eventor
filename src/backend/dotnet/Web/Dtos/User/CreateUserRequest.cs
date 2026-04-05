using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateUserRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Phone { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public UserRole Role { get; set; }

    [Required]
    public required string Password { get; set; }
}