using ChatApp.Core.DTOs.Users;

namespace ChatApp.Web.Models;

public class DirectoryDetailsViewModel
{
    public DirectoryUserDetailDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}
