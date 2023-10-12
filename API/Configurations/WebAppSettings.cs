using System.ComponentModel.DataAnnotations;

namespace API.Configurations;

public class WebAppSettings
{
    public const string SectionName = "WebApp";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}
