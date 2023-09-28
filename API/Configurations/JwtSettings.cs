namespace API.Configurations;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string JwtTokenSecret { get; set; }
    public int TokenDurationInMinutes { get; set; } = 30;
    public string RefreshTokenSecret { get; set; }
}