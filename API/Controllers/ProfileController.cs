using System.Security.Claims;
using API.Models.DTO;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[Route("[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService profileService;

    public ProfileController(ProfileService profileService)
    {
        this.profileService = profileService;
    }

    [HttpGet("me")]
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

    [HttpPut("update")]
    public async Task<IActionResult> UpdateAsync([FromForm]UpdateProfileDto updateProfileDto)
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
