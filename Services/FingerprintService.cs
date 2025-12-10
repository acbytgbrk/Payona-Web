using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payona.API.Data;
using Payona.API.DTOs;
using Payona.API.Models;

namespace Payona.API.Services;

public class FingerprintService
{
    private readonly AppDbContext _context;

    public FingerprintService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FingerprintDto> CreateAsync(Guid userId, CreateFingerprintRequest request)
    {
        // Parse DateTime from string (ensure UTC)
        DateTime? availableDate = null;
        if (!string.IsNullOrWhiteSpace(request.AvailableDate))
        {
            // HTML date input sends "YYYY-MM-DD" format (date only, no time)
            // Parse as UTC to avoid timezone issues
            if (DateTime.TryParseExact(request.AvailableDate, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, 
                out var parsedDate))
            {
                availableDate = parsedDate;
            }
            else if (DateTime.TryParse(request.AvailableDate, null, System.Globalization.DateTimeStyles.None, out var fallbackDate))
            {
                // Fallback: if parse exact fails, use regular parse and ensure UTC
                availableDate = fallbackDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(fallbackDate.Date, DateTimeKind.Utc)
                    : fallbackDate.ToUniversalTime();
            }
        }

        // Parse TimeSpan from string
        TimeSpan? startTime = null;
        if (!string.IsNullOrWhiteSpace(request.StartTime))
        {
            if (TimeSpan.TryParse(request.StartTime, out var parsedStartTime))
            {
                startTime = parsedStartTime;
            }
        }

        TimeSpan? endTime = null;
        if (!string.IsNullOrWhiteSpace(request.EndTime))
        {
            if (TimeSpan.TryParse(request.EndTime, out var parsedEndTime))
            {
                endTime = parsedEndTime;
            }
        }

        var fingerprint = new Fingerprint
        {
            UserId = userId,
            MealType = request.MealType,
            AvailableDate = availableDate,
            StartTime = startTime,
            EndTime = endTime,
            Description = request.Description
        };

        _context.Fingerprints.Add(fingerprint);
        await _context.SaveChangesAsync();

        // Load user for DTO
        await _context.Entry(fingerprint).Reference(f => f.User).LoadAsync();
        await _context.Entry(fingerprint.User).Reference(u => u.DormInfo).LoadAsync();

        return new FingerprintDto
        {
            Id = fingerprint.Id,
            UserId = fingerprint.UserId,
            UserName = fingerprint.User.Name + " " + fingerprint.User.Surname.Substring(0, 1) + ".",
            UserGender = fingerprint.User.DormInfo?.Gender,
            UserDorm = fingerprint.User.DormInfo?.Dorm,
            MealType = fingerprint.MealType,
            AvailableDate = fingerprint.AvailableDate,
            StartTime = fingerprint.StartTime,
            EndTime = fingerprint.EndTime,
            Description = fingerprint.Description,
            Status = fingerprint.Status,
            CreatedAt = fingerprint.CreatedAt
        };
    }

    public async Task<List<FingerprintDto>> GetAllActiveAsync(string? mealType = null, Guid? currentUserId = null)
    {
        var query = _context.Fingerprints
            .Include(f => f.User)
                .ThenInclude(u => u.DormInfo)
            .Where(f => f.Status == "active");

        // Şehir ve yurt filtresi: Kullanıcı sadece aynı şehir ve yurttaki talepleri görebilir
        if (currentUserId.HasValue)
        {
            var currentUser = await _context.Users
                .Include(u => u.DormInfo)
                .FirstOrDefaultAsync(u => u.Id == currentUserId.Value);

            if (currentUser?.DormInfo != null)
            {
                var userCity = currentUser.DormInfo.City;
                var userDorm = currentUser.DormInfo.Dorm;

                // Aynı şehir ve yurttaki talepleri filtrele
                query = query.Where(f => 
                    f.User.DormInfo != null &&
                    f.User.DormInfo.City == userCity &&
                    f.User.DormInfo.Dorm == userDorm &&
                    f.UserId != currentUserId.Value // Kendi taleplerini gösterme
                );
            }
            else
            {
                // DormInfo yoksa hiçbir şey gösterme
                query = query.Where(f => false);
            }
        }

        if (!string.IsNullOrEmpty(mealType))
        {
            query = query.Where(f => f.MealType == mealType);
        }

        var fingerprints = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fingerprints.Select(f => new FingerprintDto
        {
            Id = f.Id,
            UserId = f.UserId,
            UserName = f.User.Name + " " + f.User.Surname.Substring(0, 1) + ".",
            UserGender = f.User.DormInfo?.Gender,
            UserDorm = f.User.DormInfo?.Dorm,
            MealType = f.MealType,
            AvailableDate = f.AvailableDate,
            StartTime = f.StartTime,
            EndTime = f.EndTime,
            Description = f.Description,
            Status = f.Status,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task<List<FingerprintDto>> GetMyFingerprintsAsync(Guid userId)
    {
        var fingerprints = await _context.Fingerprints
            .Include(f => f.User)
                .ThenInclude(u => u.DormInfo)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fingerprints.Select(f => new FingerprintDto
        {
            Id = f.Id,
            UserId = f.UserId,
            UserName = f.User.Name + " " + f.User.Surname,
            UserGender = f.User.DormInfo?.Gender,
            UserDorm = f.User.DormInfo?.Dorm,
            MealType = f.MealType,
            AvailableDate = f.AvailableDate,
            StartTime = f.StartTime,
            EndTime = f.EndTime,
            Description = f.Description,
            Status = f.Status,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteAsync(Guid fingerprintId, Guid userId)
    {
        var fingerprint = await _context.Fingerprints
            .FirstOrDefaultAsync(f => f.Id == fingerprintId && f.UserId == userId);

        if (fingerprint == null)
            return false;

        fingerprint.Status = "cancelled";
        await _context.SaveChangesAsync();
        return true;
    }
}