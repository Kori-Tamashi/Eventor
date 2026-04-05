using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class UpdateFeedbackRequest
{
    public string? Comment { get; set; }

    [Range(1, 5)]
    public int? Rate { get; set; }
}