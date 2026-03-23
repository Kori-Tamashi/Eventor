namespace Domain.Models;

public class Location
{
    public Location() { }
    
    public Location(Guid id, 
        string title, 
        string description, 
        decimal cost, 
        int capacity)
    {
        Id = id;
        Title = title;
        Description = description;
        Cost = cost;
        Capacity = capacity;
    }
    
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Cost { get; set; }
    public int Capacity { get; set; }
}
