using System;
using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class CreateFingerprintRequest
{
    [Required(ErrorMessage = "Öğün türü gerekli")]
    public string MealType { get; set; } = string.Empty; // "lunch" veya "dinner"
    
    public DateTime? AvailableDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}