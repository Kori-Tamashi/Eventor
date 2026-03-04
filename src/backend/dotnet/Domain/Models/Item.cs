namespace Eventor.Domain.Models;

public class Item
{
    public Item() { }
    
    public Item(Guid id, string title, double cost)
    {
        Id = id;
        Title = title;
        Cost = cost;
    }
    
    public Guid Id { get; set; }
    public string Title { get; set; }
    public double Cost { get; set; }
}
