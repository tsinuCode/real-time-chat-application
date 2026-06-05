namespace ChatApp.Core.DTOs.Chat;

/// <summary>
/// Client/server SignalR event names used by the chat hub and MVC client.
/// </summary>
public static class RealtimeEventNames
{
    public const string ReceivePrivateMessage = "ReceivePrivateMessage";
    public const string ReceiveGroupMessage = "ReceiveGroupMessage";
    public const string TypingIndicator = "TypingIndicator";
    public const string UserStatusChanged = "UserStatusChanged";
    public const string UnreadCountUpdated = "UnreadCountUpdated";
}
