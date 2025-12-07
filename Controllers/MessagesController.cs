using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Payona.API.DTOs;
using Payona.API.Services;

namespace Payona.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessagesController(MessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "İstek verisi bulunamadı" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            return BadRequest(new { message = string.Join(" ", errors) });
        }

        try
        {
            var userId = GetUserId();
            var result = await _messageService.SendMessageAsync(userId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Mesaj gönderilemedi: " + ex.Message });
        }
    }

    [HttpGet("conversation/{otherUserId}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId)
    {
        var userId = GetUserId();
        var result = await _messageService.GetConversationAsync(userId, otherUserId);
        return Ok(result);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversationList()
    {
        var userId = GetUserId();
        var result = await _messageService.GetConversationListAsync(userId);
        return Ok(result);
    }

    [HttpGet("conversations/summaries")]
    public async Task<IActionResult> GetConversationSummaries()
    {
        var userId = GetUserId();
        var result = await _messageService.GetConversationSummariesAsync(userId);
        return Ok(result);
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInboxMessages()
    {
        var userId = GetUserId();
        var result = await _messageService.GetInboxMessagesAsync(userId);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _messageService.GetUnreadMessageCountAsync(userId);
        return Ok(new { count });
    }
}