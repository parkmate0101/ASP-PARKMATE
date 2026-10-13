namespace ParkMate.ViewModels
{
    public class PaymentHistoryViewModel
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public string Mode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PayDate { get; set; }
        public string? ReceiptNumber { get; set; }
        public decimal RefundAmount { get; set; }
        public int? RefundId { get; set; }
        public string? RefundStatus { get; set; }
        public string? RefundReceiptNumber { get; set; }
    }
}
