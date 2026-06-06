using ChatApp.Core.DTOs.Users;

namespace ChatApp.Web.Models;

public class DirectoryIndexViewModel
{
    public string? Query { get; set; }
    public IReadOnlyList<DirectoryUserSummaryDto> Users { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
