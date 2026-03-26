namespace Web.Dtos;

public class Menu
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<MenuItem> MenuItems { get; set; } = [];
}