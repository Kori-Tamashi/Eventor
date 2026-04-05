namespace Web.Dtos;

public class Item
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public double Cost { get; set; }
}