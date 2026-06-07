using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs.Groups;

public class AddGroupMemberDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
