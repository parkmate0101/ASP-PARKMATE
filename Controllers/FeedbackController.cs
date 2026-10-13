using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;

namespace ParkMate.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ParkMateDbContext _context;

        public FeedbackController(ParkMateDbContext context)
        {
            _context = context;
        }

        private int? GetLoggedInUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // =========================================================
        // USER FEEDBACK
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = GetLoggedInUserId();

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var expiredBookings = await _context.Bookings
                .Where(b =>
                    b.UId == userId.Value &&
                    b.BStatus == "Active" &&
                    b.EndTime <= DateTime.Now)
                .ToListAsync();

            foreach (var booking in expiredBookings)
            {
                booking.BStatus = "Completed";
            }

            if (expiredBookings.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var bookings = await _context.Bookings
                .Where(b =>
                    b.UId == userId.Value &&
                    b.BStatus == "Completed" &&
                    !_context.Feedbacks.Any(f =>
                        f.BId == b.BId))
                .OrderByDescending(b => b.EndTime)
                .ToListAsync();

            return View(bookings);
        }

        // =========================================================
        // SUBMIT FEEDBACK
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int bookingId,
            byte rating,
            string comment)
        {
            int? userId = GetLoggedInUserId();

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            comment = (comment ?? string.Empty).Trim();

            if (rating < 1 || rating > 5)
            {
                TempData["FeedbackError"] =
                    "Please select a rating between 1 and 5.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["FeedbackError"] =
                    "Please enter your feedback.";

                return RedirectToAction(nameof(Index));
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.BId == bookingId &&
                    b.UId == userId.Value &&
                    b.BStatus == "Completed");

            if (booking == null)
            {
                TempData["FeedbackError"] =
                    "Invalid booking.";

                return RedirectToAction(nameof(Index));
            }

            bool alreadySubmitted =
                await _context.Feedbacks
                    .AnyAsync(f => f.BId == bookingId);

            if (alreadySubmitted)
            {
                TempData["FeedbackError"] =
                    "Feedback has already been submitted for this booking.";

                return RedirectToAction(nameof(Index));
            }

            var feedback = new Feedback
            {
                BId = bookingId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);

            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] =
                "Thank you for your feedback!";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // ADMIN FEEDBACK
        // =========================================================
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Admin()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Booking)
                    .ThenInclude(b => b!.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedbacks);
        }

        // =========================================================
        // ADMIN DELETE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FId == id);

            if (feedback == null)
            {
                TempData["FeedbackError"] =
                    "Feedback was not found.";

                return RedirectToAction(nameof(Admin));
            }

            _context.Feedbacks.Remove(feedback);

            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] =
                "Feedback deleted successfully.";

            return RedirectToAction(nameof(Admin));
        }
    }
}