using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;

namespace ParkMate.Controllers
{
    [AdminOnly]
    public class RefundsController : Controller
    {
        private readonly ParkMateDbContext _context;

        public RefundsController(ParkMateDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CHECK ADMIN LOGIN
        // =========================================================
        private bool IsAdmin()
        {
            int? userRole =
                HttpContext.Session.GetInt32("UserRole");

            return userRole.HasValue &&
                   userRole.Value == 1;
        }

        // =========================================================
        // ADMIN REFUND LIST
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -----------------------------------------------------
            // Only admin can access refunds.
            // -----------------------------------------------------
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            // -----------------------------------------------------
            // Load all refunds with:
            // Refund
            // Booking
            // User
            // Vehicle
            // Slot
            // Refund Receipt
            // -----------------------------------------------------
            var refunds =
                await _context.Refunds
                    .Include(r => r.Booking)
                        .ThenInclude(b => b!.Vehicle)
                    .Include(r => r.Booking)
                        .ThenInclude(b => b!.Slot)
                    .Include(r => r.User)
                    .Include(r => r.RefundReceipt)
                    .OrderByDescending(r => r.RefundId)
                    .ToListAsync();

            return View(refunds);
        }

        // =========================================================
        // MARK REFUND AS PROCESSED
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id)
        {
            // -----------------------------------------------------
            // Admin protection
            // -----------------------------------------------------
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            // -----------------------------------------------------
            // Find refund
            // -----------------------------------------------------
            var refund =
                await _context.Refunds
                    .Include(r => r.RefundReceipt)
                    .FirstOrDefaultAsync(r =>
                        r.RefundId == id);

            if (refund == null)
            {
                TempData["RefundError"] =
                    "Refund record was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            // -----------------------------------------------------
            // Already processed
            // -----------------------------------------------------
            if (refund.RefundStatus == "processed")
            {
                TempData["RefundError"] =
                    "This refund has already been processed.";

                return RedirectToAction(
                    nameof(Index));
            }

            // -----------------------------------------------------
            // Update refund status
            // -----------------------------------------------------
            refund.RefundStatus =
                "processed";

            // -----------------------------------------------------
            // Generate refund receipt
            // -----------------------------------------------------
            if (refund.RefundReceipt == null)
            {
                var refundReceipt =
                    new RefundReceipt
                    {
                        RefundId =
                            refund.RefundId,

                        RecNo =
                            "REF"
                            + DateTime.Now.ToString(
                                "yyyyMMddHHmmssfff")
                            + Random.Shared.Next(
                                100,
                                999),

                        GeneratedAt =
                            DateTime.Now
                    };

                _context.RefundReceipts.Add(
                    refundReceipt);
            }

            await _context.SaveChangesAsync();

            TempData["RefundSuccess"] =
                "Refund processed successfully and refund receipt generated.";

            return RedirectToAction(
                nameof(Index));
        }
    }
}