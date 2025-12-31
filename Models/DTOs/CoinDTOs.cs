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

