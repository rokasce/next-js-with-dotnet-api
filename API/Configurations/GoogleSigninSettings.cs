using System.ComponentModel.DataAnnotations;

namespace API.Configurations;

public class GoogleSigninSettings
{
    public const string SectionName = "GoogleSignIn";

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;
}