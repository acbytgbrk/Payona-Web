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

        // ÖNEMLİ: Geçmiş eşleşmeler yeni eşleşmeyi engellemez
        // Her yeni aktif talep bağımsız olarak eşleşebilir
        // Sadece aynı fingerprint ve mealRequest kombinasyonu için zaten eşleşme varsa kontrol et
        var existingMatch = await _context.Matches
            .FirstOrDefaultAsync(m => 
                m.FingerprintId == fingerprintId && 
                m.MealRequestId == mealRequestId);

        if (existingMatch != null)
        {
            // Bu spesifik kombinasyon zaten eşleşmiş, mevcut eşleşmeyi döndür
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

    /// <summary>
    /// İki kullanıcı arasında otomatik eşleşme oluşturur. Eksik talepleri otomatik oluşturur.
    /// </summary>
    public async Task<MatchDto?> CreateAutoMatchAsync(Guid currentUserId, Guid otherUserId, string mealType = "lunch")
    {
        // Mevcut aktif talepleri kontrol et
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

        // Senaryo 1: Benim fingerprint'im var, karşı tarafın meal request'i var
        if (myFingerprints.Any() && otherMealRequests.Any())
        {
            var matchingFp = myFingerprints.FirstOrDefault(f => string.IsNullOrWhiteSpace(mealType) || f.MealType == mealType) ?? myFingerprints.First();
            var matchingMr = otherMealRequests.FirstOrDefault(m => string.IsNullOrWhiteSpace(mealType) || m.MealType == mealType) ?? otherMealRequests.First();
            fingerprintId = matchingFp.Id;
            mealRequestId = matchingMr.Id;
        }
        // Senaryo 2: Benim meal request'im var, karşı tarafın fingerprint'i var
        else if (myMealRequests.Any() && otherFingerprints.Any())
        {
            var matchingMr = myMealRequests.FirstOrDefault(m => string.IsNullOrWhiteSpace(mealType) || m.MealType == mealType) ?? myMealRequests.First();
            var matchingFp = otherFingerprints.FirstOrDefault(f => string.IsNullOrWhiteSpace(mealType) || f.MealType == mealType) ?? otherFingerprints.First();
            mealRequestId = matchingMr.Id;
            fingerprintId = matchingFp.Id;
        }
        // Senaryo 3: Benim fingerprint'im var, karşı tarafın meal request'i yok - otomatik oluştur
        else if (myFingerprints.Any() && !otherMealRequests.Any())
        {
            var myFingerprint = myFingerprints.FirstOrDefault(f => string.IsNullOrWhiteSpace(mealType) || f.MealType == mealType) ?? myFingerprints.First();
            fingerprintId = myFingerprint.Id;
            
            // Karşı taraf için meal request oluştur
            var newMealRequest = new MealRequest
            {
                UserId = otherUserId,
                MealType = myFingerprint.MealType,
                PreferredDate = myFingerprint.AvailableDate,
                PreferredStartTime = myFingerprint.StartTime,
                PreferredEndTime = myFingerprint.EndTime,
                Notes = "Otomatik oluşturuldu",
                Status = "active"
            };
            
            _context.MealRequests.Add(newMealRequest);
            await _context.SaveChangesAsync();
            mealRequestId = newMealRequest.Id;
        }
        // Senaryo 4: Benim meal request'im var, karşı tarafın fingerprint'i yok - otomatik oluştur
        else if (myMealRequests.Any() && !otherFingerprints.Any())
        {
            var myMealRequest = myMealRequests.FirstOrDefault(m => string.IsNullOrWhiteSpace(mealType) || m.MealType == mealType) ?? myMealRequests.First();
            mealRequestId = myMealRequest.Id;
            
            // Karşı taraf için fingerprint oluştur
            var newFingerprint = new Fingerprint
            {
                UserId = otherUserId,
                MealType = myMealRequest.MealType,
                AvailableDate = myMealRequest.PreferredDate,
                StartTime = myMealRequest.PreferredStartTime,
                EndTime = myMealRequest.PreferredEndTime,
                Description = "Otomatik oluşturuldu",
                Status = "active"
            };
            
            _context.Fingerprints.Add(newFingerprint);
            await _context.SaveChangesAsync();
            fingerprintId = newFingerprint.Id;
        }
        // Senaryo 5: Hiçbiri yok - her iki taraf için de otomatik oluştur
        else
        {
            // Varsayılan değerler
            var defaultDate = DateTime.UtcNow.Date.AddDays(1);
            var defaultStartTime = new TimeSpan(12, 0, 0); // 12:00
            var defaultEndTime = new TimeSpan(14, 0, 0); // 14:00
            
            // Benim için fingerprint oluştur
            var myFingerprint = new Fingerprint
            {
                UserId = currentUserId,
                MealType = mealType,
                AvailableDate = defaultDate,
                StartTime = defaultStartTime,
                EndTime = defaultEndTime,
                Description = "Otomatik oluşturuldu",
                Status = "active"
            };
            
            // Karşı taraf için meal request oluştur
            var otherMealRequest = new MealRequest
            {
                UserId = otherUserId,
                MealType = mealType,
                PreferredDate = defaultDate,
                PreferredStartTime = defaultStartTime,
                PreferredEndTime = defaultEndTime,
                Notes = "Otomatik oluşturuldu",
                Status = "active"
            };
            
            _context.Fingerprints.Add(myFingerprint);
            _context.MealRequests.Add(otherMealRequest);
            await _context.SaveChangesAsync();
            
            fingerprintId = myFingerprint.Id;
            mealRequestId = otherMealRequest.Id;
        }

        // Eşleşme oluştur
        return await CreateMatchAsync(fingerprintId, mealRequestId, currentUserId);
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