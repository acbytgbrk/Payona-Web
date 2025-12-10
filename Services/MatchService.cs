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

    // =====================================================
    // CREATE MATCH
    // =====================================================
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

        bool fingerprintIsMine = fingerprint.UserId == currentUserId;
        bool mealRequestIsMine = mealRequest.UserId == currentUserId;

        // Kendi ilanıyla eşleşmesin
        if (fingerprintIsMine && mealRequestIsMine)
            return null;

        // İkisi de başkasına aitse eşleşme oluşturamaz
        if (!fingerprintIsMine && !mealRequestIsMine)
            return null;

        // Bu kombinasyon daha önce eşleşmiş mi?
        var existingMatch = await _context.Matches
            .FirstOrDefaultAsync(m =>
                m.FingerprintId == fingerprintId &&
                m.MealRequestId == mealRequestId);

        if (existingMatch != null)
        {
            await _context.Entry(existingMatch).Reference(m => m.Giver).LoadAsync();
            await _context.Entry(existingMatch).Reference(m => m.Receiver).LoadAsync();

            return new MatchDto
            {
                Id = existingMatch.Id,
                GiverId = existingMatch.GiverId,
                GiverName = existingMatch.Giver.Name + " " + existingMatch.Giver.Surname,
                ReceiverId = existingMatch.ReceiverId,
                ReceiverName = existingMatch.Receiver.Name + " " + existingMatch.Receiver.Surname,
                MealType = existingMatch.MealType,
                Status = existingMatch.Status,
                MatchDate = existingMatch.MatchDate
            };
        }

        // Yeni eşleşme oluştur
        var match = new Match
        {
            FingerprintId = fingerprintId,
            MealRequestId = mealRequestId,
            GiverId = fingerprint.UserId,
            ReceiverId = mealRequest.UserId,
            MealType = fingerprint.MealType
        };

        _context.Matches.Add(match);

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

    // =====================================================
    // GET MATCH FOR SPECIFIC REQUEST (NEW)
    // =====================================================
    public async Task<MatchDto?> GetMatchByFingerprintAndRequestAsync(Guid fingerprintId, Guid mealRequestId)
    {
        var match = await _context.Matches
            .Include(m => m.Giver)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m =>
                m.FingerprintId == fingerprintId &&
                m.MealRequestId == mealRequestId);

        if (match == null)
            return null;

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

    // =====================================================
    // GET MY MATCHES (FULL HISTORY)
    // =====================================================
    public async Task<List<MatchDto>> GetMyMatchesAsync(Guid userId)
    {
        var matches = await _context.Matches
            .Include(m => m.Giver)
            .Include(m => m.Receiver)
            .Where(m =>
                m.GiverId == userId ||
                m.ReceiverId == userId)
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

    // =====================================================
    // UPDATE STATUS
    // =====================================================
    public async Task<bool> UpdateMatchStatusAsync(Guid matchId, Guid userId, string status)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m =>
                m.Id == matchId &&
                (m.GiverId == userId || m.ReceiverId == userId));

        if (match == null)
            return false;

        match.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    // =====================================================
    // AUTO MATCH
    // =====================================================
    public async Task<MatchDto?> CreateAutoMatchAsync(Guid currentUserId, Guid otherUserId, string mealType = "lunch")
    {
        var myFingerprints = await _context.Fingerprints
            .Where(f => f.UserId == currentUserId && f.Status == "active")
            .ToListAsync();

        var myMealRequests = await _context.MealRequests
            .Where(m => m.UserId == currentUserId && m.Status == "active")
            .ToListAsync();

        var otherFingerprints = await _context.Fingerprints
            .Where(f => f.UserId == otherUserId && f.Status == "active")
            .ToListAsync();

        var otherMealRequests = await _context.MealRequests
            .Where(m => m.UserId == otherUserId && m.Status == "active")
            .ToListAsync();

        Guid fingerprintId;
        Guid mealRequestId;

        // Senaryo 1
        if (myFingerprints.Any() && otherMealRequests.Any())
        {
            var fp = myFingerprints.First();
            var mr = otherMealRequests.First();
            fingerprintId = fp.Id;
            mealRequestId = mr.Id;
        }
        // Senaryo 2
        else if (myMealRequests.Any() && otherFingerprints.Any())
        {
            var mr = myMealRequests.First();
            var fp = otherFingerprints.First();
            fingerprintId = fp.Id;
            mealRequestId = mr.Id;
        }
        // Senaryo 3
        else if (myFingerprints.Any() && !otherMealRequests.Any())
        {
            var fp = myFingerprints.First();
            fingerprintId = fp.Id;

            var newReq = new MealRequest
            {
                UserId = otherUserId,
                MealType = fp.MealType,
                PreferredDate = fp.AvailableDate,
                PreferredStartTime = fp.StartTime,
                PreferredEndTime = fp.EndTime,
                Notes = "Otomatik oluşturuldu",
                Status = "active"
            };

            _context.MealRequests.Add(newReq);
            await _context.SaveChangesAsync();
            mealRequestId = newReq.Id;
        }
        // Senaryo 4
        else if (myMealRequests.Any() && !otherFingerprints.Any())
        {
            var mr = myMealRequests.First();
            mealRequestId = mr.Id;

            var newFp = new Fingerprint
            {
                UserId = otherUserId,
                MealType = mr.MealType,
                AvailableDate = mr.PreferredDate,
                StartTime = mr.PreferredStartTime,
                EndTime = mr.PreferredEndTime,
                Description = "Otomatik oluşturuldu",
                Status = "active"
            };

            _context.Fingerprints.Add(newFp);
            await _context.SaveChangesAsync();
            fingerprintId = newFp.Id;
        }
        // Senaryo 5
        else
        {
            var defaultDate = DateTime.UtcNow.Date.AddDays(1);
            var defaultStart = new TimeSpan(12, 0, 0);
            var defaultEnd = new TimeSpan(14, 0, 0);

            var fp = new Fingerprint
            {
                UserId = currentUserId,
                MealType = mealType,
                AvailableDate = defaultDate,
                StartTime = defaultStart,
                EndTime = defaultEnd,
                Description = "Otomatik oluşturuldu",
                Status = "active"
            };

            var mr = new MealRequest
            {
                UserId = otherUserId,
                MealType = mealType,
                PreferredDate = defaultDate,
                PreferredStartTime = defaultStart,
                PreferredEndTime = defaultEnd,
                Notes = "Otomatik oluşturuldu",
                Status = "active"
            };

            _context.Fingerprints.Add(fp);
            _context.MealRequests.Add(mr);
            await _context.SaveChangesAsync();

            fingerprintId = fp.Id;
            mealRequestId = mr.Id;
        }

        return await CreateMatchAsync(fingerprintId, mealRequestId, currentUserId);
    }

    // =====================================================
    // ACTIVITY STATS
    // =====================================================
    public async Task<ActivityStatsDto> GetActivityStatsAsync(Guid userId, string period = "week")
    {
        var now = DateTime.UtcNow;
        DateTime startDate = period.ToLower() switch
        {
            "day" => now.Date,
            "week" => now.Date.AddDays(-7),
            "month" => now.Date.AddMonths(-1),
            "year" => now.Date.AddYears(-1),
            _ => now.Date.AddDays(-7)
        };

        var fingerprints = await _context.Fingerprints
            .Where(f => f.UserId == userId && f.CreatedAt >= startDate)
            .ToListAsync();

        var mealRequests = await _context.MealRequests
            .Where(m => m.UserId == userId && m.CreatedAt >= startDate)
            .ToListAsync();

        var matches = await _context.Matches
            .Where(m => (m.GiverId == userId || m.ReceiverId == userId) && m.CreatedAt >= startDate)
            .ToListAsync();

        var dailyStats = new Dictionary<DateTime, DailyActivityDto>();

        foreach (var fp in fingerprints)
        {
            var date = fp.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
                dailyStats[date] = new DailyActivityDto { Date = date };
            dailyStats[date].RequestsCreated++;
        }

        foreach (var mr in mealRequests)
        {
            var date = mr.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
                dailyStats[date] = new DailyActivityDto { Date = date };
            dailyStats[date].RequestsCreated++;
        }

        foreach (var match in matches)
        {
            var date = match.CreatedAt.Date;
            if (!dailyStats.ContainsKey(date))
                dailyStats[date] = new DailyActivityDto { Date = date };

            dailyStats[date].MatchesCreated++;

            if (match.ReceiverId == userId)
                dailyStats[date].MatchesAccepted++;
        }

        var allDates = new List<DateTime>();
        for (var d = startDate; d <= now.Date; d = d.AddDays(1))
        {
            allDates.Add(d);
            if (!dailyStats.ContainsKey(d))
                dailyStats[d] = new DailyActivityDto { Date = d };
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