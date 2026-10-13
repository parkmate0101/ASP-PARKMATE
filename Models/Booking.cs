namespace ParkMate.Models;

public class Booking
{
    public int BId { get; set; }

    public int UId { get; set; }

    public int VId { get; set; }

    public string VType { get; set; } = string.Empty;

    public int SlotId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;

    public string BStatus { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }

    public decimal? RefundAmount { get; set; }

    public User? User { get; set; }

    public Vehicle? Vehicle { get; set; }

    public Slot? Slot { get; set; }

    public ICollection<Payment> Payments { get; set; }
        = new List<Payment>();

    public ICollection<Feedback> Feedbacks { get; set; }
        = new List<Feedback>();

    public Refund? Refund { get; set; }
}