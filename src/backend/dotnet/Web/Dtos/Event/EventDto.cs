namespace Web.Dtos;

public class Event
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public Guid LocationId { get; set; }
    public int DaysCount { get; set; }
    public double Percent { get; set; }
    public Guid CreatedByUserId { get; set; }
    public double Rating { get; set; }
    public int PersonCount { get; set; }
}