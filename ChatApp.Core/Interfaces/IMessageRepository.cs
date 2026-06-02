using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);
}
