using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IGroupRepository
{
    Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default);
    Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);
}
