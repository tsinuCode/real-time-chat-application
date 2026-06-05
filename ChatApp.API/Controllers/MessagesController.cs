using System.Security.Claims;
using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Messages;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;

    public MessagesController(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet("private/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MessageDto>>>> GetPrivateHistory(string otherUserId)
    {
        var history = await _messageRepository.GetPrivateHistoryAsync(CurrentUserId, otherUserId);
        await _messageRepository.MarkPrivateMessagesAsSeenAsync(CurrentUserId, otherUserId);
        return Ok(ApiResponse<IReadOnlyList<MessageDto>>.Ok(history));
    }

    [HttpGet("group/{groupId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MessageDto>>>> GetGroupHistory(int groupId)
    {
        var history = await _messageRepository.GetGroupHistoryAsync(groupId);
        return Ok(ApiResponse<IReadOnlyList<MessageDto>>.Ok(history));
    }

    [HttpGet("summaries")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConversationSummaryDto>>>> GetConversationSummaries()
    {
        var summaries = await _messageRepository.GetConversationSummariesAsync(CurrentUserId);
        return Ok(ApiResponse<IReadOnlyList<ConversationSummaryDto>>.Ok(summaries));
    }
}
