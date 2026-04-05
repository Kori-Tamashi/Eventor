using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateRegistrationRequest
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public RegistrationType Type { get; set; }

    public bool Payment { get; set; }

    [Required]
    public required List<Guid> DayIds { get; set; }
}