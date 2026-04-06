using System.ComponentModel.DataAnnotations;

namespace Application.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "Eventor";

    [Required]
    public string Audience { get; set; } = "Eventor.Client";

    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}