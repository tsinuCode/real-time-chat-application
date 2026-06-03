using System.Security.Claims;
using ChatApp.Core.DTOs.Chat;
using ChatApp.Core.DTOs.Messages;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IConnectionTracker _connectionTracker;

    public ChatHub(
        IMessageRepository messageRepository,
        IGroupRepository groupRepository,
        IConnectionTracker connectionTracker)
    {
        _messageRepository = messageRepository;
        _groupRepository = groupRepository;
        _connectionTracker = connectionTracker;
    }

    private string CurrentUserId =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string CurrentUsername =>
        Context.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public override async Task OnConnectedAsync()
    {
        _connectionTracker.AddConnection(CurrentUserId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionTracker.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var message = new Message
        {
            SenderId = CurrentUserId,
            ReceiverId = receiverId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

        var saved = await _messageRepository.AddAsync(message);
        var dto = ToMessageDto(saved);

        await Clients.User(receiverId).SendAsync(RealtimeEventNames.ReceivePrivateMessage, dto);
        await Clients.Caller.SendAsync(RealtimeEventNames.ReceivePrivateMessage, dto);

        var unread = await _messageRepository.GetPrivateUnreadCountAsync(receiverId, CurrentUserId);
        await Clients.User(receiverId).SendAsync(
            RealtimeEventNames.UnreadCountUpdated,
            "private",
            CurrentUserId,
            unread);
    }

    public async Task JoinGroupChat(int groupId)
    {
        if (!await _groupRepository.IsMemberAsync(groupId, CurrentUserId))
        {
            throw new HubException("You are not a member of this group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupChannel(groupId));
    }

    public async Task SendGroupMessage(int groupId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        if (!await _groupRepository.IsMemberAsync(groupId, CurrentUserId))
        {
            throw new HubException("You are not a member of this group.");
        }

        var message = new Message
        {
            SenderId = CurrentUserId,
            GroupId = groupId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

        var saved = await _messageRepository.AddAsync(message);
        var dto = ToMessageDto(saved);

        await Clients.Group(GetGroupChannel(groupId))
            .SendAsync(RealtimeEventNames.ReceiveGroupMessage, dto);
    }

    private MessageDto ToMessageDto(Message message) => new()
    {
        Id = message.Id,
        SenderId = message.SenderId,
        SenderUsername = CurrentUsername,
        ReceiverId = message.ReceiverId,
        GroupId = message.GroupId,
        Content = message.Content,
        SentAt = message.SentAt,
        IsSeen = message.IsSeen
    };

    private static string GetGroupChannel(int groupId) => $"group-{groupId}";
}
