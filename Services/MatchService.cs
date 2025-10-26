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

        // Kendi ilanıyla eşleşmesin
        if (fingerprint.UserId == currentUserId || mealRequest.UserId == currentUserId)
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
}