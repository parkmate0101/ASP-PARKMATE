namespace ParkMate.Models;

public class Pricing
{
    public int PriceId { get; set; }

    public string VType { get; set; } = string.Empty;

    public int PricePerHour { get; set; }

    public int PricePerDay { get; set; }

    public DateTime UpdatedAt { get; set; }
}