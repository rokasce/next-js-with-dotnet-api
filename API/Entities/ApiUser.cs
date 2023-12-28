using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class ApiUser : IdentityUser
{
    public string Bio { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}