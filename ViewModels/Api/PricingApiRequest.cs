using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels.Api
{
    public class PricingApiRequest
    {
        [Required]
        public string VType { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public int PricePerHour { get; set; }

        [Range(0, 1000000)]
        public int PricePerDay { get; set; }
    }
}
