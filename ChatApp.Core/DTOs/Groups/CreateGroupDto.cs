using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs.Groups;

public class CreateGroupDto
{
    [Required, MinLength(3), MaxLength(100)]
    public string GroupName { get; set; } = string.Empty;
}
