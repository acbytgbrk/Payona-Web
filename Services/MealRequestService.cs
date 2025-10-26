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
        var mealRequest = new MealRequest
        {
            UserId = userId,
            MealType = request.MealType,
            PreferredDate = request.PreferredDate,
            PreferredStartTime = request.PreferredStartTime,
            PreferredEndTime = request.PreferredEndTime,
            Notes = request.Notes
        };

        _context.MealRequests.Add(mealRequest);
        await _context.SaveChangesAsync();

        await _context.Entry(mealRequest).Reference(m => m.User).LoadAsync();

        return new MealRequestDto
        {
            Id = mealRequest.Id,
            UserId = mealRequest.UserId,
            UserName = mealRequest.User.Name + " " + mealRequest.User.Surname.Substring(0, 1) + ".",
            UserGender = mealRequest.User.DormInfo.Gender,
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
            UserGender = m.User.DormInfo.Gender,
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
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return requests.Select(m => new MealRequestDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User.Name + " " + m.User.Surname,
            UserGender = m.User.DormInfo.Gender,
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