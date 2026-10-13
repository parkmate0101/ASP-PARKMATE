using System.ComponentModel.DataAnnotations;

namespace ParkMate.ViewModels.Api
{
    public class SlotApiRequest
    {
        [Required]
        [StringLength(20)]
        public string SlotNum { get; set; } = string.Empty;

        [Required]
        public string SlotType { get; set; } = string.Empty;

        [Required]
        public string SlotStatus { get; set; } = "Available";
    }
}
