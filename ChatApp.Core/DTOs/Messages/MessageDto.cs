namespace ChatApp.Core.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderUsername { get; set; } = string.Empty;
    public string? ReceiverId { get; set; }
    public int? GroupId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsSeen { get; set; }
}
