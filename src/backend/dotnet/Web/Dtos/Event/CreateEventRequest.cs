using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateEventRequest
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public int DaysCount { get; set; }

    [Required]
    public double Percent { get; set; }

    [Required]
    public Guid CreatedByUserId { get; set; }
}