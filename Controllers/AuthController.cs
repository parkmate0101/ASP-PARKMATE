using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Models;
using ParkMate.Services;
using ParkMate.ViewModels;

namespace ParkMate.Controllers
{
    public class AuthController : Controller
    {
        private readonly ParkMateDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly OtpService _otpService;

        private const string OtpUserIdKey = "OtpUserId";
        private const string OtpPurposeKey = "OtpPurpose";
        private const string OtpReturnUrlKey = "OtpReturnUrl";
        private const string PasswordSetupUserIdKey = "PasswordSetupUserId";
        private const string PasswordSetupModeKey = "PasswordSetupMode";
        private const string PasswordSetupReturnUrlKey = "PasswordSetupReturnUrl";

        public AuthController(
            ParkMateDbContext context,
            OtpService otpService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Email = (model.Email ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            if (!user.IsVerified)
            {
                ModelState.AddModelError(
                    "",
                    "Please verify your email with OTP before logging in.");

                return View(model);
            }

            // Blank password = OTP login for normal users.
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                if (user.RoleId == 1)
                {
                    ModelState.AddModelError(
                        "",
                        "Administrator login requires a password.");

                    return View(model);
                }

                try
                {
                    await _otpService.SendOtpAsync(user, "login");

                    HttpContext.Session.SetInt32(
                        OtpUserIdKey,
                        user.UId);

                    HttpContext.Session.SetString(
                        OtpPurposeKey,
                        "login");

                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                        Url.IsLocalUrl(model.ReturnUrl))
                    {
                        HttpContext.Session.SetString(
                            OtpReturnUrlKey,
                            model.ReturnUrl);
                    }
                    else
                    {
                        HttpContext.Session.Remove(OtpReturnUrlKey);
                    }

                    TempData["OtpMessage"] =
                        "A 6-digit login OTP has been sent to your email.";

                    return RedirectToAction(
                        "Verify",
                        "Otp");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }

            // User does not have a password yet.
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(
                    "",
                    "This account uses OTP login. Leave password blank to receive an OTP.");

                return View(model);
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    model.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                await SaveLoginRecord(
                    user.UId,
                    "password",
                    "failed");

                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            await SaveLoginRecord(
                user.UId,
                "password",
                "success");

            SetUserSession(user);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return user.RoleId == 1
                ? RedirectToAction("Admin", "Dashboard")
                : RedirectToAction("User", "Dashboard");
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            return View(new RegisterViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            model.Name = (model.Name ?? string.Empty).Trim();
            model.Contact = (model.Contact ?? string.Empty).Trim();
            model.Email = (model.Email ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already registered.");

                return View(model);
            }

            bool contactExists = await _context.Users
                .AnyAsync(u => u.Contact == model.Contact);

            if (contactExists)
            {
                ModelState.AddModelError(
                    "Contact",
                    "This contact number is already registered.");

                return View(model);
            }

            // Old ParkMate registration stores the account first,
            // verifies it through OTP, and sets the password later.
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Contact = model.Contact,
                Password = string.Empty,
                RoleId = 2,
                IsVerified = false,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                await _otpService.SendOtpAsync(user, "register");
            }
            catch (Exception ex)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            HttpContext.Session.SetInt32(
                OtpUserIdKey,
                user.UId);

            HttpContext.Session.SetString(
                OtpPurposeKey,
                "register");

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                HttpContext.Session.SetString(
                    OtpReturnUrlKey,
                    model.ReturnUrl);
            }
            else
            {
                HttpContext.Session.Remove(OtpReturnUrlKey);
            }

            TempData["OtpMessage"] =
                "A 6-digit verification OTP has been sent to your email.";

            return RedirectToAction(
                "Verify",
                "Otp");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            model.Email = (model.Email ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Email is not registered.");

                return View(model);
            }

