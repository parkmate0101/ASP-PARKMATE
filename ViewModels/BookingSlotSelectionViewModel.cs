namespace ParkMate.ViewModels
{
    public class BookingSlotSelectionViewModel
    {
        public BookingDraftViewModel Draft { get; set; }
            = new BookingDraftViewModel();

        public List<BookingSlotOptionViewModel> Slots { get; set; }
            = new List<BookingSlotOptionViewModel>();

        public int SelectedSlotId { get; set; }
    }

    public class BookingSlotOptionViewModel
    {
        public int SlotId { get; set; }

        public string SlotNumber { get; set; } = string.Empty;

        public string SlotType { get; set; } = string.Empty;

        public bool IsBooked { get; set; }

        public DateTime? NextAvailable { get; set; }
    }
}