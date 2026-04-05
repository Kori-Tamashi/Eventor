using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateFeedbackRequest
{
    [Required]
    public Guid RegistrationId { get; set; }

    [Required]
    public required string Comment { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rate { get; set; }
}