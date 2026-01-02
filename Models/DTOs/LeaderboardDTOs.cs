namespace Application.Models.DTOs;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string Email { get; set; } = string.Empty;
    public int TotalShipmentDelivered { get; set; }
    public long TotalIncome { get; set; }
    public long Value { get; set; } // = TotalShipmentDelivered * TotalIncome
}

public class LeaderboardResponse
{
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public int TotalCount { get; set; }
}

