using ChatApp.Core.DTOs.Messages;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ChatAppDbContext _context;

    public MessageRepository(ChatAppDbContext context)
    {
        _context = context;
    }

    public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<IReadOnlyList<MessageDto>> GetPrivateHistoryAsync(
        string userId, string otherUserId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.GroupId == null &&
                ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                 (m.SenderId == otherUserId && m.ReceiverId == userId)))
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.UserName ?? string.Empty,
                ReceiverId = m.ReceiverId,
                GroupId = m.GroupId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsSeen = m.IsSeen
            })
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MessageDto>> GetGroupHistoryAsync(
        int groupId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.GroupId == groupId)
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.UserName ?? string.Empty,
                ReceiverId = m.ReceiverId,
                GroupId = m.GroupId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsSeen = m.IsSeen
            })
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationSummariesAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var privateConversations = await _context.Messages
            .Where(m => m.GroupId == null && (m.SenderId == userId || m.ReceiverId == userId))
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId! : m.SenderId)
            .Select(g => new ConversationSummaryDto
            {
                ConversationType = "private",
                ConversationId = g.Key,
                Title = _context.Users.Where(u => u.Id == g.Key).Select(u => u.UserName!).FirstOrDefault() ?? "User",
                LastMessagePreview = g.OrderByDescending(m => m.SentAt).Select(m => m.Content).FirstOrDefault() ?? "No messages yet",
                LastMessageAt = g.Max(m => m.SentAt),
                UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsSeen),
                IsOnline = _context.Users.Where(u => u.Id == g.Key).Select(u => u.IsOnline).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var groupConversations = await _context.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => new ConversationSummaryDto
            {
                ConversationType = "group",
                ConversationId = gm.GroupId.ToString(),
                Title = gm.Group.GroupName,
                LastMessagePreview = _context.Messages
                    .Where(m => m.GroupId == gm.GroupId)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "No messages yet",
                LastMessageAt = _context.Messages
                    .Where(m => m.GroupId == gm.GroupId)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault(),
                UnreadCount = 0,
                IsOnline = false
            })
            .ToListAsync(cancellationToken);

        return privateConversations
            .Concat(groupConversations)
            .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
            .ToList();
    }

    public async Task<int> GetPrivateUnreadCountAsync(
        string userId, string senderId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .CountAsync(m => m.GroupId == null && m.SenderId == senderId && m.ReceiverId == userId && !m.IsSeen, cancellationToken);
    }

    public async Task MarkPrivateMessagesAsSeenAsync(
        string userId, string senderId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => m.GroupId == null && m.SenderId == senderId && m.ReceiverId == userId && !m.IsSeen)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.IsSeen = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
