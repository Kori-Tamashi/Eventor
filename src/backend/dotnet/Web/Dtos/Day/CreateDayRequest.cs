using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateDayRequest
{
    [Required]
    public Guid MenuId { get; set; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public int SequenceNumber { get; set; }
}