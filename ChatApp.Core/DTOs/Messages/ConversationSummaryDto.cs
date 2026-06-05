namespace ChatApp.Core.DTOs.Messages;

public class ConversationSummaryDto
{
    public string ConversationType { get; set; } = "private";
    public string ConversationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = "No messages yet";
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}
