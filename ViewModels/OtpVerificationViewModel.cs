using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class OtpVerificationViewModel
    {
        [Required]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
        public string Otp { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = "register";
    }
}
