namespace Web.Dtos;

public class UpdateRegistrationRequest
{
    public RegistrationType? Type { get; set; }
    public bool? Payment { get; set; }
    public List<Guid>? DayIds { get; set; }
}