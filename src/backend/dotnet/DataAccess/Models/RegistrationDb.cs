using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace DataAccess.Models;

public class RegistrationDb
{
    public RegistrationDb(Guid id, Guid eventId, Guid userId, RegistrationType type, bool paid)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Type = type;
        Paid = paid;
    }
    
    [Key]
    [Column("registration_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Column("event_id", TypeName = "uuid")]
    public Guid EventId { get; set; }

    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    [Column("type", TypeName = "registration_type_enum")]
    public RegistrationType Type { get; set; }

    [Column("payment", TypeName = "boolean")]
    public bool Paid { get; set; }
    
    public EventDb Event { get; set; } = null!;
    
    public UserDb User { get; set; } = null!;
}