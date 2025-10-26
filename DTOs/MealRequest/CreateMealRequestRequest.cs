using System;
using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class CreateMealRequestRequest
{
    [Required(ErrorMessage = "Öğün türü gerekli")]
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    
    public DateTime? PreferredDate { get; set; }
    public TimeSpan? PreferredStartTime { get; set; }
    public TimeSpan? PreferredEndTime { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}