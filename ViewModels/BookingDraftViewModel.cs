namespace ParkMate.ViewModels
{
    public class BookingDraftViewModel
    {
        public int? UserId { get; set; }

        public string BookingType { get; set; } = "hourly";

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string VehicleNumber { get; set; } = string.Empty;

        public string VehicleType { get; set; } = string.Empty;

        public int SlotId { get; set; }

        public string SlotNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
    }
}
