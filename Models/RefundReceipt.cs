namespace ParkMate.Models;

public class RefundReceipt
{
    public int RefundRecId { get; set; }

    public int RefundId { get; set; }

    public string RecNo { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }

    public Refund? Refund { get; set; }
}