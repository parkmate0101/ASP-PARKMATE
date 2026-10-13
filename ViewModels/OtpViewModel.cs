using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class OtpViewModel
    {
        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "OTP must be exactly 6 digits.")]
        public string Otp { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
