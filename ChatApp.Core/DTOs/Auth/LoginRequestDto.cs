using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
