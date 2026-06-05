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
    private readonly IUserRepository _userRepository;
    private readonly IConnectionTracker _connectionTracker;

    public ChatHub(
        IMessageRepository messageRepository,
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IConnectionTracker connectionTracker)
    {
        _messageRepository = messageRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _connectionTracker = connectionTracker;
    }

    private string CurrentUserId =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string CurrentUsername =>
        Context.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public override async Task OnConnectedAsync()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Context.Abort();
            return;
        }

        _connectionTracker.AddConnection(CurrentUserId, Context.ConnectionId);
        await _userRepository.SetOnlineStatusAsync(CurrentUserId, true);
        await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUserId);
        await Clients.Others.SendAsync(RealtimeEventNames.UserStatusChanged, CurrentUserId, true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _connectionTracker.GetUserId(Context.ConnectionId) ?? CurrentUserId;
        _connectionTracker.RemoveConnection(Context.ConnectionId);

        if (!string.IsNullOrEmpty(userId) && _connectionTracker.GetConnections(userId).Count == 0)
        {
            await _userRepository.SetOnlineStatusAsync(userId, false);
            await Clients.Others.SendAsync(RealtimeEventNames.UserStatusChanged, userId, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var saved = await _messageRepository.AddAsync(CreateMessage(receiverId: receiverId, content: content));
        await BroadcastPrivateMessageAsync(receiverId, ToMessageDto(saved));
    }

    public async Task JoinGroupChat(int groupId)
    {
        await EnsureGroupMemberAsync(groupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupChannel(groupId));
    }

    public async Task SendGroupMessage(int groupId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        await EnsureGroupMemberAsync(groupId);

        var saved = await _messageRepository.AddAsync(CreateMessage(groupId: groupId, content: content));
        await Clients.Group(GetGroupChannel(groupId))
            .SendAsync(RealtimeEventNames.ReceiveGroupMessage, ToMessageDto(saved));
    }

    public async Task SendTypingIndicator(string? receiverId, int? groupId, bool isTyping)
    {
        var indicator = new TypingIndicatorDto
        {
            UserId = CurrentUserId,
            Username = CurrentUsername,
            ReceiverId = receiverId,
            GroupId = groupId,
            IsTyping = isTyping
        };

        if (groupId.HasValue)
        {
            if (!await _groupRepository.IsMemberAsync(groupId.Value, CurrentUserId))
            {
                return;
            }

            await Clients.Group(GetGroupChannel(groupId.Value))
                .SendAsync(RealtimeEventNames.TypingIndicator, indicator);
        }
        else if (!string.IsNullOrEmpty(receiverId))
        {
            await Clients.User(receiverId).SendAsync(RealtimeEventNames.TypingIndicator, indicator);
        }
    }

    private Message CreateMessage(string content, string? receiverId = null, int? groupId = null) =>
        new()
        {
            SenderId = CurrentUserId,
            ReceiverId = receiverId,
            GroupId = groupId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

    private async Task BroadcastPrivateMessageAsync(string receiverId, MessageDto dto)
    {
        await Clients.User(receiverId).SendAsync(RealtimeEventNames.ReceivePrivateMessage, dto);
        await Clients.Caller.SendAsync(RealtimeEventNames.ReceivePrivateMessage, dto);

        var unread = await _messageRepository.GetPrivateUnreadCountAsync(receiverId, CurrentUserId);
        await Clients.User(receiverId).SendAsync(
            RealtimeEventNames.UnreadCountUpdated,
            "private",
            CurrentUserId,
            unread);
    }

    private async Task EnsureGroupMemberAsync(int groupId)
    {
        if (!await _groupRepository.IsMemberAsync(groupId, CurrentUserId))
        {
            throw new HubException("You are not a member of this group.");
        }
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
