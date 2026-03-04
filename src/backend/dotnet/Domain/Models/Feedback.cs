namespace Eventor.Domain.Models;

public class Feedback
{
    public Feedback() { }
    
    public Feedback(Guid id, Guid registationId, string comment, double rate)
    {
        Id = id;
        Comment = comment;
        Rate = rate;
        RegistationId = registationId;
    }
    
    public Guid Id { get; set; }
    public Guid RegistationId { get; set; }
    public string Comment { get; set; }
    public double Rate { get; set; }
}
 