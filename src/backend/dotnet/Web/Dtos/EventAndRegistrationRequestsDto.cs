using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateEventRequest
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public int DaysCount { get; set; }

    [Required]
    public double Percent { get; set; }

    [Required]
    public Guid CreatedByUserId { get; set; }
}

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public Guid? LocationId { get; set; }
    public int? DaysCount { get; set; }
    public double? Percent { get; set; }
}

public class CreateDayRequest
{
    [Required]
    public Guid MenuId { get; set; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public int SequenceNumber { get; set; }
}

public class UpdateDayRequest
{
    public Guid? MenuId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? SequenceNumber { get; set; }
}

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

public class UpdateRegistrationRequest
{
    public RegistrationType? Type { get; set; }
    public bool? Payment { get; set; }
    public List<Guid>? DayIds { get; set; }
}

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

public class UpdateFeedbackRequest
{
    public string? Comment { get; set; }

    [Range(1, 5)]
    public int? Rate { get; set; }
}