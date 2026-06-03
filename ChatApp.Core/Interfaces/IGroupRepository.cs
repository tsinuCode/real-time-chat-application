namespace ChatApp.Core.Interfaces;

public interface IGroupRepository
{
    Task<bool> IsMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
}
