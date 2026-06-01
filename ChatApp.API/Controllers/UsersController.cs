using System.Security.Claims;
using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Users;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetUsers()
    {
        var users = await _userRepository.GetAllUsersAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    [HttpGet("online")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetOnlineUsers()
    {
        var users = await _userRepository.GetAllUsersAsync(CurrentUserId);
        var online = users.Where(u => u.IsOnline).ToList();
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(online));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var user = await _userRepository.GetByIdAsync(CurrentUserId);
        if (user is null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }
}
