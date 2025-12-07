using System;

namespace Payona.API.DTOs;

public class FingerprintDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserGender { get; set; }
    public string? UserDorm { get; set; }
    public string MealType { get; set; } = string.Empty;
    public DateTime? AvailableDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}