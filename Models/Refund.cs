namespace ParkMate.Models;

public class Refund
{
    public int RefundId { get; set; }

    public int BId { get; set; }

    public int UId { get; set; }

    public decimal Amount { get; set; }

    public string RefundStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }

    public User? User { get; set; }

    public RefundReceipt? RefundReceipt { get; set; }
}