using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;

namespace ParkMate.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ParkMateDbContext _context;

        public DashboardController(ParkMateDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // MAIN DASHBOARD REDIRECT
        // ============================================================
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            int? role = HttpContext.Session.GetInt32("UserRole");

            if (role == 1)
            {
                return RedirectToAction("Admin");
            }

            return RedirectToAction("User");
        }


        // ============================================================
        // USER DASHBOARD
        // ============================================================
        public async Task<IActionResult> User()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            // Not logged in
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Admin cannot access User Dashboard
            if (role == 1)
            {
                return RedirectToAction("Admin");
            }

            // Only normal users can access this dashboard
            if (role != 2)
            {
                return RedirectToAction("Login", "Auth");
            }

            int bookingCount = await _context.Bookings
                .CountAsync(b => b.UId == userId.Value);

            ViewBag.BookingCount = bookingCount;

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "User";

            return View();
        }


        // ============================================================
        // ADMIN DASHBOARD
        // ============================================================
        public async Task<IActionResult> Admin(
            string? filterOption,
            DateTime? startDate,
            DateTime? endDate)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? role = HttpContext.Session.GetInt32("UserRole");

            // --------------------------------------------------------
            // LOGIN CHECK
            // --------------------------------------------------------
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // --------------------------------------------------------
            // ADMIN ROLE CHECK
            // --------------------------------------------------------
            if (role != 1)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "Admin";

            // --------------------------------------------------------
            // AUTOMATICALLY COMPLETE EXPIRED ACTIVE BOOKINGS
            // --------------------------------------------------------
            // This keeps dashboard counts and slot availability
            // consistent when a booking's end time has passed.
            var expiredBookings = await _context.Bookings
                .Where(b =>
                    b.BStatus == "Active" &&
                    b.EndTime <= DateTime.Now)
                .ToListAsync();

            if (expiredBookings.Count > 0)
            {
                foreach (var booking in expiredBookings)
                {
                    booking.BStatus = "Completed";
                }

                await _context.SaveChangesAsync();
            }


            // ========================================================
            // FILTER
            // ========================================================

            if (string.IsNullOrWhiteSpace(filterOption))
            {
                filterOption = "select";
            }

            DateTime? filterStart = null;
            DateTime? filterEnd = null;

            DateTime today = DateTime.Today;

            switch (filterOption.ToLower())
            {
                case "today":

                    filterStart = today;
                    filterEnd = today.AddDays(1);

                    break;


                case "last5":

                    filterStart = today.AddDays(-4);
                    filterEnd = today.AddDays(1);

                    break;


                case "month":

                    filterStart = new DateTime(
                        today.Year,
                        today.Month,
                        1);

                    filterEnd = filterStart.Value.AddMonths(1);

                    break;


                case "total":

                    filterStart = null;
                    filterEnd = null;

                    break;


                case "select":

                    // No filter
                    filterStart = null;
                    filterEnd = null;

                    break;
            }


            // ========================================================
            // CUSTOM DATE RANGE
            // ========================================================

            if (startDate.HasValue || endDate.HasValue)
            {
                if (startDate.HasValue)
                {
                    filterStart = startDate.Value.Date;
                }

                if (endDate.HasValue)
                {
                    filterEnd = endDate.Value.Date.AddDays(1);
                }
            }


            // ========================================================
            // BASE BOOKING QUERY
            // ========================================================

            IQueryable<Models.Booking> bookingQuery =
                _context.Bookings;


            if (filterStart.HasValue)
            {
                bookingQuery = bookingQuery.Where(
                    b => b.BookingDate >= filterStart.Value);
            }

            if (filterEnd.HasValue)
            {
                bookingQuery = bookingQuery.Where(
                    b => b.BookingDate < filterEnd.Value);
            }


            // ========================================================
            // TOTAL BOOKINGS
            // ========================================================

            int totalBookings =
                await bookingQuery.CountAsync();


            // ========================================================
            // ACTIVE BOOKINGS
            // ========================================================

            int activeBookings =
                await bookingQuery
                    .CountAsync(b => b.BStatus == "Active");


            // ========================================================
            // TOTAL SLOTS
            // ========================================================

            int totalSlots =
                await _context.Slots.CountAsync();


            // ========================================================
            // CURRENTLY BOOKED SLOTS
            //
            // Original ParkMate logic:
            // Active booking + current time between start/end
            // ========================================================

            DateTime now = DateTime.Now;

            int bookedSlots =
                await _context.Bookings
                    .Where(b =>
                        b.BStatus == "Active" &&
                        b.StartTime <= now &&
                        b.EndTime >= now)
                    .Select(b => b.SlotId)
                    .Distinct()
                    .CountAsync();


            // ========================================================
            // AVAILABLE SLOTS
            // ========================================================

            int availableSlots =
                totalSlots - bookedSlots;

            if (availableSlots < 0)
            {
                availableSlots = 0;
            }


            // ========================================================
            // REGISTERED USERS
            // ========================================================

            int totalUsers =
                await _context.Users.CountAsync();


            // ========================================================
            // TOTAL PAID REVENUE
            //
            // Paid payments only.
            // Processed refunds are deducted.
            // ========================================================

            var paidPaymentQuery =
                from payment in _context.Payments
                join booking in _context.Bookings
                    on payment.BId equals booking.BId
                where payment.PayStatus == "paid"
                select new
                {
                    Payment = payment,
                    Booking = booking
                };


            if (filterStart.HasValue)
            {
                paidPaymentQuery =
                    paidPaymentQuery.Where(x =>
                        x.Booking.BookingDate >= filterStart.Value);
            }

            if (filterEnd.HasValue)
            {
                paidPaymentQuery =
                    paidPaymentQuery.Where(x =>
                        x.Booking.BookingDate < filterEnd.Value);
            }


            decimal paidRevenue =
                await paidPaymentQuery
                    .Select(x => (decimal?)x.Payment.PaidAmount)
                    .SumAsync() ?? 0m;


            // ========================================================
            // PROCESSED REFUNDS
            // ========================================================

            var processedRefundQuery =
                from refund in _context.Refunds
                join booking in _context.Bookings
                    on refund.BId equals booking.BId
                where refund.RefundStatus == "processed"
                select new
                {
                    Refund = refund,
                    Booking = booking
                };


            if (filterStart.HasValue)
            {
                processedRefundQuery =
                    processedRefundQuery.Where(x =>
                        x.Booking.BookingDate >= filterStart.Value);
            }

            if (filterEnd.HasValue)
            {
                processedRefundQuery =
                    processedRefundQuery.Where(x =>
                        x.Booking.BookingDate < filterEnd.Value);
            }


            decimal processedRefunds =
                await processedRefundQuery
                    .Select(x => (decimal?)x.Refund.Amount)
                    .SumAsync() ?? 0m;


            // ========================================================
            // FINAL REVENUE
            // ========================================================

            decimal revenue =
                paidRevenue - processedRefunds;


            if (revenue < 0)
            {
                revenue = 0;
            }


            // ========================================================
            // EXPECTED REVENUE
            //
            // Paid + Active bookings
            // ========================================================

            var expectedRevenueQuery =
                from payment in _context.Payments
                join booking in _context.Bookings
                    on payment.BId equals booking.BId
                where payment.PayStatus == "paid"
                      && booking.BStatus == "Active"
                select new
                {
                    Payment = payment,
                    Booking = booking
                };


            if (filterStart.HasValue)
            {
                expectedRevenueQuery =
                    expectedRevenueQuery.Where(x =>
                        x.Booking.BookingDate >= filterStart.Value);
            }

            if (filterEnd.HasValue)
            {
                expectedRevenueQuery =
                    expectedRevenueQuery.Where(x =>
                        x.Booking.BookingDate < filterEnd.Value);
            }


            decimal expectedRevenue =
                await expectedRevenueQuery
                    .Select(x => (decimal?)x.Payment.PaidAmount)
                    .SumAsync() ?? 0m;

            // ========================================================
            // TOTAL PAYMENTS
            // ========================================================

            var paymentCountQuery =
                from payment in _context.Payments
                join booking in _context.Bookings
                    on payment.BId equals booking.BId
                where payment.PayStatus == "paid"
                select new
                {
                    Payment = payment,
                    Booking = booking
                };

            if (filterStart.HasValue)
            {
                paymentCountQuery =
                    paymentCountQuery.Where(x =>
                        x.Booking.BookingDate >= filterStart.Value);
            }

            if (filterEnd.HasValue)
            {
                paymentCountQuery =
                    paymentCountQuery.Where(x =>
                        x.Booking.BookingDate < filterEnd.Value);
            }

            int paymentCount =
                await paymentCountQuery.CountAsync();

            // ========================================================
            // AVERAGE RATING
            // ========================================================

            double averageRating =
                await _context.Feedbacks
                    .Select(f => (double?)f.Rating)
                    .AverageAsync() ?? 0;


            averageRating =
                Math.Round(averageRating, 1);


            // ========================================================
            // RATING STARS
            // ========================================================

            int fullStars =
                (int)Math.Round(
                    averageRating,
                    MidpointRounding.AwayFromZero);

            if (fullStars < 0)
            {
                fullStars = 0;
            }

            if (fullStars > 5)
            {
                fullStars = 5;
            }

            string ratingStars =
                new string('★', fullStars) +
                new string('☆', 5 - fullStars);


            // ========================================================
            // SEND DATA TO VIEW
            // ========================================================

            ViewBag.FilterOption = filterOption;

            ViewBag.StartDate =
                startDate?.ToString("yyyy-MM-dd");

            ViewBag.EndDate =
                endDate?.ToString("yyyy-MM-dd");

            ViewBag.Revenue = revenue;

            ViewBag.ExpectedRevenue =
                expectedRevenue;

            ViewBag.TotalUsers =
                totalUsers;

            ViewBag.BookedSlots =
                bookedSlots;

            ViewBag.AvailableSlots =
                availableSlots;

            ViewBag.TotalSlots =
                totalSlots;

            ViewBag.ActiveBookings =
                activeBookings;

            ViewBag.TotalBookings =
                totalBookings;

            ViewBag.PaymentCount =
                paymentCount;

            ViewBag.AverageRating =
                averageRating;

            ViewBag.RatingStars =
                ratingStars;


            return View();
        }
    }
}