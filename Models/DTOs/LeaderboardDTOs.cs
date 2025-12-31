namespace Application.Models.DTOs;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string Email { get; set; } = string.Empty;
    public long Coins { get; set; }
}

public class LeaderboardResponse
{
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public int TotalCount { get; set; }
}

