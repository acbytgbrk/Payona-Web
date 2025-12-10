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
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return BadRequest(new { message = "E-posta veya şifre hatalı" });
        }
        return Ok(result);
    }

    [HttpPut("dorm-info/{id}")]
    [Authorize]
    public async Task<IActionResult> UpsertDormInfo(Guid id, [FromBody] DormInfoRequest dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            return BadRequest(new { message = string.Join(" ", errors) });
        }

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
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }

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

    // ✅ Çıkış Yap
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await _authService.LogoutAsync(token);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // ✅ Hesap Sil
    [HttpDelete("delete-account/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(Guid id, [FromBody] DeleteAccountRequest request)
    {
        Console.WriteLine("hata1" +id);
        Console.WriteLine("hata2" +request.Password);
        // Token'dan kullanıcı ID'sini al
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        

        if (string.IsNullOrEmpty(userIdClaim) || userIdClaim != id.ToString())
        {
            return Unauthorized(new { message = "Yetkisiz işlem" });
        }

        var result = await _authService.DeleteAccountAsync(id, request.Password);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // ✅ Şifre Değiştir
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
       
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new { message = "Yetkisiz işlem" });
        }

        var userId = Guid.Parse(userIdClaim);
        var result = await _authService.ChangePasswordAsync(userId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // ✅ Şifre Sıfırlama Talebi
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);
        return Ok(new { message = result.Message });
    }

    // ✅ Şifre Sıfırlama
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }
}