using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateMenuRequest
{
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
}