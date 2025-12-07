using System;
using System.Collections.Generic;

namespace Payona.API.DTOs;

public class ActivityStatsDto
{
    public List<DailyActivityDto> DailyStats { get; set; } = new();
    public int TotalRequestsCreated { get; set; }
    public int TotalMatchesAccepted { get; set; }
    public int TotalMatchesCreated { get; set; }
}

public class DailyActivityDto
{
    public DateTime Date { get; set; }
    public int RequestsCreated { get; set; }
    public int MatchesAccepted { get; set; }
    public int MatchesCreated { get; set; }
}

