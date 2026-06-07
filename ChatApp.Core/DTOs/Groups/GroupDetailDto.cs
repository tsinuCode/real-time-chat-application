namespace ChatApp.Core.DTOs.Groups;

public class GroupDetailDto
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatorUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<GroupMemberDto> Members { get; set; } = [];
}
