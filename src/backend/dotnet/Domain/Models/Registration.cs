using Domain.Enums;

namespace Eventor.Domain.Models;

public class Registration
{
    public Registration() { }

    public Registration(Guid id, 
        Guid eventId, 
        Guid userId, 
        RegistrationType type, 
        bool payment)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Type = type;
        Payment = payment;
    }
    
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public RegistrationType Type { get; set; }
    public bool Payment { get; set; }
    
    public List<Day> Days { get; set; } = [];
} 