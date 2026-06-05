namespace ChatApp.Core.Interfaces;

public interface IUserRepository
{
    Task SetOnlineStatusAsync(string userId, bool isOnline, CancellationToken cancellationToken = default);
}
