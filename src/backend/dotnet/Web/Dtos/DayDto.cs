namespace Web.Dtos;

public class Day
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid MenuId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int SequenceNumber { get; set; }
}