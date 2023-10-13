using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Configurations;
using API.Entities;
using API.Models.DTO;
using API.Models.DTO.V1.Requests;
using API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService authService;
    private readonly TokenService tokenService;
    private readonly PasswordService passwordService;
    private readonly UserManager<ApiUser> userManager;
    private readonly WebAppSettings webAppSettings;

    public AuthController(AuthService authService,
        UserManager<ApiUser> userManager,
        PasswordService passwordService,
        TokenService tokenService,
        IOptions<WebAppSettings> webAppSettings)
    {
        this.authService = authService;
        this.userManager = userManager;
        this.passwordService = passwordService;
        this.tokenService = tokenService;
        this.webAppSettings = webAppSettings.Value;
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
        // TODO: Check if Third party token is being invalidated after login
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        Response.Cookies.Delete("refreshToken");

        return Redirect($"{webAppSettings.BaseUrl}");
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
        // TODO: Signing in with Google account will override account password of the same email.
        // Should we separate these accounts or merge it together?

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

                return Redirect($"{webAppSettings.BaseUrl}/home");
            }
        }

        return Redirect($"{webAppSettings.BaseUrl}");
    }

    // TODO: Move to auth models
    public record ChangePasswordRequest([Required] string OldPassword, [Required] string NewPassword);

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName)) 
            return Unauthorized();

        var (oldPassword, newPassword) = request;

        var result = await passwordService.ChangePasswordAsync(oldPassword, newPassword, userId);

        if (result.Success) 
        {
            var loginResult = await authService.LoginAsync(userName, newPassword); 
            if (loginResult.Success)
            {
                SetRefreshToken(loginResult.Data.RefreshToken);

                return Ok();
            }

            return BadRequest("Password got changed, but user could not be signed in");
        }

        return result switch
        {
            SuccessResult<bool> => Ok(),
            ErrorResult<bool> error => BadRequest(error.Errors),
            _ => throw new NotImplementedException(),
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
