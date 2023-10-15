namespace API.Configurations;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string DisplayName { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;

    public EmailServiceType Type { get; set; } = EmailServiceType.Logging;
}

public enum EmailServiceType
{
    Logging,
    Smtp
}

