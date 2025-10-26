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
public class MealRequestsController : ControllerBase
{
    private readonly MealRequestService _mealRequestService;

    public MealRequestsController(MealRequestService mealRequestService)
    {
        _mealRequestService = mealRequestService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMealRequestRequest request)
    {
        var userId = GetUserId();
        var result = await _mealRequestService.CreateAsync(userId, request);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? mealType = null)
    {
        var result = await _mealRequestService.GetAllActiveAsync(mealType);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetUserId();
        var result = await _mealRequestService.GetMyRequestsAsync(userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var success = await _mealRequestService.DeleteAsync(id, userId);
        
        if (!success)
            return NotFound(new { message = "Talep bulunamadı" });

        return Ok(new { message = "Silindi" });
    }
}