using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payona.API.Data;
using Payona.API.DTOs;
using Payona.API.Models;

namespace Payona.API.Services;

public class MessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageRequest request)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            MatchId = request.MatchId,
            Content = request.Content
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await _context.Entry(message).Reference(m => m.Sender).LoadAsync();
        await _context.Entry(message).Reference(m => m.Receiver).LoadAsync();

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender.Name + " " + message.Sender.Surname,
            ReceiverId = message.ReceiverId,
            ReceiverName = message.Receiver.Name + " " + message.Receiver.Surname,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                       (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // Okunmamış mesajları okundu yap
        var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead);
        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }
        await _context.SaveChangesAsync();

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender.Name + " " + m.Sender.Surname,
            ReceiverId = m.ReceiverId,
            ReceiverName = m.Receiver.Name + " " + m.Receiver.Surname,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<List<UserDto>> GetConversationListAsync(Guid userId)
    {
        var userIds = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .ToListAsync();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Surname = u.Surname
        }).ToList();
    }
}