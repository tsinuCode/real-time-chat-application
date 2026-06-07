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
    private readonly IUserRepository _userRepository;

    public GroupsController(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupDto>>>> GetMyGroups()
    {
        var groups = await _groupRepository.GetUserGroupsAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<GroupDto>>.Ok(groups));
    }

    [HttpGet("{groupId:int}")]
    public async Task<ActionResult<ApiResponse<GroupDetailDto>>> GetGroupDetail(int groupId)
    {
        if (!await _groupRepository.IsMemberAsync(groupId, CurrentUserId))
        {
            return Forbid();
        }

        var group = await _groupRepository.GetGroupDetailAsync(groupId);
        if (group is null)
        {
            return NotFound(ApiResponse<GroupDetailDto>.Fail("Group not found."));
        }

        return Ok(ApiResponse<GroupDetailDto>.Ok(group));
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

    [HttpPost("{groupId:int}/members")]
    public async Task<ActionResult<ApiResponse<object>>> AddMember(int groupId, [FromBody] AddGroupMemberDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));
        }

        if (!await _groupRepository.IsMemberAsync(groupId, CurrentUserId))
        {
            return Forbid();
        }

        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group is null)
        {
            return NotFound(ApiResponse<object>.Fail("Group not found."));
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }

        if (await _groupRepository.IsMemberAsync(groupId, request.UserId))
        {
            return BadRequest(ApiResponse<object>.Fail("User is already a member of this group."));
        }

        await _groupRepository.AddMemberAsync(groupId, request.UserId);
        return Ok(ApiResponse<object>.Ok(new { }, "Member added successfully."));
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
