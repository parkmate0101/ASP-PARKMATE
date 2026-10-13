namespace ParkMate.Models;

public class Slot
{
    public int SlotId { get; set; }

    public string SlotNum { get; set; } = string.Empty;

    public string SlotType { get; set; } = string.Empty;

    public string SlotStatus { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}