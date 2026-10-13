using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels.Api
{
    public class BookingStatusApiRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
