namespace ParkMate.Models;

public class Vehicle
{
    public int VId { get; set; }

    public int UId { get; set; }

    public string VNum { get; set; } = string.Empty;

    public string VType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}