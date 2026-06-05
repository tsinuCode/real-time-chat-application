using Microsoft.AspNetCore.Identity;

namespace ChatApp.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<ChatGroup> CreatedGroups { get; set; } = new List<ChatGroup>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
}
