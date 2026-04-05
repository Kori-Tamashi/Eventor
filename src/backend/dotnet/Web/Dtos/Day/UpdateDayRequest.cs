namespace Web.Dtos;

public class UpdateDayRequest
{
    public Guid? MenuId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? SequenceNumber { get; set; }
}