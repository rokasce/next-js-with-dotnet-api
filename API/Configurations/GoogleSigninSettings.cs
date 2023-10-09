namespace API.Configurations;

public class GoogleSigninSettings
{
    public const string SectionName = "GoogleSignIn";

    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}