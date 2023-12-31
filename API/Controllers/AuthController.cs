using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Configurations;
using API.Entities;
using API.Models.DTO;
using API.Models.DTO.V1.Requests;
using API.Routes;
using API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Controllers;

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

    [HttpPost(AppRoutes.Authentication.Register)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var (email, password) = registerRequest;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var registerResult = await authService.RegisterAsync(email, password, baseUrl);

        return registerResult switch
        {
            SuccessResult<RegisterResult> => Ok(),
            ErrorResult<RegisterResult> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException(nameof(registerResult))
        };
    }

    [HttpPost(AppRoutes.Authentication.Login)]
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

    [HttpGet(AppRoutes.Authentication.ConfirmEmail)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request)
    {
        var (email, token) = request;

        if (token is null || email is null) return Unauthorized();

        var result = await authService.ValidateEmailConfirmationToken(email, token);

        return result switch
        {
            SuccessResult<bool> _ => Ok(),
            ErrorResult<bool> error => BadRequest(error),
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }


    [HttpPost(AppRoutes.Authentication.Logout)]
    public async Task<IActionResult> Logout()
    {
        // TODO: Check if Third party token is being invalidated after login
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        Response.Cookies.Delete("refreshToken");

        return Redirect($"{webAppSettings.BaseUrl}");
    }

    [HttpPost]
    [Route(AppRoutes.Authentication.ExternalLogin)]
    public IActionResult ExternalLogin(string provider)
    {
        return Challenge(
            authenticationSchemes: new string[] { provider },
            properties: new AuthenticationProperties() { RedirectUri = $"/auth/signing/${provider}" });
    }

    [HttpGet]
    [Route(AppRoutes.Authentication.SignInExternal)]
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

        var redirectUrl = $"{webAppSettings.BaseUrl}/login?error=external_login_failure";

        return Redirect(redirectUrl);
    }

    [Authorize]
    [HttpPost(AppRoutes.Authentication.ChangePassword)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            return Unauthorized();

        var (oldPassword, newPassword) = request;

        var result = await passwordService.ChangePasswordAsync(oldPassword, newPassword, userId);

        return result switch
        {
            SuccessResult<bool> => Ok(),
            ErrorResult<bool> error => BadRequest(error.Errors),
            _ => throw new NotImplementedException(),
        };
    }

    [HttpPost(AppRoutes.Authentication.RefreshToken)]
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
            user = new User(user.Email!, user.Avatar),
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

