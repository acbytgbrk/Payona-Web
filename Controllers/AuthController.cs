using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payona.API.DTOs;
using Payona.API.DTOs.Profile;
using Payona.API.Services;

namespace Payona.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return BadRequest(new { message = string.Join(" ", errors) });
        }

        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(new { message = "Bu e-posta zaten kullanımda" });
        }

        return Ok(new
        {
            Token = result.Token,
            User = new
            {
                id = result.User.Id,
                email = result.User.Email,
                name = result.User.Name,
                surname = result.User.Surname,
                gender = result.User.DormInfo?.Gender,
                city = result.User.DormInfo?.City,
                dorm = result.User.DormInfo?.Dorm
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return BadRequest(new { message = "E-posta veya şifre hatalı" });
        }

        return Ok(new
        {
            Token = result.Token,
            User = new
            {
                id = result.User.Id,
                email = result.User.Email,
                name = result.User.Name,
                surname = result.User.Surname,
                gender = result.User.DormInfo?.Gender,
                city = result.User.DormInfo?.City,
                dorm = result.User.DormInfo?.Dorm
            }
        });
    }

    [HttpPut("dorm-info/{id}")]
    [Authorize]
    public async Task<IActionResult> UpsertDormInfo(Guid id, [FromBody] DormInfoRequest dto)
    {
        var result = await _authService.DormInfo(id, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                name = result.User.Name,
                surname = result.User.Surname,
                gender = result.User.DormInfo?.Gender,
                city = result.User.DormInfo?.City,
                dorm = result.User.DormInfo?.Dorm
            }
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
        }

        var userId = Guid.Parse(userIdClaim);
        var result = await _authService.UpdateProfile(userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                name = result.User.Name,
                surname = result.User.Surname,
                gender = result.User.DormInfo?.Gender,
                city = result.User.DormInfo?.City,
                dorm = result.User.DormInfo?.Dorm
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Çıkış başarılı" });
    }
}