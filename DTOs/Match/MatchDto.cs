using System;

namespace Payona.API.DTOs;

public class MatchDto
{
    public Guid Id { get; set; }
    public Guid GiverId { get; set; }
    public string GiverName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
}