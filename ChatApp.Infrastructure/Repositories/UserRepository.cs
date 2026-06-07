using ChatApp.Core.DTOs.Users;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChatAppDbContext _context;

    public UserRepository(ChatAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(
        string? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(u => u.Id != excludeUserId);
        }

        return await query
            .OrderBy(u => u.UserName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsOnline = u.IsOnline,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsOnline = u.IsOnline,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> SearchUsersAsync(
        string query, string? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = query.Trim().ToLower();

        var usersQuery = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            usersQuery = usersQuery.Where(u => u.Id != excludeUserId);
        }

        if (!string.IsNullOrEmpty(normalized))
        {
            usersQuery = usersQuery.Where(u =>
                (u.UserName ?? string.Empty).ToLower().Contains(normalized) ||
                (u.Email ?? string.Empty).ToLower().Contains(normalized));
        }

        return await usersQuery
            .OrderBy(u => u.UserName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsOnline = u.IsOnline,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SetOnlineStatusAsync(
        string userId, bool isOnline, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return;
        }

        user.IsOnline = isOnline;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
