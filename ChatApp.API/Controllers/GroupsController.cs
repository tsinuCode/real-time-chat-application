using System.Security.Claims;
using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Groups;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupRepository _groupRepository;

    public GroupsController(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupDto>>>> GetMyGroups()
    {
        var groups = await _groupRepository.GetUserGroupsAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<GroupDto>>.Ok(groups));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupDto>>> CreateGroup([FromBody] CreateGroupDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<GroupDto>.Fail("Validation failed."));
        }

        var group = new ChatGroup
        {
            GroupName = request.GroupName,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            Members = new List<GroupMember>
            {
                new() { UserId = CurrentUserId }
            }
        };

        var created = await _groupRepository.CreateAsync(group);

        return CreatedAtAction(nameof(GetMyGroups), ApiResponse<GroupDto>.Ok(new GroupDto
        {
            Id = created.Id,
            GroupName = created.GroupName,
            CreatedBy = created.CreatedBy,
            CreatedAt = created.CreatedAt,
            MemberCount = 1
        }));
    }

    [HttpPost("{groupId:int}/join")]
    public async Task<ActionResult<ApiResponse<object>>> JoinGroup(int groupId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group is null)
        {
            return NotFound(ApiResponse<object>.Fail("Group not found."));
        }

        await _groupRepository.AddMemberAsync(groupId, CurrentUserId);
        return Ok(ApiResponse<object>.Ok(new { }, "Joined group successfully."));
    }

    [HttpPost("{groupId:int}/leave")]
    public async Task<ActionResult<ApiResponse<object>>> LeaveGroup(int groupId)
    {
        await _groupRepository.RemoveMemberAsync(groupId, CurrentUserId);
        return Ok(ApiResponse<object>.Ok(new { }, "Left group successfully."));
    }
}
