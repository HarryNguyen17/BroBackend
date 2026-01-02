namespace Application.Models.DTOs;

public class CoinResponse
{
    public long Coins { get; set; }
}

public class UpdateCoinRequest
{
    public long Coins { get; set; }
}

public class UpdateCoinResponse
{
    public bool Success { get; set; }
    public long Coins { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class StatsResponse
{
    public long Coins { get; set; }
    public int TotalShipmentDelivered { get; set; }
    public long TotalIncome { get; set; }
}

public class UpdateStatsRequest
{
    public long Coins { get; set; }
    public int TotalShipmentDelivered { get; set; }
    public long TotalIncome { get; set; }
}

public class UpdateStatsResponse
{
    public bool Success { get; set; }
    public long Coins { get; set; }
    public int TotalShipmentDelivered { get; set; }
    public long TotalIncome { get; set; }
    public string Message { get; set; } = string.Empty;
}

