using API.Configurations;
using Microsoft.Extensions.Options;

namespace API.Services;

public class TokenService
{
    private readonly JwtSettings jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        this.jwtSettings = jwtSettings.Value;
    }
}