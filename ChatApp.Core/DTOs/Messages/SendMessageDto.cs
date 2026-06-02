using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs.Messages;

public class SendMessageDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string? ReceiverId { get; set; }
    public int? GroupId { get; set; }
}
