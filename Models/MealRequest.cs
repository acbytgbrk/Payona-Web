using System;

namespace Payona.API.Models;

public class MealRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    public DateTime? PreferredDate { get; set; }
    public TimeSpan? PreferredStartTime { get; set; }
    public TimeSpan? PreferredEndTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "active"; // active, matched, cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
}