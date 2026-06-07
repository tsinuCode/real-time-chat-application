using ChatApp.Core.DTOs.Groups;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly ChatAppDbContext _context;

    public GroupRepository(ChatAppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatGroup> CreateAsync(ChatGroup group, CancellationToken cancellationToken = default)
    {
        _context.ChatGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.UserId == userId)
            .Select(gm => new GroupDto
            {
                Id = gm.Group.Id,
                GroupName = gm.Group.GroupName,
                CreatedBy = gm.Group.CreatedBy,
                CreatorUsername = gm.Group.Creator.UserName ?? string.Empty,
                CreatedAt = gm.Group.CreatedAt,
                MemberCount = gm.Group.Members.Count
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatGroup?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

    public async Task<GroupDetailDto?> GetGroupDetailAsync(
        int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => new GroupDetailDto
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedBy = g.CreatedBy,
                CreatorUsername = g.Creator!.UserName ?? string.Empty,
                CreatedAt = g.CreatedAt,
                Members = g.Members
                    .OrderBy(m => m.User!.UserName)
                    .Select(m => new GroupMemberDto
                    {
                        UserId = m.UserId,
                        Username = m.User!.UserName ?? string.Empty,
                        Email = m.User!.Email ?? string.Empty
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId, cancellationToken);
    }

    public async Task AddMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default)
    {
        if (await IsMemberAsync(groupId, userId, cancellationToken))
        {
            return;
        }

        _context.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId, cancellationToken);

        if (member is null)
        {
            return;
        }

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
