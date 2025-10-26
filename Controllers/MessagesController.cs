using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        var userId = GetUserId();
        var result = await _messageService.SendMessageAsync(userId, request);
        return Ok(result);
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
}