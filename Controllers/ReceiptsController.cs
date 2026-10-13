using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Services;

namespace ParkMate.Controllers
{
    public class ReceiptsController : Controller
    {
        private readonly ParkMateDbContext _context;
        private readonly SimplePdfService _pdfService;
        private readonly ReceiptAccessService _receiptAccessService;

        public ReceiptsController(
            ParkMateDbContext context,
            SimplePdfService pdfService,
            ReceiptAccessService receiptAccessService)
        {
            _context = context;
            _pdfService = pdfService;
            _receiptAccessService = receiptAccessService;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPayment(
            int id,
            string? token = null)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            // Normally the receipt requires the logged-in session.
            // PaymentSuccess additionally provides a short-lived token so
            // the immediate download does not ask the user to login again.
            // Validate the token once here when the request is anonymous.
            if (!userId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(token) ||
                    !_receiptAccessService.TryGetPaymentId(
                        token,
                        out int tokenPaymentId) ||
                    tokenPaymentId != id)
                {
                    return RedirectToAction("Login", "Auth");
                }
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Vehicle)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Slot)
                .Include(p => p.Receipts)
                .FirstOrDefaultAsync(p => p.PId == id);

            if (payment?.Booking == null)
            {
                return NotFound("Payment receipt was not found.");
            }

            // Logged-in normal users may download only their own receipt.
            // Admin can download any receipt.
            if (userId.HasValue && role != 1 &&
                payment.Booking.UId != userId.Value)
            {
                return Forbid();
            }

            // For anonymous requests, the token was already validated
            // against the requested payment ID above.

            var receipt = payment.Receipts.FirstOrDefault();

            if (receipt == null || string.IsNullOrWhiteSpace(receipt.RecNo))
            {
                return NotFound(
                    "Receipt has not been generated for this payment.");
            }

            int paymentDisplayId = await _context.Payments
                .CountAsync(p => p.PId <= payment.PId);

            int bookingDisplayId = await _context.Bookings
                .CountAsync(b => b.BId <= payment.Booking.BId);

            var lines = new List<string>
            {
                $"Receipt No: {receipt.RecNo}",
                $"Payment ID: #P{paymentDisplayId}",
                $"Booking ID: #B{bookingDisplayId}",
                $"Customer: {payment.Booking.User?.Name ?? "N/A"}",
                $"Email: {payment.Booking.User?.Email ?? "N/A"}",
                $"Vehicle: {payment.Booking.Vehicle?.VNum ?? "N/A"}",
                $"Parking Slot: {payment.Booking.Slot?.SlotNum ?? "N/A"}",
                $"Booking Time: {payment.Booking.StartTime:dd MMM yyyy, hh:mm tt} - {payment.Booking.EndTime:dd MMM yyyy, hh:mm tt}",
                $"Amount: INR {payment.PaidAmount:0.00}",
                $"Payment Mode: {payment.Mode.ToUpperInvariant()}",
                $"Payment Status: {payment.PayStatus}",
                $"Generated: {receipt.GeneratedAt:dd MMM yyyy, hh:mm tt}",
                "Thank you for using ParkMate."
            };

            byte[] pdf = _pdfService.BuildReceiptPdf(lines);

            return File(
                pdf,
                "application/pdf",
                $"ParkMate_Receipt_{receipt.RecNo}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadRefund(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var refund = await _context.Refunds
                .Include(r => r.Booking)
                    .ThenInclude(b => b!.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b!.Vehicle)
                .Include(r => r.Booking)
                    .ThenInclude(b => b!.Slot)
                .Include(r => r.RefundReceipt)
                .FirstOrDefaultAsync(r => r.RefundId == id);

            if (refund?.Booking == null || refund.RefundReceipt == null)
            {
                return NotFound("Refund receipt was not found.");
            }

            if (role != 1 && refund.UId != userId.Value)
            {
                return Forbid();
            }

            int refundDisplayId = await _context.Refunds
                .CountAsync(r => r.RefundId <= refund.RefundId);

            int bookingDisplayId = await _context.Bookings
                .CountAsync(b => b.BId <= refund.Booking.BId);

            var lines = new List<string>
            {
                $"Refund Receipt No: {refund.RefundReceipt.RecNo}",
                $"Refund ID: #R{refundDisplayId}",
                $"Booking ID: #B{bookingDisplayId}",
                $"Customer: {refund.Booking.User?.Name ?? "N/A"}",
                $"Email: {refund.Booking.User?.Email ?? "N/A"}",
                $"Vehicle: {refund.Booking.Vehicle?.VNum ?? "N/A"}",
                $"Parking Slot: {refund.Booking.Slot?.SlotNum ?? "N/A"}",
                $"Refund Amount: INR {refund.Amount:0.00}",
                $"Refund Status: {refund.RefundStatus}",
                $"Generated: {refund.RefundReceipt.GeneratedAt:dd MMM yyyy, hh:mm tt}",
                "ParkMate refund receipt"
            };

            byte[] pdf = _pdfService.BuildReceiptPdf(lines);

            return File(
                pdf,
                "application/pdf",
                $"ParkMate_Refund_{refund.RefundReceipt.RecNo}.pdf");
        }
    }
}
