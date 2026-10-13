using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace ParkMate.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public BrevoEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpAsync(
            string email,
            string name,
            string otp,
            string purpose)
        {
            string host = _configuration["EmailSettings:SmtpHost"] ?? string.Empty;
            int port = int.TryParse(
                _configuration["EmailSettings:SmtpPort"],
                out int parsedPort)
                ? parsedPort
                : 587;

            string username = _configuration["EmailSettings:SmtpUsername"] ?? string.Empty;
            string password = _configuration["EmailSettings:SmtpPassword"] ?? string.Empty;
            string fromEmail = _configuration["EmailSettings:FromEmail"] ?? string.Empty;
            string fromName = _configuration["EmailSettings:FromName"] ?? "ParkMate";

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                username.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) ||
                password.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) ||
                fromEmail.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Brevo SMTP is not configured. Open appsettings.Development.json and enter your real Brevo SMTP credentials and verified sender email.");
            }

            string safeName = HtmlEncoder.Default.Encode(
                string.IsNullOrWhiteSpace(name) ? "there" : name);

            string purposeText = purpose switch
            {
                "register" => "email verification",
                "login" => "login",
                "reset" => "password reset",
                _ => "verification"
            };

            string subject = purpose switch
            {
                "register" => "ParkMate - Verify your email",
                "login" => "ParkMate - Login OTP",
                "reset" => "ParkMate - Password reset OTP",
                _ => "ParkMate - Verification OTP"
            };

            string body = $"""
                <html>
                <body style="font-family:Arial,sans-serif;line-height:1.6;color:#1f2937;">
                    <h2 style="color:#5149F6;">ParkMate</h2>
                    <p>Hello {safeName},</p>
                    <p>Your OTP for {purposeText} is:</p>
                    <p style="font-size:30px;font-weight:bold;letter-spacing:8px;">{otp}</p>
                    <p>This OTP is valid for 5 minutes and can be used only once.</p>
                    <p>If you did not request this, you can safely ignore this email.</p>
                    <p>Regards,<br />ParkMate Team</p>
                </body>
                </html>
                """;

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(email);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            await client.SendMailAsync(message);
        }
    }
}
