namespace ChatApp.Core.Entities;

public class ChatGroup
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
