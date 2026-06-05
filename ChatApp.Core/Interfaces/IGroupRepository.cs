using ChatApp.Core.DTOs.Groups;
using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IGroupRepository
{
    Task<ChatGroup> CreateAsync(ChatGroup group, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ChatGroup?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
}
