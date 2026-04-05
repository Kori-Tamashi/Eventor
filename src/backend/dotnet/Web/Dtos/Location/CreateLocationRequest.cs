using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateLocationRequest
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public double Cost { get; set; }

    [Required]
    public int Capacity { get; set; }
}