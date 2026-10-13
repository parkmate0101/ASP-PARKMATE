namespace ParkMate.Models;

public class Feedback
{
    public int FId { get; set; }

    public int BId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
}