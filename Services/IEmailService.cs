namespace ParkMate.Services
{
    public interface IEmailService
    {
        Task SendOtpAsync(
            string email,
            string name,
            string otp,
            string purpose);
    }
}
