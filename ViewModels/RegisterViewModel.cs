using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Name must be between 3 and 50 characters.")]
        [RegularExpression(
            @"^[A-Za-z ]+$",
            ErrorMessage = "Only letters and spaces are allowed.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Phone must be exactly 10 digits.")]
        public string Contact { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
