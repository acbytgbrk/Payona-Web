using System;

namespace Payona.API.Models;

public class Fingerprint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    public DateTime? AvailableDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "active"; // active, matched, cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
}