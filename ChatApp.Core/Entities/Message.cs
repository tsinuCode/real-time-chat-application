namespace ChatApp.Core.Entities;

public class Message
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string? ReceiverId { get; set; }
    public int? GroupId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsSeen { get; set; }
}
