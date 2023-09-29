using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class ApiUser : IdentityUser
{
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}