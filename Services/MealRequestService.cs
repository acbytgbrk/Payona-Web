using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payona.API.Data;
using Payona.API.DTOs;
using Payona.API.Models;

namespace Payona.API.Services;

public class MealRequestService
{
    private readonly AppDbContext _context;

    public MealRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MealRequestDto> CreateAsync(Guid userId, CreateMealRequestRequest request)
    {
        // Parse DateTime from string (ensure UTC)
        DateTime? preferredDate = null;
        if (!string.IsNullOrWhiteSpace(request.PreferredDate))
        {
            // HTML date input sends "YYYY-MM-DD" format (date only, no time)
            // Parse as UTC to avoid timezone issues
            if (DateTime.TryParseExact(request.PreferredDate, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, 
                out var parsedDate))
            {
                preferredDate = parsedDate;
            }
            else if (DateTime.TryParse(request.PreferredDate, null, System.Globalization.DateTimeStyles.None, out var fallbackDate))
            {
                // Fallback: if parse exact fails, use regular parse and ensure UTC
                preferredDate = fallbackDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(fallbackDate.Date, DateTimeKind.Utc)
                    : fallbackDate.ToUniversalTime();
            }
        }

        // Parse TimeSpan from string
        TimeSpan? preferredStartTime = null;
        if (!string.IsNullOrWhiteSpace(request.PreferredStartTime))
        {
            if (TimeSpan.TryParse(request.PreferredStartTime, out var parsedStartTime))
            {
                preferredStartTime = parsedStartTime;
            }
        }

        TimeSpan? preferredEndTime = null;
        if (!string.IsNullOrWhiteSpace(request.PreferredEndTime))
        {
            if (TimeSpan.TryParse(request.PreferredEndTime, out var parsedEndTime))
            {
                preferredEndTime = parsedEndTime;
            }
        }

        var mealRequest = new MealRequest
        {
            UserId = userId,
            MealType = request.MealType,
            PreferredDate = preferredDate,
            PreferredStartTime = preferredStartTime,
            PreferredEndTime = preferredEndTime,
            Notes = request.Notes
        };

        _context.MealRequests.Add(mealRequest);
        await _context.SaveChangesAsync();

        await _context.Entry(mealRequest).Reference(m => m.User).LoadAsync();
        await _context.Entry(mealRequest.User).Reference(u => u.DormInfo).LoadAsync();

        return new MealRequestDto
        {
            Id = mealRequest.Id,
            UserId = mealRequest.UserId,
            UserName = mealRequest.User.Name + " " + mealRequest.User.Surname.Substring(0, 1) + ".",
            UserGender = mealRequest.User.DormInfo?.Gender,
            UserDorm = mealRequest.User.DormInfo?.Dorm,
            MealType = mealRequest.MealType,
            PreferredDate = mealRequest.PreferredDate,
            PreferredStartTime = mealRequest.PreferredStartTime,
            PreferredEndTime = mealRequest.PreferredEndTime,
            Notes = mealRequest.Notes,
            Status = mealRequest.Status,
            CreatedAt = mealRequest.CreatedAt
        };
    }

    public async Task<List<MealRequestDto>> GetAllActiveAsync(string? mealType = null)
    {
        var query = _context.MealRequests
            .Include(m => m.User)
                .ThenInclude(u => u.DormInfo)
            .Where(m => m.Status == "active");

        if (!string.IsNullOrEmpty(mealType))
        {
            query = query.Where(m => m.MealType == mealType);
        }

        var requests = await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return requests.Select(m => new MealRequestDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User.Name + " " + m.User.Surname.Substring(0, 1) + ".",
            UserGender = m.User.DormInfo?.Gender,
            UserDorm = m.User.DormInfo?.Dorm,
            MealType = m.MealType,
            PreferredDate = m.PreferredDate,
            PreferredStartTime = m.PreferredStartTime,
            PreferredEndTime = m.PreferredEndTime,
            Notes = m.Notes,
            Status = m.Status,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<List<MealRequestDto>> GetMyRequestsAsync(Guid userId)
    {
        var requests = await _context.MealRequests
            .Include(m => m.User)
                .ThenInclude(u => u.DormInfo)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return requests.Select(m => new MealRequestDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User.Name + " " + m.User.Surname,
            UserGender = m.User.DormInfo?.Gender,
            UserDorm = m.User.DormInfo?.Dorm,
            MealType = m.MealType,
            PreferredDate = m.PreferredDate,
            PreferredStartTime = m.PreferredStartTime,
            PreferredEndTime = m.PreferredEndTime,
            Notes = m.Notes,
            Status = m.Status,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteAsync(Guid requestId, Guid userId)
    {
        var request = await _context.MealRequests
            .FirstOrDefaultAsync(m => m.Id == requestId && m.UserId == userId);

        if (request == null)
            return false;

        request.Status = "cancelled";
        await _context.SaveChangesAsync();
        return true;
    }
}