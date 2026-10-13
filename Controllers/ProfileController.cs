using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.ViewModels;

namespace ParkMate.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ParkMateDbContext _context;

        public ProfileController(ParkMateDbContext context)
        {
            _context = context;
        }

        private int? GetLoggedInUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // =========================================================
        // PROFILE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = GetLoggedInUserId();

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UId == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Contact = user.Contact ?? string.Empty
            };

            return View(model);
        }

        // =========================================================
        // PROFILE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            int? userId = GetLoggedInUserId();

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UId == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // Check duplicate contact
            bool contactExists = await _context.Users
                .AnyAsync(u =>
                    u.Contact == model.Contact &&
                    u.UId != userId.Value);

            if (contactExists)
            {
                ModelState.AddModelError(
                    "Contact",
                    "This contact number is already registered.");

                return View(model);
            }

            user.Name = model.Name.Trim();
            user.Contact = model.Contact.Trim();

            await _context.SaveChangesAsync();

            // Update session name
            HttpContext.Session.SetString(
                "UserName",
                user.Name);

            TempData["ProfileSuccess"] =
                "Profile updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}