using API.Models.DTO;
using API.Models.DTO.V1.Requests;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService authService;

    public AuthController(AuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var (email, password) = registerRequest;
        var registerResult = await authService.Register(email, password);

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
        var loginResult = await authService.Login(email, password);

        return loginResult switch
        {
            SuccessResult<LoginResult> success => Ok(success.Data),
            ErrorResult<LoginResult> error => BadRequest(error.Errors),
            _ => throw new ArgumentOutOfRangeException(nameof(loginResult))
        };
    }
}