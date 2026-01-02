namespace Application.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public long Coins { get; set; }
    public int TotalShipmentDelivered { get; set; }
    public long TotalIncome { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

