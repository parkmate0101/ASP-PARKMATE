using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.ViewModels;

namespace ParkMate.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ParkMateDbContext _context;

        public PaymentsController(ParkMateDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var payments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Refund)
                        .ThenInclude(r => r!.RefundReceipt)
                .Include(p => p.Receipts)
                .Where(p =>
                    p.Booking != null &&
                    p.Booking.UId == userId.Value)
                .OrderByDescending(p => p.PayDate)
                .ToListAsync();

            var model = payments.Select(p =>
            {
                var refund = p.Booking?.Refund;

                return new PaymentHistoryViewModel
                {
                    PaymentId = p.PId,
                    BookingId = p.BId,
                    Amount = p.PaidAmount,
                    Mode = p.Mode,
                    Status = p.PayStatus,
                    PayDate = p.PayDate,
                    ReceiptNumber = p.Receipts
                        .Select(r => r.RecNo)
                        .FirstOrDefault(),
                    RefundAmount = refund?.Amount ?? 0m,
                    RefundId = refund?.RefundId,
                    RefundStatus = refund?.RefundStatus,
                    RefundReceiptNumber = refund?.RefundReceipt?.RecNo
                };
            }).ToList();

            return View(model);
        }

        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Admin(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Refund)
                        .ThenInclude(r => r!.RefundReceipt)
                .Include(p => p.Receipts)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p =>
                    p.PayDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime nextDay = toDate.Value.Date.AddDays(1);
                query = query.Where(p => p.PayDate < nextDay);
            }

            var payments = await query
                .OrderByDescending(p => p.PayDate)
                .ToListAsync();

            var model = payments.Select(p =>
            {
                var refund = p.Booking?.Refund;

                return new PaymentHistoryViewModel
                {
                    PaymentId = p.PId,
                    BookingId = p.BId,
                    CustomerName = p.Booking?.User?.Name,
                    CustomerEmail = p.Booking?.User?.Email,
                    Amount = p.PaidAmount,
                    Mode = p.Mode,
                    Status = p.PayStatus,
                    PayDate = p.PayDate,
                    ReceiptNumber = p.Receipts
                        .Select(r => r.RecNo)
                        .FirstOrDefault(),
                    RefundAmount = refund?.Amount ?? 0m,
                    RefundId = refund?.RefundId,
                    RefundStatus = refund?.RefundStatus,
                    RefundReceiptNumber = refund?.RefundReceipt?.RecNo
                };
            }).ToList();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.TotalRevenue = payments
                .Where(p => p.PayStatus == "paid")
                .Sum(p => p.PaidAmount);

            return View(model);
        }
    }
}
