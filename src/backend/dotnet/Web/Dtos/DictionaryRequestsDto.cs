using System.ComponentModel.DataAnnotations;

namespace Web.Dtos;

public class CreateLocationRequest
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public double Cost { get; set; }

    [Required]
    public int Capacity { get; set; }
}

public class UpdateLocationRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public double? Cost { get; set; }
    public int? Capacity { get; set; }
}

public class CreateItemRequest
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public double Cost { get; set; }
}

public class UpdateItemRequest
{
    public string? Title { get; set; }
    public double? Cost { get; set; }
}

public class CreateMenuRequest
{
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
}

public class UpdateMenuRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}