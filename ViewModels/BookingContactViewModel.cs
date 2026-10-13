using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class BookingContactViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Mobile number must be exactly 10 digits."
        )]
        public string Mobile { get; set; } = string.Empty;
    }
}