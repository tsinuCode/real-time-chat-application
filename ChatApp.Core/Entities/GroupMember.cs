namespace ChatApp.Core.Entities;

public class GroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ChatGroup? Group { get; set; }
    public ApplicationUser? User { get; set; }
}
