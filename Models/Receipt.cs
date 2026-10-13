namespace ParkMate.Models;

public class Receipt
{
    public int RecId { get; set; }

    public int PId { get; set; }

    public string RecNo { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }

    public Payment? Payment { get; set; }
}