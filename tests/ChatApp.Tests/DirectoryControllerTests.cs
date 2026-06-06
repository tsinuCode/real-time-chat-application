using System.Security.Claims;
using ChatApp.API.Controllers;
using ChatApp.Core.DTOs.Users;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ChatApp.Tests;

public class DirectoryControllerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly DirectoryController _controller;

    public DirectoryControllerTests()
    {
        _controller = new DirectoryController(_userRepository.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "current-user-id")],
                    "TestAuth"))
            }
        };
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var users = new List<UserDto>
        {
            new() { Id = "user-1", Username = "abdi", Email = "abdi@test.com", CreatedAt = DateTime.UtcNow },
            new() { Id = "user-2", Username = "kaleab", Email = "kaleab@test.com", CreatedAt = DateTime.UtcNow }
        };

        _userRepository
            .Setup(r => r.GetAllUsersAsync("current-user-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChatApp.Core.Common.ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
        Assert.Equal("abdi", response.Data[0].Username);
    }

    [Fact]
    public async Task SearchUsers_ReturnsMatchingUsers()
    {
        var users = new List<UserDto>
        {
            new() { Id = "user-1", Username = "abdi", Email = "abdi@test.com", CreatedAt = DateTime.UtcNow }
        };

        _userRepository
            .Setup(r => r.SearchUsersAsync("ab", "current-user-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _controller.SearchUsers("ab");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChatApp.Core.Common.ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
        Assert.Equal("abdi@test.com", response.Data[0].Email);
    }

    [Fact]
    public async Task GetUserById_ReturnsUserDetail()
    {
        var user = new UserDto
        {
            Id = "user-1",
            Username = "abdi",
            Email = "abdi@test.com",
            CreatedAt = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        _userRepository
            .Setup(r => r.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.GetUserById("user-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChatApp.Core.Common.ApiResponse<DirectoryUserDetailDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("abdi", response.Data!.Username);
        Assert.Equal(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), response.Data.JoinedDate);
    }

    [Fact]
    public async Task GetUserById_InvalidId_ReturnsNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        var result = await _controller.GetUserById("invalid-id");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ChatApp.Core.Common.ApiResponse<DirectoryUserDetailDto>>(notFoundResult.Value);
        Assert.False(response.Success);
    }
}
