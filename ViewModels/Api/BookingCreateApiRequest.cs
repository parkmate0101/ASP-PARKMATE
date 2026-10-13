using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels.Api
{
    public class BookingCreateApiRequest
    {
        [Required]
        public int SlotId { get; set; }

        [Required]
        [StringLength(20)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        public string VehicleType { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string PaymentMode { get; set; } = "upi";
    }
}
