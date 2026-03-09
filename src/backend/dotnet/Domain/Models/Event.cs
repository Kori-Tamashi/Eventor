namespace Domain.Models;

public class Event
{
    public Event() { }
    
    public Event(Guid id, 
        Guid locationId, 
        string name, 
        string description, 
        DateOnly date, 
        int daysCount, 
        double percent)
    {
        Id = id;
        Title = name;
        Description = description;
        StartDate = date;
        LocationId = locationId;
        DaysCount = daysCount;
        Percent = percent;
    }
    
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public Guid LocationId { get; set; }
    public int DaysCount { get; set; }
    public double Percent { get; set; }
}

