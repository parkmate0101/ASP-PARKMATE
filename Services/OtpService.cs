using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Models;

namespace ParkMate.Services
{
    public class OtpService
    {
        private readonly ParkMateDbContext _context;
        private readonly IEmailService _emailService;

        public OtpService(
            ParkMateDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendOtpAsync(User user, string purpose)
        {
            DateTime now = DateTime.Now;

            var recentOtp = await _context.OtpVerifications
                .Where(o =>
                    o.UId == user.UId &&
                    o.Purpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentOtp != null &&
                (now - recentOtp.CreatedAt).TotalSeconds < 30)
            {
                throw new InvalidOperationException(
                    "Please wait 30 seconds before requesting another OTP.");
            }

            await _context.OtpVerifications
                .Where(o =>
                    o.UId == user.UId &&
                    o.Purpose == purpose &&
                    !o.IsUsed)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(o => o.IsUsed, true));

            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var record = new OtpVerification
            {
                UId = user.UId,
                OtpCode = otp,
                Purpose = purpose,
                ExpiresAt = now.AddMinutes(5),
                IsUsed = false,
                AttemptCount = 0,
                BlockedUntil = null,
                CreatedAt = now
            };

            _context.OtpVerifications.Add(record);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpAsync(
                    user.Email,
                    user.Name,
                    otp,
                    purpose);
            }
            catch
            {
                record.IsUsed = true;
                await _context.SaveChangesAsync();
                throw;
            }
        }
    }
}
