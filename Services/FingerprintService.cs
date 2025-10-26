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
        var fingerprint = new Fingerprint
        {
            UserId = userId,
            MealType = request.MealType,
            AvailableDate = request.AvailableDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Description = request.Description
        };

        _context.Fingerprints.Add(fingerprint);
        await _context.SaveChangesAsync();

        // Load user for DTO
        await _context.Entry(fingerprint).Reference(f => f.User).LoadAsync();

        return new FingerprintDto
        {
            Id = fingerprint.Id,
            UserId = fingerprint.UserId,
            UserName = fingerprint.User.Name + " " + fingerprint.User.Surname.Substring(0, 1) + ".",
            UserGender = fingerprint.User.DormInfo.Gender,
            MealType = fingerprint.MealType,
            AvailableDate = fingerprint.AvailableDate,
            StartTime = fingerprint.StartTime,
            EndTime = fingerprint.EndTime,
            Description = fingerprint.Description,
            Status = fingerprint.Status,
            CreatedAt = fingerprint.CreatedAt
        };
    }

    public async Task<List<FingerprintDto>> GetAllActiveAsync(string? mealType = null)
    {
        var query = _context.Fingerprints
            .Include(f => f.User)
            .Where(f => f.Status == "active");

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
            UserGender = f.User.DormInfo.Gender,
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
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fingerprints.Select(f => new FingerprintDto
        {
            Id = f.Id,
            UserId = f.UserId,
            UserName = f.User.Name + " " + f.User.Surname,
            UserGender = f.User.DormInfo.Gender,
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