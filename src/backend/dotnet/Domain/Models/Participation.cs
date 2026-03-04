namespace Eventor.Domain.Models;

public class Participation
{
    public Participation() { }

    public Participation(Guid dayId, Guid registrationId)
    {
        DayId = dayId;
        RegistrationId = registrationId;
    }
    
    public Guid DayId { get; set; }
    public Guid RegistrationId { get; set; }
}