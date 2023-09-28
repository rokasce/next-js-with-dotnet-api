using System.IdentityModel.Tokens.Jwt;
using API.Entities;
using API.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

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

    public async Task<Result<RegisterResult>> Register(string email, string password)
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

    public async Task<Result<LoginResult>> Login(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return new ErrorResult<LoginResult>("Invalid login attempt");

        var isPasswordCorrect = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordCorrect)
        {
            return new ErrorResult<LoginResult>("Invalid login attempt");
        }

        try
        {
            var accessToken = tokenService.GenerateAccessToken(user);
            var writtenToken = new JwtSecurityTokenHandler()
                .WriteToken(accessToken);

            return new SuccessResult<LoginResult>(new LoginResult(writtenToken, accessToken.ValidTo, new User(user.Email!)));
        }
        catch (Exception e)
        {
            var errors = new[] { new Error("TokenGenerationFail", e.Message) };
            return new ErrorResult<LoginResult>("Failed generating AccessToken", errors);
        }
    }
}

public record RegisterResult(ApiUser User);

public record LoginResult(string AccessToken, DateTime Expires, User User);
