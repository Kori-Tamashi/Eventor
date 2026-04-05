namespace Web.Dtos;

public class Location
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public double Cost { get; set; }
    public int Capacity { get; set; }
}