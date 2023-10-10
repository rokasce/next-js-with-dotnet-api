using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Models.DTO;
using API.Models.DTO.V1.Requests;
using API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("[controller]")]
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
        var registerResult = await authService.RegisterAsync(email, password);

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
        var loginResult = await authService.LoginAsync(email, password);

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

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        Response.Cookies.Delete("refreshToken");

        return Redirect("https://localhost:3000");
    }

    [HttpPost]
    [Route("login/{provider}")]
    public IActionResult ExternalLogin(string provider)
    {
        return Challenge(
            authenticationSchemes: new string[] { provider },
            properties: new AuthenticationProperties() { RedirectUri = $"/auth/signing/${provider}" });
    }

    [HttpGet]
    [Route("signing/{provider}")]
    public async Task<IActionResult> ExternalLoginComplete(string provider)
    {
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        if (result.Succeeded)
        {
            var principal = result.Principal;
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var email = (principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity?.Name)!;

            var createExternalUserResult = await authService.CreateExternalUserAsync(provider, new ExternalUserInfo(email, id!));

            if (createExternalUserResult.Success)
            {
                SetRefreshToken(createExternalUserResult.Data.RefreshToken);

                return Redirect("https://localhost:3000/home");
            }
        }

        return Redirect("https://localhost:3000/");
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
