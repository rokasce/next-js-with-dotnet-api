using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using API.Entities;
using API.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

// TODO: This is to complicated, please do some refactoring
public class AuthService
{
    private readonly UserManager<ApiUser> userManager;
    private readonly TokenService tokenService;

    public AuthService(UserManager<ApiUser> userManager, TokenService tokenService)
    {
        this.userManager = userManager;
        this.userManager = userManager;
        this.tokenService = tokenService;
    }

    public async Task<Result<RegisterResult>> RegisterAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
            return new ErrorResult<RegisterResult>("User already exists");

        var newUser = new ApiUser
        {
            Email = email,
            UserName = email
        };

        try
        {
            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
                return new SuccessResult<RegisterResult>(new RegisterResult(newUser));

            return new ErrorResult<RegisterResult>(
                "Failed to create user",
                result.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
        }
        catch (Exception e)
        {
            return new ErrorResult<RegisterResult>(e.Message);
        }
    }

    public async Task<Result<LoginResult>> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return new ErrorResult<LoginResult>("Invalid login attempt");

        var isLoginSuccessful = await userManager.CheckPasswordAsync(user, password);
        if (!isLoginSuccessful)
        {
            return new ErrorResult<LoginResult>("Invalid login attempt");
        }

        var refreshToken = tokenService.GenerateRefreshToken(user);
        try
        {
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);
        }
        catch (Exception e)
        {
            return new ErrorResult<LoginResult>(
                "Failed persisting refresh token", new[] { new Error("RefreshTokenPersistFail", e.Message) }
            );
        }

        try
        {
            var token = tokenService.GenerateAccessToken(user);
            var writtenToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new SuccessResult<LoginResult>(
                new LoginResult(writtenToken, token.ValidTo, refreshToken.Token, new User(user.Email!)));
        }
        catch (Exception e)
        {
            var errors = new[] { new Error("TokenGenerationFail", e.Message) };
            return new ErrorResult<LoginResult>("Failed generating AccessToken", errors);
        }
    }

    public async Task<Result<LoginResult>> CreateExternalUserAsync(string provider, ExternalUserInfo userInfo)
    {
        var user = await userManager.FindByLoginAsync(provider, userInfo.ProviderKey);

        var result = IdentityResult.Success;

        if (user is null)
        {
            user = new ApiUser()
            {
                UserName = userInfo.Email.Split('@')[0],
                Email = userInfo.Email,
            };

            result = await userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                // TODO: Whats the displayName
                result = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, userInfo.ProviderKey, displayName: null));
            }
        }

        if (result.Succeeded)
        {
            var refreshToken = tokenService.GenerateRefreshToken(user);
            try
            {
                user.RefreshTokens.Add(refreshToken);
                await userManager.UpdateAsync(user);
            }
            catch (Exception e)
            {
                return new ErrorResult<LoginResult>(
                    "Failed persisting refresh token", new[] { new Error("RefreshTokenPersistFail", e.Message) }
                );
            }

            try
            {
                var accessToken = tokenService.GenerateAccessToken(user);
                var writtenToken = new JwtSecurityTokenHandler()
                    .WriteToken(accessToken);


                return new SuccessResult<LoginResult>(
                    new LoginResult(writtenToken, accessToken.ValidTo, refreshToken.Token, new User(user.Email!)));
            }
            catch (Exception e)
            {
                var errors = new[] { new Error("TokenGenerationFail", e.Message) };
                return new ErrorResult<LoginResult>("Failed generating AccessToken", errors);
            }
        }


        var error = new[] {
            new Error("External login failed", result.Errors.FirstOrDefault()?.Description ?? "Unknown")
        };
        return new ErrorResult<LoginResult>("ExternalLoginFailed", error);
    }
}

public record RegisterResult(ApiUser User);

public record LoginResult(string AccessToken, DateTime Expires, string RefreshToken, User User);

public record ExternalUserInfo([Required] string Email, [Required] string ProviderKey);
