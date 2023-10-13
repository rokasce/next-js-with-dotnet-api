using API.Entities;
using API.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

public class PasswordService
{
    private readonly UserManager<ApiUser> userManager;

    public PasswordService(UserManager<ApiUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<Result<bool>> ChangePasswordAsync(string oldPassword, string newPassword, string userId)
    {
        var currentUser = await userManager.FindByIdAsync(userId);
        if (currentUser is null)
            return new ErrorResult<bool>("Could not find user with provided ID");

        try
        {
            var result = await userManager.ChangePasswordAsync(currentUser, oldPassword, newPassword);
            if (result.Succeeded)
            {
                return new SuccessResult<bool>(true);
            }

            return new ErrorResult<bool>(result.Errors?.First()?.Description ?? "Change password failed");
        }
        catch (Exception ex)
        {
            return new ErrorResult<bool>(ex.Message);
        }
    }
}
