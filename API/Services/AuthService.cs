using API.Entities;
using API.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace API.Services;
public class AuthService
{
    private readonly UserManager<ApiUser> userManager;

    public AuthService(UserManager<ApiUser> userManager)
    {
        this.userManager = userManager;
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
}

public record RegisterResult(ApiUser User);