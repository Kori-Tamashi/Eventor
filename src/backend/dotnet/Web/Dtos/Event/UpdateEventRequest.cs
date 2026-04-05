namespace Web.Dtos;

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public Guid? LocationId { get; set; }
    public int? DaysCount { get; set; }
    public double? Percent { get; set; }
}