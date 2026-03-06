namespace Eventor.Domain.Models;

public class Day
{
    public Day() { }

    public Day(Guid id, 
        Guid eventId, 
        Guid menuId, 
        string title, 
        int sequenceNumber, 
        string description)
    {
        Id = id;
        EventId = eventId;
        MenuId = menuId;
        Title = title;
        SequenceNumber = sequenceNumber;
        Description = description;
    }
    
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid MenuId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int SequenceNumber { get; set; }
}

