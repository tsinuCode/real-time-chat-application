using ChatApp.Core.DTOs.Groups;

namespace ChatApp.Web.Models;

public class GroupsIndexViewModel
{
    public IReadOnlyList<GroupDto> Groups { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public CreateGroupDto NewGroup { get; set; } = new();
}
