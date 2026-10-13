using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Models;
using ParkMate.Services;
using ParkMate.ViewModels;

namespace ParkMate.Controllers
{
    public class OtpController : Controller
    {
        private readonly ParkMateDbContext _context;
        private readonly OtpService _otpService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<OtpController> _logger;
        private readonly IWebHostEnvironment _environment;

        public OtpController(
            ParkMateDbContext context,
            OtpService otpService,
            ILogger<OtpController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _otpService = otpService;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Verify()
        {
            int? userId = AuthController.GetOtpUserId(HttpContext);
            string? purpose = AuthController.GetOtpPurpose(HttpContext);

            if (!userId.HasValue || string.IsNullOrWhiteSpace(purpose))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                AuthController.ClearOtpSession(HttpContext);
                return RedirectToAction("Register", "Auth");
            }

            return View("~/Views/Auth/VerifyOtp.cshtml", new OtpViewModel
            {
                Purpose = purpose,
                Email = MaskEmail(user.Email),
                ReturnUrl = AuthController.GetOtpReturnUrl(HttpContext)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(OtpViewModel model)
        {
            int? userId = AuthController.GetOtpUserId(HttpContext);
            string? purpose = AuthController.GetOtpPurpose(HttpContext);

            if (!userId.HasValue || string.IsNullOrWhiteSpace(purpose))
            {
                return RedirectToAction("Login", "Auth");
            }

            model.Purpose = purpose;
            model.Otp = (model.Otp ?? string.Empty).Trim();

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                AuthController.ClearOtpSession(HttpContext);
                ModelState.AddModelError("", "Account not found. Please register again.");
                return View("~/Views/Auth/VerifyOtp.cshtml", model);
            }

            model.Email = MaskEmail(user.Email);
            model.ReturnUrl = AuthController.GetOtpReturnUrl(HttpContext);

            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/VerifyOtp.cshtml", model);
            }

            try
            {
                var otp = await _context.OtpVerifications
                    .Where(o =>
                        o.UId == user.UId &&
                        o.Purpose == purpose &&
                        !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null || otp.ExpiresAt <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        "",
                        "OTP has expired or is invalid. Please request a new OTP.");
                    return View("~/Views/Auth/VerifyOtp.cshtml", model);
                }

                if (otp.BlockedUntil.HasValue &&
                    otp.BlockedUntil.Value > DateTime.Now)
                {
                    ModelState.AddModelError(
                        "",
                        "Too many wrong attempts. Please try again after the temporary block expires.");
                    return View("~/Views/Auth/VerifyOtp.cshtml", model);
                }

                if (!string.Equals(otp.OtpCode, model.Otp, StringComparison.Ordinal))
                {
                    otp.AttemptCount++;

                    if (otp.AttemptCount >= 5)
                    {
                        otp.BlockedUntil = DateTime.Now.AddMinutes(15);
                    }

                    await _context.SaveChangesAsync();

                    string message = otp.AttemptCount >= 5
                        ? "Too many wrong attempts. OTP is blocked for 15 minutes."
                        : $"Wrong OTP. Attempts left: {5 - otp.AttemptCount}.";

                    ModelState.AddModelError("", message);
                    return View("~/Views/Auth/VerifyOtp.cshtml", model);
                }

                // Keep OTP verification and login-record creation atomic.
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                otp.IsUsed = true;

                if (purpose == "register")
                {
                    user.IsVerified = true;
                }

                await _context.SaveChangesAsync();

                if (purpose == "register" || purpose == "login")
                {
                    _context.Logins.Add(new Login
                    {
                        UId = user.UId,
                        LoginTime = DateTime.Now,
                        LoginType = "otp",
                        LoginStatus = "success"
                    });

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                string? returnUrl = AuthController.GetOtpReturnUrl(HttpContext);

                // Do not clear ReturnUrl before reading it.
                AuthController.ClearOtpSession(HttpContext);

                if (purpose == "register")
                {
                    AuthController.SetPasswordSetupSession(
                        HttpContext,
                        user.UId,
                        "register",
                        returnUrl);

                    return RedirectToAction(
                        "SetPassword",
                        "Auth");
                }

                if (purpose == "reset")
                {
                    AuthController.SetPasswordSetupSession(
                        HttpContext,
                        user.UId,
                        "reset",
                        null);

                    return RedirectToAction(
                        "SetPassword",
                        "Auth");
                }

                // OTP login: only go to Set Password when the account does
                // not have one. Otherwise sign the user in immediately.
                SetUserSession(user);

                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return user.RoleId == 1
                    ? RedirectToAction("Admin", "Dashboard")
                    : RedirectToAction("User", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OTP verification failed for user {UserId}, purpose {Purpose}",
                    user.UId,
                    purpose);

                string message = _environment.IsDevelopment()
                    ? $"OTP verification failed: {ex.Message}"
                    : "OTP verification failed. Please request a new OTP and try again.";

                ModelState.AddModelError("", message);
                return View("~/Views/Auth/VerifyOtp.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend()
        {
            int? userId = AuthController.GetOtpUserId(HttpContext);
            string? purpose = AuthController.GetOtpPurpose(HttpContext);

            if (!userId.HasValue || string.IsNullOrWhiteSpace(purpose))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                AuthController.ClearOtpSession(HttpContext);
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _otpService.SendOtpAsync(user, purpose);
                TempData["OtpMessage"] = "A new OTP has been sent to your email.";
            }
            catch (Exception ex)
            {
                TempData["OtpError"] = ex.Message;
            }

            return RedirectToAction(nameof(Verify));
        }

        private void SetUserSession(User user)
        {
            HttpContext.Session.SetInt32("UserId", user.UId);
            HttpContext.Session.SetString("UserName", user.Name ?? string.Empty);
            HttpContext.Session.SetInt32("UserRole", user.RoleId);
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            int atIndex = email.IndexOf('@');

            if (atIndex <= 1)
            {
                return email;
            }

            string local = email[..atIndex];
            string domain = email[atIndex..];

            string masked =
                local[0] +
                new string('*', Math.Max(1, local.Length - 2)) +
                local[^1];

            return masked + domain;
        }
    }
}
