using ChatApp.Core.DTOs.Messages;
using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageDto>> GetPrivateHistoryAsync(string userId, string otherUserId, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageDto>> GetGroupHistoryAsync(int groupId, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationSummariesAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetPrivateUnreadCountAsync(string userId, string senderId, CancellationToken cancellationToken = default);
    Task MarkPrivateMessagesAsSeenAsync(string userId, string senderId, CancellationToken cancellationToken = default);
}
