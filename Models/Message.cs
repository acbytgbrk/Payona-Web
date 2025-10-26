using System;

namespace Payona.API.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid? MatchId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public Match? Match { get; set; }
}