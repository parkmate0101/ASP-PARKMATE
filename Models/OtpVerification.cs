namespace ParkMate.Models;

public class OtpVerification
{
    public int OtpId { get; set; }

    public int UId { get; set; }

    public string OtpCode { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? BlockedUntil { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}