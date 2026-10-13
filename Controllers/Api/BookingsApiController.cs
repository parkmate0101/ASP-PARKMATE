using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;
using ParkMate.ViewModels.Api;

namespace ParkMate.Controllers.Api
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsApiController : ControllerBase
    {
        private readonly ParkMateDbContext _context;

        public BookingsApiController(ParkMateDbContext context)
        {
            _context = context;
        }

        // GET: /api/bookings/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "Please login to view your bookings." });

            var bookings = await BookingQuery()
                .Where(b => b.UId == userId.Value)
                .OrderByDescending(b => b.BId)
                .Select(ProjectBooking())
                .ToListAsync();

            return Ok(new { success = true, count = bookings.Count, data = bookings });
        }

        // GET: /api/bookings
        // Admin-only list of all bookings.
        [HttpGet]
        [ApiAdminOnly]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await BookingQuery()
                .OrderByDescending(b => b.BId)
                .Select(ProjectBooking())
                .ToListAsync();

            return Ok(new { success = true, count = bookings.Count, data = bookings });
        }

        // GET: /api/bookings/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "Please login to view booking details." });

            var query = BookingQuery().Where(b => b.BId == id);

            if (role != 1)
                query = query.Where(b => b.UId == userId.Value);

            var booking = await query
                .Select(ProjectBooking())
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound(new { success = false, message = "Booking not found." });

            return Ok(new { success = true, data = booking });
        }

        // POST: /api/bookings
        // Creates a booking and its payment/receipt for the logged-in user.
        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateApiRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "Please login before creating a booking." });

            string vehicleNumber = (request.VehicleNumber ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (!Regex.IsMatch(vehicleNumber, @"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$"))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Enter a valid vehicle number, e.g. GJ01AB1234."
                });
            }

            if (request.VehicleType != "2_wheeler" && request.VehicleType != "4_wheeler")
                return BadRequest(new { success = false, message = "VehicleType must be 2_wheeler or 4_wheeler." });

            if (request.StartTime <= DateTime.Now)
                return BadRequest(new { success = false, message = "Booking start time must be in the future." });

            if (request.EndTime <= request.StartTime)
                return BadRequest(new { success = false, message = "EndTime must be greater than StartTime." });

            string paymentMode = request.PaymentMode?.ToLowerInvariant() == "card" ? "card" : "upi";

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var slot = await _context.Slots.FirstOrDefaultAsync(s => s.SlotId == request.SlotId);

                if (slot == null)
                    return NotFound(new { success = false, message = "Parking slot not found." });

                if (slot.SlotStatus == "Blocked")
                    return Conflict(new { success = false, message = "Selected parking slot is blocked." });

                if (slot.SlotType != request.VehicleType)
                    return BadRequest(new { success = false, message = "Selected slot does not match the vehicle type." });

                bool conflict = await _context.Bookings.AnyAsync(b =>
                    b.SlotId == request.SlotId &&
                    b.BStatus == "Active" &&
                    request.StartTime < b.EndTime &&
                    request.EndTime > b.StartTime);

                if (conflict)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "This slot is already booked for the selected time."
                    });
                }

                var pricing = await _context.Pricings
                    .FirstOrDefaultAsync(p => p.VType == request.VehicleType);

                if (pricing == null)
                    return Conflict(new { success = false, message = "Pricing is not configured for this vehicle type." });

                double totalHours = (request.EndTime - request.StartTime).TotalHours;
                decimal totalAmount;

                if (totalHours >= 24)
                {
                    int days = (int)Math.Ceiling(totalHours / 24d);
                    totalAmount = days * pricing.PricePerDay;
                }
                else
                {
                    totalAmount = (decimal)Math.Ceiling(totalHours) * pricing.PricePerHour;
                }

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.UId == userId.Value && v.VNum == vehicleNumber);

                if (vehicle == null)
                {
                    bool vehicleUsedByAnotherUser = await _context.Vehicles
                        .AnyAsync(v => v.VNum == vehicleNumber && v.UId != userId.Value);

                    if (vehicleUsedByAnotherUser)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "This vehicle number is already registered to another user."
                        });
                    }

                    vehicle = new Vehicle
                    {
                        UId = userId.Value,
                        VNum = vehicleNumber,
                        VType = request.VehicleType,
                        CreatedAt = DateTime.Now
                    };

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                }
                else if (vehicle.VType != request.VehicleType)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vehicle type does not match the existing vehicle."
                    });
                }

                var booking = new Booking
                {
                    UId = userId.Value,
                    VId = vehicle.VId,
                    VType = request.VehicleType,
                    SlotId = request.SlotId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TotalAmount = totalAmount,
                    PaymentStatus = "paid",
                    BStatus = "Active",
                    BookingDate = DateTime.Now
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var payment = new Payment
                {
                    BId = booking.BId,
                    PaidAmount = totalAmount,
                    PaymentType = "advance",
                    Mode = paymentMode,
                    PayStatus = "paid",
                    PayDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                var receipt = new Receipt
                {
                    PId = payment.PId,
                    RecNo = "REC" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Random.Shared.Next(100, 999),
                    GeneratedAt = DateTime.Now
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = booking.BId }, new
                {
                    success = true,
                    message = "Booking created successfully.",
                    data = new
                    {
                        bookingId = booking.BId,
                        slotId = booking.SlotId,
                        slotNumber = slot.SlotNum,
                        vehicleNumber = vehicle.VNum,
                        vehicleType = booking.VType,
                        startTime = booking.StartTime,
                        endTime = booking.EndTime,
                        totalAmount = booking.TotalAmount,
                        paymentStatus = booking.PaymentStatus,
                        bookingStatus = booking.BStatus,
                        paymentId = payment.PId,
                        receiptNumber = receipt.RecNo
                    }
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: /api/bookings/1/status
        // Admin-only status management.
        [HttpPut("{id:int}/status")]
        [ApiAdminOnly]
        public async Task<IActionResult> UpdateStatus(int id, BookingStatusApiRequest request)
        {
            if (request.Status != "Active" &&
                request.Status != "Completed" &&
                request.Status != "Cancelled")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Status must be Active, Completed, or Cancelled."
                });
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { success = false, message = "Booking not found." });

            booking.BStatus = request.Status;

            if (request.Status == "Cancelled")
                booking.CancelledAt ??= DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Booking status updated successfully.",
                bookingId = booking.BId,
                status = booking.BStatus
            });
        }

        // DELETE: /api/bookings/1
        // User cancellation. Refund rules match the MVC booking flow.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "Please login to cancel a booking." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b =>
                        b.BId == id &&
                        b.UId == userId.Value &&
                        b.BStatus == "Active");

                if (booking == null)
                    return NotFound(new { success = false, message = "Active booking not found." });

                if (DateTime.Now >= booking.StartTime)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Booking cannot be cancelled after the start time."
                    });
                }

                double hoursLeft = (booking.StartTime - DateTime.Now).TotalHours;
                decimal refundAmount = hoursLeft > 24
                    ? booking.TotalAmount
                    : hoursLeft > 12
                        ? booking.TotalAmount * 0.5m
                        : 0m;

                booking.BStatus = "Cancelled";
                booking.CancelledAt = DateTime.Now;
                booking.CancelReason = "User cancelled through API";
                booking.RefundAmount = refundAmount;

                if (refundAmount > 0)
                {
                    bool refundExists = await _context.Refunds.AnyAsync(r => r.BId == booking.BId);

                    if (!refundExists)
                    {
                        _context.Refunds.Add(new Refund
                        {
                            BId = booking.BId,
                            UId = userId.Value,
                            Amount = refundAmount,
                            RefundStatus = "pending",
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = refundAmount > 0
                        ? $"Booking cancelled. Refund of ₹{refundAmount:0.00} is pending."
                        : "Booking cancelled. No refund is applicable.",
                    bookingId = booking.BId,
                    status = booking.BStatus,
                    refundAmount
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private IQueryable<Booking> BookingQuery()
        {
            return _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Slot)
                .Include(b => b.Vehicle);
        }

        private static System.Linq.Expressions.Expression<Func<Booking, object>> ProjectBooking()
        {
            return b => new
            {
                bookingId = b.BId,
                customer = b.User != null ? b.User.Name : null,
                email = b.User != null ? b.User.Email : null,
                slotNumber = b.Slot != null ? b.Slot.SlotNum : null,
                vehicleNumber = b.Vehicle != null ? b.Vehicle.VNum : null,
                vehicleType = b.VType,
                startTime = b.StartTime,
                endTime = b.EndTime,
                totalAmount = b.TotalAmount,
                paymentStatus = b.PaymentStatus,
                bookingStatus = b.BStatus,
                bookingDate = b.BookingDate
            };
        }
    }
}
