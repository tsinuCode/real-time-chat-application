using System.Security.Claims;
using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Users;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/directory")]
public class DirectoryController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public DirectoryController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>> GetUsers()
    {
        var users = await _userRepository.GetAllUsersAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>.Ok(MapToSummaries(users)));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>> SearchUsers(
        [FromQuery] string? query)
    {
        var users = string.IsNullOrWhiteSpace(query)
            ? await _userRepository.GetAllUsersAsync(CurrentUserId)
            : await _userRepository.SearchUsersAsync(query, CurrentUserId);

        return Ok(ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>.Ok(MapToSummaries(users)));
    }

    [HttpGet("user/{id}")]
    public async Task<ActionResult<ApiResponse<DirectoryUserDetailDto>>> GetUserById(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound(ApiResponse<DirectoryUserDetailDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<DirectoryUserDetailDto>.Ok(MapToDetail(user)));
    }

    private static IReadOnlyList<DirectoryUserSummaryDto> MapToSummaries(IReadOnlyList<UserDto> users) =>
        users.Select(u => new DirectoryUserSummaryDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email
        }).ToList();

    private static DirectoryUserDetailDto MapToDetail(UserDto user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            JoinedDate = user.CreatedAt
        };
}
