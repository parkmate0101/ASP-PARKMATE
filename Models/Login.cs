namespace ParkMate.Models;

public class Login
{
    public int LoginId { get; set; }

    public int UId { get; set; }

    public DateTime LoginTime { get; set; }

    public string LoginType { get; set; } = string.Empty;

    public string LoginStatus { get; set; } = string.Empty;

    public User? User { get; set; }
}