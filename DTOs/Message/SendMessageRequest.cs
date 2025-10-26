using System;
using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class SendMessageRequest
{
    [Required]
    public Guid ReceiverId { get; set; }
    
    public Guid? MatchId { get; set; }
    
    [Required(ErrorMessage = "Mesaj içeriği gerekli")]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}