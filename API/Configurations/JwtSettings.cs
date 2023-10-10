using System.ComponentModel.DataAnnotations;

namespace API.Configurations;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Audience { get; set; } = string.Empty;
    
    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string JwtTokenSecret { get; set; } = string.Empty;

    public int TokenDurationInMinutes { get; set; } = 30;

    [Required]
    public string RefreshTokenSecret { get; set; } = string.Empty;
}