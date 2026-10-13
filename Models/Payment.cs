namespace ParkMate.Models;

public class Payment
{
    public int PId { get; set; }

    public int BId { get; set; }

    public decimal PaidAmount { get; set; }

    public string PaymentType { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string PayStatus { get; set; } = string.Empty;

    public DateTime PayDate { get; set; }

    public Booking? Booking { get; set; }

    public ICollection<Receipt> Receipts { get; set; }
        = new List<Receipt>();
}