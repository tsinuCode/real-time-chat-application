namespace ChatApp.Core.DTOs.Chat;

public class TypingIndicatorDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ReceiverId { get; set; }
    public int? GroupId { get; set; }
    public bool IsTyping { get; set; }
}