            try
            {
                await _otpService.SendOtpAsync(user, "reset");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            HttpContext.Session.SetInt32(
                OtpUserIdKey,
                user.UId);

            HttpContext.Session.SetString(
                OtpPurposeKey,
                "reset");

            HttpContext.Session.Remove(OtpReturnUrlKey);

            TempData["OtpMessage"] =
                "A 6-digit password reset OTP has been sent to your email.";

            return RedirectToAction(
                "Verify",
                "Otp");
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            int? userId = GetPasswordSetupUserId(HttpContext);
            string? mode = GetPasswordSetupMode(HttpContext);

            if (!userId.HasValue || string.IsNullOrWhiteSpace(mode))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                ClearPasswordSetupSession(HttpContext);
                return RedirectToAction(nameof(Login));
            }

            return View(new SetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(
            SetPasswordViewModel model)
        {
            int? userId = GetPasswordSetupUserId(HttpContext);
            string? mode = GetPasswordSetupMode(HttpContext);

            if (!userId.HasValue || string.IsNullOrWhiteSpace(mode))
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                ClearPasswordSetupSession(HttpContext);
                return RedirectToAction(nameof(Login));
            }

            user.Password = _passwordHasher.HashPassword(
                user,
                model.Password);

            if (mode == "register")
            {
                user.IsVerified = true;
            }

            await _context.SaveChangesAsync();

            string? returnUrl =
                GetPasswordSetupReturnUrl(HttpContext);

            // Register/login flows leave the user signed in.
            if (mode == "register" || mode == "login")
            {
                SetUserSession(user);
            }

            ClearPasswordSetupSession(HttpContext);

            if (mode == "reset")
            {
                TempData["SuccessMessage"] =
                    "Password reset successfully. Please login with your new password.";

                return RedirectToAction(nameof(Login));
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return user.RoleId == 1
                ? RedirectToAction("Admin", "Dashboard")
                : RedirectToAction("User", "Dashboard");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private void SetUserSession(User user)
        {
            HttpContext.Session.SetInt32(
                "UserId",
                user.UId);

            HttpContext.Session.SetString(
                "UserName",
                user.Name ?? string.Empty);

            HttpContext.Session.SetInt32(
                "UserRole",
                user.RoleId);
        }

        public static void SetPasswordSetupSession(
            HttpContext httpContext,
            int userId,
            string mode,
            string? returnUrl)
        {
            httpContext.Session.SetInt32(
                PasswordSetupUserIdKey,
                userId);

            httpContext.Session.SetString(
                PasswordSetupModeKey,
                mode);

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                httpContext.Session.SetString(
                    PasswordSetupReturnUrlKey,
                    returnUrl);
            }
            else
            {
                httpContext.Session.Remove(
                    PasswordSetupReturnUrlKey);
            }
        }

        public static void ClearOtpSession(HttpContext httpContext)
        {
            httpContext.Session.Remove(OtpUserIdKey);
            httpContext.Session.Remove(OtpPurposeKey);
            httpContext.Session.Remove(OtpReturnUrlKey);
        }

        public static void ClearPasswordSetupSession(HttpContext httpContext)
        {
            httpContext.Session.Remove(PasswordSetupUserIdKey);
            httpContext.Session.Remove(PasswordSetupModeKey);
            httpContext.Session.Remove(PasswordSetupReturnUrlKey);
        }

        public static int? GetOtpUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32(OtpUserIdKey);
        }

        public static string? GetOtpPurpose(HttpContext httpContext)
        {
            return httpContext.Session.GetString(OtpPurposeKey);
        }

        public static string? GetOtpReturnUrl(HttpContext httpContext)
        {
            return httpContext.Session.GetString(OtpReturnUrlKey);
        }

        public static int? GetPasswordSetupUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32(PasswordSetupUserIdKey);
        }

        public static string? GetPasswordSetupMode(HttpContext httpContext)
        {
            return httpContext.Session.GetString(PasswordSetupModeKey);
        }

        public static string? GetPasswordSetupReturnUrl(HttpContext httpContext)
        {
            return httpContext.Session.GetString(PasswordSetupReturnUrlKey);
        }

        private async Task SaveLoginRecord(
            int userId,
            string loginType,
            string loginStatus)
        {
            var login = new Login
            {
                UId = userId,
                LoginTime = DateTime.Now,
                LoginType = loginType,
                LoginStatus = loginStatus
            };

            _context.Logins.Add(login);
            await _context.SaveChangesAsync();
        }
    }
}
