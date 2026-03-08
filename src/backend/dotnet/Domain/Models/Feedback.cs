namespace Eventor.Domain.Models;

public class Feedback
{
    public Feedback() { }
    
    public Feedback(Guid id, 
        Guid registationId, 
        string comment, 
        int rate)
    {
        Id = id;
        Comment = comment;
        Rate = rate;
        RegistrationId = registationId;
    }
    
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string Comment { get; set; }
    public int Rate { get; set; }
}
 