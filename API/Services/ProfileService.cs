using API.Entities;
using API.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

public class ProfileService
{
    private readonly UserManager<ApiUser> userManager;
    private readonly FileService fileService;

    public ProfileService(UserManager<ApiUser> userManager, FileService fileService)
    {
        this.userManager = userManager;
        this.fileService = fileService;
    }

    public async Task<Result<ProfileDto>> GetProfileAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new ErrorResult<ProfileDto>("User not found");
        }

        return new SuccessResult<ProfileDto>(
            new ProfileDto(user.UserName!, user.Bio, user.Avatar)
        );
    }

    public async Task<Result<ProfileDto>> UpdateProfileAsync(UpdateProfileDto updateProfileDto, string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new ErrorResult<ProfileDto>("User not found");
        }

        if (!string.IsNullOrEmpty(updateProfileDto.Username))
        {
            user.UserName = updateProfileDto.Username;
        }

        if (updateProfileDto.Avatar != null)
        {
            // TODO: Check if we don't need to map it
            var result = await fileService.UploadAsync(updateProfileDto.Avatar);
            user.Avatar = result.Blob.Uri!;
        }

        if (!string.IsNullOrEmpty(updateProfileDto.Bio))
        {
            user.Bio = updateProfileDto.Bio;
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new ErrorResult<ProfileDto>("Could not update profile");
        }

        return new SuccessResult<ProfileDto>(
            new ProfileDto(user.UserName!, user.Bio, user.Avatar)
        );
    }
}

public record UpdateProfileDto(string Username, string Bio, IFormFile Avatar);

public record ProfileDto(string Username, string Bio, string Avatar);