using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Name must be between 3 and 50 characters.")]
        [RegularExpression(
            @"^[A-Za-z ]+$",
            ErrorMessage = "Name can contain only letters and spaces.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(
            @"^[0-9]{10}$",
            ErrorMessage = "Contact must be exactly 10 digits.")]
        public string Contact { get; set; } = string.Empty;
    }
}