using API.Services;
using API.Models.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Routes;

namespace API.Controllers;

[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ProfileService profileService;

    public ProfileController(ProfileService profileService)
    {
        this.profileService = profileService;
    }

    [HttpGet(AppRoutes.Profile.Me)]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await profileService.GetProfileAsync(userId);
        return result switch
        {
            SuccessResult<ProfileDto> dto => Ok(dto.Data),
            ErrorResult<ProfileDto> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPut(AppRoutes.Profile.Update)]
    public async Task<IActionResult> UpdateAsync([FromForm] UpdateProfileDto updateProfileDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await profileService.UpdateProfileAsync(updateProfileDto, userId);
        return result switch
        {
            SuccessResult<ProfileDto> data => Ok(data.Data),
            ErrorResult<ProfileDto> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }
}
