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

    // -----------------------------------------
    // MATCH CREATE
    // -----------------------------------------
    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromQuery] Guid fingerprintId, [FromQuery] Guid mealRequestId)
    {
        if (fingerprintId == Guid.Empty || mealRequestId == Guid.Empty)
        {
            return BadRequest(new { message = "Geçersiz parmak izi veya yemek talebi ID'si" });
        }

        try
        {
            var userId = GetUserId();
            var result = await _matchService.CreateMatchAsync(fingerprintId, mealRequestId, userId);

            if (result == null)
                return BadRequest(new { message = "Eşleşme oluşturulamadı. Taleplerin aktif olduğundan ve farklı kullanıcılara ait olduğundan emin olun." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Eşleşme oluşturulamadı: " + ex.Message });
        }
    }

    // -----------------------------------------
    // GET MY MATCHES
    // -----------------------------------------
    [HttpGet("my")]
    public async Task<IActionResult> GetMyMatches()
    {
        var userId = GetUserId();
        var result = await _matchService.GetMyMatchesAsync(userId);
        return Ok(result);
    }

    // -----------------------------------------
    // UPDATE MATCH STATUS
    // -----------------------------------------
    [HttpPut("{matchId}/status")]
    public async Task<IActionResult> UpdateStatus(Guid matchId, [FromQuery] string status)
    {
        var userId = GetUserId();
        var success = await _matchService.UpdateMatchStatusAsync(matchId, userId, status);

        if (!success)
            return NotFound(new { message = "Eşleşme bulunamadı" });

        return Ok(new { message = "Durum güncellendi" });
    }

    // -----------------------------------------
    // ACTIVITY STATS
    // -----------------------------------------
    [HttpGet("activity-stats")]
    public async Task<IActionResult> GetActivityStats([FromQuery] string period = "week")
    {
        var userId = GetUserId();
        var result = await _matchService.GetActivityStatsAsync(userId, period);
        return Ok(result);
    }

    // -----------------------------------------
    // AUTO MATCH
    // -----------------------------------------
    [HttpPost("auto-match")]
    public async Task<IActionResult> CreateAutoMatch([FromQuery] Guid otherUserId, [FromQuery] string? mealType = null)
    {
        if (otherUserId == Guid.Empty)
            return BadRequest(new { message = "Geçersiz kullanıcı ID'si" });

        try
        {
            var userId = GetUserId();
            var result = await _matchService.CreateAutoMatchAsync(userId, otherUserId, mealType ?? "lunch");

            if (result == null)
                return BadRequest(new { message = "Eşleşme oluşturulamadı." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Eşleşme oluşturulamadı: " + ex.Message });
        }
    }

    // -----------------------------------------
    // GET MATCH BY SPECIFIC FINGERPRINT + REQUEST
    // -----------------------------------------
    [HttpGet("for-request")]
    public async Task<IActionResult> GetMatchByRequestAsync([FromQuery] Guid fingerprintId, [FromQuery] Guid mealRequestId)
    {
        if (fingerprintId == Guid.Empty || mealRequestId == Guid.Empty)
            return BadRequest(new { message = "Geçersiz ID" });

        var match = await _matchService.GetMatchByFingerprintAndRequestAsync(fingerprintId, mealRequestId);

        if (match == null)
            return Ok(null);

        return Ok(match);
    }
}