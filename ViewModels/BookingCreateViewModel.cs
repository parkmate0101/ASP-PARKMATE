using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class BookingCreateViewModel
    {
        [Required]
        public string BookingType { get; set; } = "hourly";

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required]
        public TimeSpan StartClock { get; set; } = new TimeSpan(10, 0, 0);

        [Required]
        public TimeSpan EndClock { get; set; } = new TimeSpan(11, 0, 0);

        [Required]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        public string VehicleType { get; set; } = string.Empty;
    }
}