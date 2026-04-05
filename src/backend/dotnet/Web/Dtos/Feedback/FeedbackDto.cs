using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public required string Comment { get; set; }

    [Range(1, 5)]
    public int Rate { get; set; }
}