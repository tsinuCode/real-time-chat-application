namespace ChatApp.Core.DTOs.Users;

public class DirectoryUserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
}
