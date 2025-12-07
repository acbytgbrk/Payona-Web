using System;
using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class CreateMealRequestRequest
{
    [Required(ErrorMessage = "Öğün türü gerekli")]
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    
    public string? PreferredDate { get; set; } // ISO date string: "YYYY-MM-DD"
    public string? PreferredStartTime { get; set; } // TimeSpan string: "HH:mm:ss"
    public string? PreferredEndTime { get; set; } // TimeSpan string: "HH:mm:ss"
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}