using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Models.DTO;
using API.Models.DTO.V1.Requests;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService authService;
    private readonly TokenService tokenService;
    private readonly UserManager<ApiUser> userManager;

    public AuthController(AuthService authService,
        UserManager<ApiUser> userManager,
        TokenService tokenService)
    {
        this.authService = authService;
        this.userManager = userManager;
        this.tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var (email, password) = registerRequest;
        var registerResult = await authService.Register(email, password);

        return registerResult switch
        {
            SuccessResult<RegisterResult> => Ok(),
            ErrorResult<RegisterResult> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException(nameof(registerResult))
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var (email, password) = loginRequest;
        var loginResult = await authService.Login(email, password);

        if (loginResult.Success)
        {
            SetRefreshToken(loginResult.Data.RefreshToken);
        }

        return loginResult switch
        {
            SuccessResult<LoginResult> success => Ok(new
            {
                accessToken = success.Data.AccessToken,
                expires = success.Data.Expires,
                user = success.Data.User
            }),
            ErrorResult<LoginResult> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException(nameof(loginResult))
        };
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Forbid("Refresh token was not provided");

        var token = tokenService.ValidateRefreshToken(refreshToken);
        if (token is null) return Forbid();

        var userEmail = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var user = await userManager.Users
            .Include(r => r.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Email == userEmail);

        if (user is null) return Unauthorized();

        var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);
        if (oldToken != null && !oldToken.IsActive) return Unauthorized();
        if (oldToken != null)
        {
            oldToken.Revoked = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken(user);
        SetRefreshToken(newRefreshToken.Token);

        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            user = new User(user.Email!),
            expires = token.ValidTo
        });
    }

    private void SetRefreshToken(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}