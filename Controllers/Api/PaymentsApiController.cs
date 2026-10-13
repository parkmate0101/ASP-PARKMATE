using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;

namespace ParkMate.Controllers.Api
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsApiController : ControllerBase
    {
        private readonly ParkMateDbContext _context;

        public PaymentsApiController(ParkMateDbContext context)
        {
            _context = context;
        }

        // GET: /api/payments/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPayments()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Please login to view your payments."
                });
            }

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Booking)
                .Include(p => p.Receipts)
                .Where(p =>
                    p.Booking != null &&
                    p.Booking.UId == userId.Value)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new
                {
                    paymentId = p.PId,
                    bookingId = p.BId,
                    amount = p.PaidAmount,
                    paymentType = p.PaymentType,
                    mode = p.Mode,
                    status = p.PayStatus,
                    payDate = p.PayDate,
                    receiptNumber = p.Receipts
                        .Select(r => r.RecNo)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                count = payments.Count,
                data = payments
            });
        }

        // GET: /api/payments
        // Admin-only endpoint returning all payments.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Please login as an administrator."
                });
            }

            if (role != 1)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "Administrator access is required."
                });
            }

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.User)
                .Include(p => p.Receipts)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new
                {
                    paymentId = p.PId,
                    bookingId = p.BId,
                    customer = p.Booking != null && p.Booking.User != null
                        ? p.Booking.User.Name
                        : null,
                    email = p.Booking != null && p.Booking.User != null
                        ? p.Booking.User.Email
                        : null,
                    amount = p.PaidAmount,
                    paymentType = p.PaymentType,
                    mode = p.Mode,
                    status = p.PayStatus,
                    payDate = p.PayDate,
                    receiptNumber = p.Receipts
                        .Select(r => r.RecNo)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                count = payments.Count,
                data = payments
            });
        }

        // GET: /api/payments/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Please login to view payment details."
                });
            }

            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.User)
                .Include(p => p.Receipts)
                .Where(p => p.PId == id);

            if (role != 1)
            {
                query = query.Where(p =>
                    p.Booking != null &&
                    p.Booking.UId == userId.Value);
            }

            var payment = await query
                .Select(p => new
                {
                    paymentId = p.PId,
                    bookingId = p.BId,
                    customer = p.Booking != null && p.Booking.User != null
                        ? p.Booking.User.Name
                        : null,
                    email = p.Booking != null && p.Booking.User != null
                        ? p.Booking.User.Email
                        : null,
                    amount = p.PaidAmount,
                    paymentType = p.PaymentType,
                    mode = p.Mode,
                    status = p.PayStatus,
                    payDate = p.PayDate,
                    receiptNumber = p.Receipts
                        .Select(r => r.RecNo)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Payment not found."
                });
            }

            return Ok(new
            {
                success = true,
                data = payment
            });
        }
    }
}
