using System;
using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class CreateFingerprintRequest
{
    [Required(ErrorMessage = "Öğün türü gerekli")]
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    
    public string? AvailableDate { get; set; } // ISO date string: "YYYY-MM-DD"
    public string? StartTime { get; set; } // TimeSpan string: "HH:mm:ss"
    public string? EndTime { get; set; } // TimeSpan string: "HH:mm:ss"
    
    [MaxLength(500)]
    public string? Description { get; set; }
}