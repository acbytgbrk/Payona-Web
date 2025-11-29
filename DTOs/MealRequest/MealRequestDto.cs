using System;

namespace Payona.API.DTOs;

public class MealRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserGender { get; set; }
    public string? UserDorm { get; set; }
    public string MealType { get; set; } = string.Empty;
    public DateTime? PreferredDate { get; set; }
    public TimeSpan? PreferredStartTime { get; set; }
    public TimeSpan? PreferredEndTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}