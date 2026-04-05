namespace Web.Dtos;

public class UpdateLocationRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public double? Cost { get; set; }
    public int? Capacity { get; set; }
}