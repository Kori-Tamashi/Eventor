using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateItemRequest
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public double Cost { get; set; }
}