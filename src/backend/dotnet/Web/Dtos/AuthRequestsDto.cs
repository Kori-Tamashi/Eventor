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

public class LoginRequest
{
    [Required]
    public required string Phone { get; set; }

    [Required]
    public required string Password { get; set; }
}

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

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public Gender? Gender { get; set; }
    public UserRole? Role { get; set; }
    public string? Password { get; set; }
}