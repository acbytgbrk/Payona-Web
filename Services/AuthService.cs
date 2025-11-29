using Microsoft.EntityFrameworkCore;
using Payona.API.Data;
using Payona.API.DTOs;
using Payona.API.DTOs.Profile;
using Payona.API.Models;

namespace Payona.API.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly EmailService _emailService;

    public AuthService(AppDbContext context, JwtService jwtService, EmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    // ✅ Kayıt Ol
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return null;
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Surname = request.Surname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                
            }
        };
    }

    // ✅ Giriş Yap
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Temporary test user for development (remove in production)
        if (request.Email == "acabeytugberk@gmail.com" && request.Password == "1234567")
        {
            var testUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var testToken = _jwtService.GenerateTokenForTestUser(testUserId, request.Email);

            return new AuthResponse
            {
                Token = testToken,
                User = new UserDto
                {
                    Id = testUserId,
                    Email = request.Email,
                    Name = "Tuğberk",
                    Surname = "Acabey",
                    Gender = null,
                    City = null,
                    Dorm = null
                }
            };
        }

        var user = await _context.Users.Include(user => user.DormInfo).FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Gender = user.DormInfo?.Gender, 
                City = user.DormInfo?.City,     
                Dorm = user.DormInfo?.Dorm   
            }
        };
    }

    // ✅ yurt bilgisi Tamamla
    public async Task<(bool Success, string Message, User? User)> DormInfo(Guid id, DormInfoRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Gender))
            return (false, "Cinsiyet zorunludur", null);
        if (string.IsNullOrWhiteSpace(dto.City))
            return (false, "Şehir zorunludur", null);
        if (string.IsNullOrWhiteSpace(dto.Dorm))
            return (false, "Yurt zorunludur", null);

        var user = await _context.Users
            .Include(u => u.DormInfo)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return (false, "Kullanıcı bulunamadı", null);

        if (user.DormInfo == null)
        {
            // ✅ INSERT
            _context.DormInfos.Add(new DormInfo
            {
                UserId = user.Id,
                Gender = dto.Gender,
                City = dto.City,
                Dorm = dto.Dorm
            });
        }
        else
        {
            // ✅ UPDATE
            user.DormInfo.Gender = dto.Gender;
            user.DormInfo.City = dto.City;
            user.DormInfo.Dorm = dto.Dorm;
            user.DormInfo.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ✅ Reload
        await _context.Entry(user).ReloadAsync();

        return (true, "Profil başarıyla tamamlandı", user);
    }
    
    public async Task<(bool Success, string Message, User? User)> UpdateProfile(Guid userId, UpdateProfileRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Ad zorunludur", null);

        if (string.IsNullOrWhiteSpace(dto.Surname))
            return (false, "Soyad zorunludur", null);

        if (string.IsNullOrWhiteSpace(dto.Email))
            return (false, "E-posta zorunludur", null);

        var user = await _context.Users
            .Include(u => u.DormInfo)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return (false, "Kullanıcı bulunamadı", null);

        // Email değişmişse, başka kullanıcı kullanıyor mu kontrol et
        if (user.Email != dto.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != userId);

            if (emailExists)
                return (false, "Bu e-posta adresi zaten kullanılıyor", null);
        }

        user.Name = dto.Name;
        user.Surname = dto.Surname;
        user.Email = dto.Email;

        await _context.SaveChangesAsync();

        return (true, "Profil başarıyla güncellendi", user);
    }

    // ✅ Çıkış Yap
    public async Task<(bool Success, string Message)> LogoutAsync(string token)
    {
        // ✅ Token blacklist'e eklenebilir (Redis kullanılabilir)
        // Şimdilik sadece başarılı dön
        await Task.CompletedTask; // Async için

        return (true, "Başarıyla çıkış yapıldı");
    }

    // ✅ Hesap Sil
    public async Task<(bool Success, string Message)> DeleteAccountAsync(Guid userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return (false, "Kullanıcı bulunamadı");
        }

        // ✅ Şifre doğrulaması
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return (false, "Şifre hatalı");
        }

        // ✅ Kullanıcıyı sil
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return (true, "Hesap başarıyla silindi");
    }

    // ✅ Şifre Değiştir
    public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return (false, "Kullanıcı bulunamadı");
        }

        // ✅ Eski şifre doğrulaması
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
        {
            return (false, "Eski şifre hatalı");
        }

        // ✅ Yeni şifre validasyonu
        if (request.NewPassword.Length < 6)
        {
            return (false, "Yeni şifre en az 6 karakter olmalıdır");
        }

        // ✅ Eski şifre ile aynı olmamalı
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            return (false, "Yeni şifre eski şifre ile aynı olamaz");
        }

        // ✅ Yeni şifreyi hashle ve kaydet
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        

        await _context.SaveChangesAsync();

        return (true, "Şifre başarıyla değiştirildi");
    }

    // ✅ Şifre Sıfırlama Talebi (Email gönder)
    public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // ✅ Güvenlik: Email var mı yok mu belli olmasın
            return (true, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi");
        }

        // ✅ Reset token oluştur (1 saatlik geçerlilik)
        var resetToken = Guid.NewGuid().ToString();
        user.ResetPasswordToken = resetToken;
        user.ResetPasswordExpires = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        
         await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

        Console.WriteLine($"[AuthService] Password reset token for {user.Email}: {resetToken}");
        Console.WriteLine($"[AuthService] Reset link: http://localhost:5049/reset-password?token={resetToken}");

        return (true, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi");
    }

    // ✅ Şifre Sıfırlama (Token ile)
    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.ResetPasswordToken == request.Token &&
            u.ResetPasswordExpires > DateTime.UtcNow);

        if (user == null)
        {
            return (false, "Geçersiz veya süresi dolmuş token");
        }

        // ✅ Yeni şifre validasyonu
        if (request.NewPassword.Length < 6)
        {
            return (false, "Şifre en az 6 karakter olmalıdır");
        }

        // ✅ Yeni şifreyi hashle ve kaydet
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordExpires = null;
        

        await _context.SaveChangesAsync();

        return (true, "Şifre başarıyla sıfırlandı");
    }
    
    
}