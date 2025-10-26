using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Payona.API.Services;

namespace Payona.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly MatchService _matchService;

    public MatchesController(MatchService matchService)
    {
        _matchService = matchService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromQuery] Guid fingerprintId, [FromQuery] Guid mealRequestId)
    {
        var userId = GetUserId();
        var result = await _matchService.CreateMatchAsync(fingerprintId, mealRequestId, userId);
        
        if (result == null)
            return BadRequest(new { message = "Eşleşme oluşturulamadı" });

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyMatches()
    {
        var userId = GetUserId();
        var result = await _matchService.GetMyMatchesAsync(userId);
        return Ok(result);
    }

    [HttpPut("{matchId}/status")]
    public async Task<IActionResult> UpdateStatus(Guid matchId, [FromQuery] string status)
    {
        var userId = GetUserId();
        var success = await _matchService.UpdateMatchStatusAsync(matchId, userId, status);
        
        if (!success)
            return NotFound(new { message = "Eşleşme bulunamadı" });

        return Ok(new { message = "Durum güncellendi" });
    }
}