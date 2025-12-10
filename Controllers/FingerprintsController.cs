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
public class FingerprintsController : ControllerBase
{
    private readonly FingerprintService _fingerprintService;

    public FingerprintsController(FingerprintService fingerprintService)
    {
        _fingerprintService = fingerprintService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFingerprintRequest request)
    {
        var userId = GetUserId();
        var result = await _fingerprintService.CreateAsync(userId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? mealType = null)
    {
        var userId = GetUserId();
        var result = await _fingerprintService.GetAllActiveAsync(mealType, userId);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetUserId();
        var result = await _fingerprintService.GetMyFingerprintsAsync(userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var success = await _fingerprintService.DeleteAsync(id, userId);
        
        if (!success)
            return NotFound(new { message = "Parmak izi bulunamadı" });

        return Ok(new { message = "Silindi" });
    }
}