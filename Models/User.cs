namespace ParkMate.Models;

public class User
{
    public int UId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Contact { get; set; }

    public string Password { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public Role? Role { get; set; }

    public ICollection<OtpVerification> OtpVerifications { get; set; }
        = new List<OtpVerification>();

    public ICollection<Login> Logins { get; set; }
        = new List<Login>();

    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();

    public ICollection<Refund> Refunds { get; set; }
        = new List<Refund>();
}