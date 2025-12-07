using System;

namespace Payona.API.DTOs;

public class ConversationSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsLastMessageFromMe { get; set; }
}

