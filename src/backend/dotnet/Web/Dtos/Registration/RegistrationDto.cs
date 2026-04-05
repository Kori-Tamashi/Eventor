namespace Web.Dtos;

public class Registration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public RegistrationType Type { get; set; }
    public bool Payment { get; set; }
    public List<Day> Days { get; set; } = [];
}