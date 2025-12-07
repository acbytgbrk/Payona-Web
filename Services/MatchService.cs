using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payona.API.Data;
using Payona.API.DTOs;
using Payona.API.Models;

namespace Payona.API.Services;

public class MatchService
{
    private readonly AppDbContext _context;

    public MatchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MatchDto?> CreateMatchAsync(Guid fingerprintId, Guid mealRequestId, Guid currentUserId)
    {
        var fingerprint = await _context.Fingerprints
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == fingerprintId && f.Status == "active");

        var mealRequest = await _context.MealRequests
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == mealRequestId && m.Status == "active");

        if (fingerprint == null || mealRequest == null)
            return null;

        // Eşleşme mantığı:
        // - fingerprint birinin (giver), mealRequest diğerinin (receiver) olmalı
        // - İkisi de aynı kişiye aitse veya ikisi de başkasına aitse hata
        // - Biri currentUserId'ye, diğeri başkasına aitse OK
        
        bool fingerprintIsMine = fingerprint.UserId == currentUserId;
        bool mealRequestIsMine = mealRequest.UserId == currentUserId;
        
        // Kendi ilanıyla eşleşmesin (ikisi de benim)
        if (fingerprintIsMine && mealRequestIsMine)
            return null;
        
        // İkisi de başkasına aitse hata (ben eşleşme oluşturamam)
        if (!fingerprintIsMine && !mealRequestIsMine)
            return null;

        var match = new Match
        {
            FingerprintId = fingerprintId,
            MealRequestId = mealRequestId,
            GiverId = fingerprint.UserId,
            ReceiverId = mealRequest.UserId,
            MealType = fingerprint.MealType
        };

        _context.Matches.Add(match);

        // İlanları matched yap
        fingerprint.Status = "matched";
        mealRequest.Status = "matched";

        await _context.SaveChangesAsync();

        await _context.Entry(match).Reference(m => m.Giver).LoadAsync();
        await _context.Entry(match).Reference(m => m.Receiver).LoadAsync();

        return new MatchDto
        {
            Id = match.Id,
            GiverId = match.GiverId,
            GiverName = match.Giver.Name + " " + match.Giver.Surname,
            ReceiverId = match.ReceiverId,
            ReceiverName = match.Receiver.Name + " " + match.Receiver.Surname,
            MealType = match.MealType,
            Status = match.Status,
            MatchDate = match.MatchDate
        };
    }

    public async Task<List<MatchDto>> GetMyMatchesAsync(Guid userId)
    {
        var matches = await _context.Matches
            .Include(m => m.Giver)
            .Include(m => m.Receiver)
            .Where(m => m.GiverId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return matches.Select(m => new MatchDto
        {
            Id = m.Id,
            GiverId = m.GiverId,
            GiverName = m.Giver.Name + " " + m.Giver.Surname,
            ReceiverId = m.ReceiverId,
            ReceiverName = m.Receiver.Name + " " + m.Receiver.Surname,
            MealType = m.MealType,
            Status = m.Status,
            MatchDate = m.MatchDate
        }).ToList();
    }

    public async Task<bool> UpdateMatchStatusAsync(Guid matchId, Guid userId, string status)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId && 
                                     (m.GiverId == userId || m.ReceiverId == userId));

        if (match == null)
            return false;

        match.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(Guid userId, string period = "week")
    {
        var now = DateTime.UtcNow;
        DateTime startDate;

        switch (period.ToLower())
        {
            case "day":
                startDate = now.Date;
                break;
            case "week":
                startDate = now.Date.AddDays(-7);
                break;
            case "month":
                startDate = now.Date.AddMonths(-1);
                break;
            case "year":
                startDate = now.Date.AddYears(-1);
                break;
            default:
                startDate = now.Date.AddDays(-7);
                break;
        }

        // Get user's requests (fingerprints + meal requests)
        var fingerprints = await _context.Fingerprints
            .Where(f => f.UserId == userId && f.CreatedAt >= startDate)
            .ToListAsync();

        var mealRequests = await _context.MealRequests
            .Where(m => m.UserId == userId && m.CreatedAt >= startDate)
            .ToListAsync();

        // Get matches where user is giver or receiver
        var matches = await _context.Matches
            .Where(m => (m.GiverId == userId || m.ReceiverId == userId) && m.CreatedAt >= startDate)
            .ToListAsync();

        // Group by date
        var dailyStats = new Dictionary<DateTime, DailyActivityDto>();

        // Process requests
        foreach (var fp in fingerprints)
        {
            var date = fp.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
            {
                dailyStats[date] = new DailyActivityDto { Date = date };
            }
            dailyStats[date].RequestsCreated++;
        }

        foreach (var mr in mealRequests)
        {
            var date = mr.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
            {
                dailyStats[date] = new DailyActivityDto { Date = date };
            }
            dailyStats[date].RequestsCreated++;
        }

        // Process matches
        foreach (var match in matches)
        {
            var date = match.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
            {
                dailyStats[date] = new DailyActivityDto { Date = date };
            }
            
            // If user created the match (is giver or receiver), count as created
            dailyStats[date].MatchesCreated++;
            
            // If user is receiver, it's an accepted match (someone else's request matched with user's request)
            if (match.ReceiverId == userId)
            {
                dailyStats[date].MatchesAccepted++;
            }
        }

        // Fill missing dates with zeros
        var allDates = new List<DateTime>();
        for (var date = startDate; date <= now.Date; date = date.AddDays(1))
        {
            allDates.Add(date);
            if (!dailyStats.ContainsKey(date))
            {
                dailyStats[date] = new DailyActivityDto { Date = date };
            }
        }

        return new ActivityStatsDto
        {
            DailyStats = allDates.Select(d => dailyStats[d]).OrderBy(d => d.Date).ToList(),
            TotalRequestsCreated = fingerprints.Count + mealRequests.Count,
            TotalMatchesAccepted = matches.Count(m => m.ReceiverId == userId),
            TotalMatchesCreated = matches.Count
        };
    }
}