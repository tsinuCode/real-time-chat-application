using ChatApp.Core.DTOs.Users;

namespace ChatApp.Core.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(
        string? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> SearchUsersAsync(
        string query, string? excludeUserId = null, CancellationToken cancellationToken = default);

    Task SetOnlineStatusAsync(string userId, bool isOnline, CancellationToken cancellationToken = default);
}
