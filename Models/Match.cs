using System;
using System.Collections.Generic;

namespace Payona.API.Models;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FingerprintId { get; set; }
    public Guid MealRequestId { get; set; }
    public Guid GiverId { get; set; } // Parmak izi veren
    public Guid ReceiverId { get; set; } // Yemek alan
    public string MealType { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending"; // pending, accepted, rejected, completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Fingerprint Fingerprint { get; set; } = null!;
    public MealRequest MealRequest { get; set; } = null!;
    public User Giver { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}