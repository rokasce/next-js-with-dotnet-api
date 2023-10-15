using System.ComponentModel.DataAnnotations;

namespace API.Models.DTO.V1.Requests;

public record RegisterRequest(
    [Required(ErrorMessage = "Email is required")] string Email,
    [Required(ErrorMessage = "Password is required")] string Password);

public record LoginRequest(
    [Required(ErrorMessage = "Email is required")] string Email,
    [Required(ErrorMessage = "Password is required")] string Password);

public record ChangePasswordRequest(
    [Required] string CurrentPassword, [Required] string NewPassword);
