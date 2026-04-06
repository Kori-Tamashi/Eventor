namespace Application.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Eventor";
    public string Audience { get; set; } = "Eventor.Client";
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}