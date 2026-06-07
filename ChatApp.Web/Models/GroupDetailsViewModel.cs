using ChatApp.Core.DTOs.Groups;
using ChatApp.Core.DTOs.Users;

namespace ChatApp.Web.Models;

public class GroupDetailsViewModel
{
    public GroupDetailDto? Group { get; set; }
    public IReadOnlyList<DirectoryUserSummaryDto> AvailableUsers { get; set; } = [];
    public string? SearchQuery { get; set; }
    public string? ErrorMessage { get; set; }
}
