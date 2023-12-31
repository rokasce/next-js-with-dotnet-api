using API.Entities;
using API.Configurations;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService
{
    private readonly ILogger<TokenService> logger;
    private readonly JwtSettings jwtSettings;
    private readonly IDateTimeProvider dateTimeProvider;

    public TokenService(IOptions<JwtSettings> jwtSettings,
        ILogger<TokenService> logger,
        IDateTimeProvider dateTimeProvider)
    {
        this.jwtSettings = jwtSettings.Value;
        this.logger = logger;
        this.dateTimeProvider = dateTimeProvider;
    }

    public JwtSecurityToken GenerateAccessToken(ApiUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.JwtTokenSecret!));
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: dateTimeProvider.GetUtcDateTimeNow().AddMinutes(jwtSettings.TokenDurationInMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        );

        return token;
    }

    public RefreshToken GenerateRefreshToken(ApiUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.RefreshTokenSecret!));
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: dateTimeProvider.GetUtcDateTimeNow().AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new RefreshToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    public JwtSecurityToken? ValidateRefreshToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            // TODO: Issuer and Audience is trowing SecurityTokenInvalidIssuerException on ExternalLogin
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.RefreshTokenSecret)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }


            return jwtSecurityToken;
        }
        catch (SecurityTokenException exception)
        {
            logger.LogError("Refresh token validation failed: {Message}", exception.Message);
            logger.LogError("Failed validating refreshToken: {Token}", token);

            return null;
        }
    }
}
