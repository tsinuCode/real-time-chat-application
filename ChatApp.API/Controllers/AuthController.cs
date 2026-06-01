using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Auth;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(
                "Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var existing = await _userManager.FindByNameAsync(request.Username);
        if (existing is not null)
        {
            return Conflict(ApiResponse<AuthResponseDto>.Fail("Username is already taken."));
        }

        existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return Conflict(ApiResponse<AuthResponseDto>.Fail("Email is already registered."));
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(
                "Registration failed.",
                result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, "User");
        var (token, expiresAt) = _tokenService.GenerateToken(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Token = token,
            ExpiresAt = expiresAt
        }, "Registration successful."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation failed."));
        }

        var user = await _userManager.FindByNameAsync(request.UsernameOrEmail)
            ?? await _userManager.FindByEmailAsync(request.UsernameOrEmail);

        if (user is null)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid credentials."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid credentials."));
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Token = token,
            ExpiresAt = expiresAt
        }, "Login successful."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(ApiResponse<object>.Ok(new { }, "Logout successful."));
    }
}
