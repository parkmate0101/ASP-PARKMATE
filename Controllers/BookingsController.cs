using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;
using ParkMate.ViewModels;
using ParkMate.Services;

namespace ParkMate.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ParkMateDbContext _context;
        private readonly ReceiptAccessService _receiptAccessService;

        private const string BookingDraftSessionKey =
            "ParkMateBookingDraft";

        private const string BookingSuccessSessionKey =
            "ParkMateBookingSuccessId";

        public BookingsController(
            ParkMateDbContext context,
            ReceiptAccessService receiptAccessService)
        {
            _context = context;
            _receiptAccessService = receiptAccessService;
        }

        // =========================================================
        // GET LOGGED-IN USER ID
        // =========================================================
        private int? GetLoggedInUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // =========================================================
        // UPDATE PAST ACTIVE BOOKINGS
        // =========================================================
        private async Task UpdatePastActiveBookingsAsync()
        {
            DateTime now = DateTime.Now;

            var pastActiveBookings =
                await _context.Bookings
                    .Where(b =>
                        b.BStatus == "Active" &&
                        b.EndTime <= now)
                    .ToListAsync();

            if (pastActiveBookings.Count == 0)
            {
                return;
            }

            foreach (var booking in pastActiveBookings)
            {
                booking.BStatus = "Completed";
            }

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // SLOT LETTER HELPER
        // =========================================================
        private static string GetSlotLetter(string slotNumber)
        {
            if (string.IsNullOrWhiteSpace(slotNumber))
            {
                return string.Empty;
            }

            return slotNumber.Substring(0, 1);
        }

        // =========================================================
        // SLOT NUMBER HELPER
        // =========================================================
        private static int GetSlotNumber(string slotNumber)
        {
            if (string.IsNullOrWhiteSpace(slotNumber))
            {
                return int.MaxValue;
            }

            if (slotNumber.Length <= 1)
            {
                return int.MaxValue;
            }

            return int.TryParse(
                slotNumber.Substring(1),
                out int number)
                ? number
                : int.MaxValue;
        }

        // =========================================================
        // ADMIN BOOKINGS
        // =========================================================
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate,
            int? slotId,
            string? status)
        {
            // -----------------------------------------------------
            // Automatically mark expired Active bookings
            // as Completed before displaying the admin list.
            // -----------------------------------------------------
            await UpdatePastActiveBookingsAsync();

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Slot)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(b =>
                    b.StartTime.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime nextDay =
                    toDate.Value.Date.AddDays(1);

                query = query.Where(b =>
                    b.StartTime < nextDay);
            }

            if (slotId.HasValue)
            {
                query = query.Where(b =>
                    b.SlotId == slotId.Value);
            }

            var allowedStatuses = new[]
            {
                "Active",
                "Completed",
                "Cancelled"
            };

            if (!string.IsNullOrWhiteSpace(status) &&
                allowedStatuses.Contains(status))
            {
                query = query.Where(b =>
                    b.BStatus == status);
            }

            var bookings = await query
                .OrderByDescending(b => b.BId)
                .ToListAsync();

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");

            ViewBag.SelectedSlotId =
                slotId;

            ViewBag.SelectedStatus =
                status;

            ViewBag.Slots =
                (await _context.Slots
                    .ToListAsync())
                .OrderBy(s => GetSlotLetter(s.SlotNum))
                .ThenBy(s => GetSlotNumber(s.SlotNum))
                .ToList();

            return View(bookings);
        }

        // =========================================================
        // CREATE / TIMESLOT - GET
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            var model = new BookingCreateViewModel
            {
                BookingType = "hourly",

                StartDate = DateTime.Today,

                EndDate = DateTime.Today,

                StartClock =
                    new TimeSpan(10, 0, 0),

                EndClock =
                    new TimeSpan(11, 0, 0)
            };

            return View(model);
        }

        // =========================================================
        // CREATE / TIMESLOT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BookingCreateViewModel model)
        {
            int? loggedInUserId =
                GetLoggedInUserId();

            model.VehicleNumber =
                (model.VehicleNumber ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            // -----------------------------------------------------
            // Vehicle number validation
            // -----------------------------------------------------
            if (!Regex.IsMatch(
                    model.VehicleNumber,
                    @"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$"))
            {
                ModelState.AddModelError(
                    nameof(model.VehicleNumber),
                    "Enter a valid vehicle number, e.g. GJ01AB1234.");
            }

            // -----------------------------------------------------
            // Vehicle type validation
            // -----------------------------------------------------
            if (model.VehicleType != "2_wheeler" &&
                model.VehicleType != "4_wheeler")
            {
                ModelState.AddModelError(
                    nameof(model.VehicleType),
                    "Please select a valid vehicle type.");
            }

            // -----------------------------------------------------
            // Booking type validation
            // -----------------------------------------------------
            if (model.BookingType != "hourly" &&
                model.BookingType != "full-day")
            {
                ModelState.AddModelError(
                    nameof(model.BookingType),
                    "Please select a valid booking type.");
            }

            DateTime startTime;
            DateTime endTime;

            // =====================================================
            // HOURLY BOOKING
            // =====================================================
            if (model.BookingType == "hourly")
            {
                startTime =
                    model.StartDate.Date
                    .Add(model.StartClock);

                endTime =
                    model.StartDate.Date
                    .Add(model.EndClock);

                if (model.StartClock <
                        new TimeSpan(10, 0, 0) ||
                    model.EndClock >
                        new TimeSpan(22, 0, 0))
                {
                    ModelState.AddModelError(
                        "",
                        "Hourly parking is allowed between 10:00 AM and 10:00 PM.");
                }

                if (endTime <= startTime)
                {
                    ModelState.AddModelError(
                        "",
                        "End time must be greater than start time.");
                }

                if (startTime <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        "",
                        "Booking start time must be in the future.");
                }
            }
            else
            {
                // =================================================
                // FULL-DAY BOOKING
                // =================================================
                if (model.EndDate.Date <
                    model.StartDate.Date)
                {
                    ModelState.AddModelError(
                        "",
                        "End date cannot be before start date.");
                }

                startTime =
                    model.StartDate.Date;

                endTime =
                    model.EndDate.Date
                    .AddDays(1)
                    .AddSeconds(-1);
            }

            if (model.StartDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "",
                    "Past dates are not allowed.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // =====================================================
            // PRICING
            // =====================================================
            var pricing =
                await _context.Pricings
                    .FirstOrDefaultAsync(p =>
                        p.VType == model.VehicleType);

            if (pricing == null)
            {
                ModelState.AddModelError(
                    "",
                    "Pricing is not configured for this vehicle type.");

                return View(model);
            }

            decimal totalAmount;

            if (model.BookingType == "full-day")
            {
                int days =
                    (model.EndDate.Date -
                     model.StartDate.Date).Days + 1;

                totalAmount =
                    days * pricing.PricePerDay;
            }
            else
            {
                double totalHours =
                    (endTime - startTime).TotalHours;

                totalAmount =
                    (decimal)Math.Ceiling(totalHours)
                    * pricing.PricePerHour;
            }

            // =====================================================
            // OPTIONAL LOGGED-IN USER
            // =====================================================
            // Book Now must work for guests too.
            // If the visitor is already logged in, we pre-fill
            // their contact details. Otherwise the draft keeps
            // UserId = null until Contact Details resolves the user.
            string email = string.Empty;
            string mobile = string.Empty;

            if (loggedInUserId.HasValue)
            {
                var user =
                    await _context.Users
                        .FirstOrDefaultAsync(u =>
                            u.UId == loggedInUserId.Value);

                if (user == null)
                {
                    HttpContext.Session.Clear();

                    loggedInUserId = null;
                }
                else
                {
                    email = user.Email ?? string.Empty;
                    mobile = user.Contact ?? string.Empty;
                }
            }

            // =====================================================
            // SAVE DRAFT IN SESSION
            // =====================================================
            var draft =
                new BookingDraftViewModel
                {
                    UserId =
                        loggedInUserId,

                    BookingType =
                        model.BookingType,

                    StartTime =
                        startTime,

                    EndTime =
                        endTime,

                    VehicleNumber =
                        model.VehicleNumber,

                    VehicleType =
                        model.VehicleType,

                    Email =
                        email,

                    Mobile =
                        mobile,

                    TotalAmount =
                        totalAmount
                };

            SaveDraft(draft);

            return RedirectToAction(
                nameof(SlotSelection));
        }

        // =========================================================
        // SLOT SELECTION - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> SlotSelection()
        {
            // Expired Active bookings must not keep slots blocked.
            await UpdatePastActiveBookingsAsync();

            var draft = GetDraft();

            if (draft == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var slots =
                (await _context.Slots
                    .Where(s =>
                        s.SlotType == draft.VehicleType &&
                        s.SlotStatus != "Blocked")
                    .ToListAsync())
                .OrderBy(s => GetSlotLetter(s.SlotNum))
                .ThenBy(s => GetSlotNumber(s.SlotNum))
                .ToList();

            var options =
                new List<BookingSlotOptionViewModel>();

            foreach (var slot in slots)
            {
                var firstConflict =
                    await _context.Bookings
                        .Where(b =>
                            b.SlotId == slot.SlotId &&
                            b.BStatus == "Active" &&
                            draft.StartTime < b.EndTime &&
                            draft.EndTime > b.StartTime)
                        .OrderBy(b => b.EndTime)
                        .FirstOrDefaultAsync();

                options.Add(
                    new BookingSlotOptionViewModel
                    {
                        SlotId = slot.SlotId,
                        SlotNumber = slot.SlotNum,
                        SlotType = slot.SlotType,
                        IsBooked = firstConflict != null,
                        NextAvailable = firstConflict?.EndTime
                    });
            }

            var model =
                new BookingSlotSelectionViewModel
                {
                    Draft = draft,
                    Slots = options
                };

            return View(model);
        }

        // =========================================================
        // SLOT SELECTION - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SlotSelection(
            int selectedSlotId)
        {
            await UpdatePastActiveBookingsAsync();

            var draft = GetDraft();

            if (draft == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var slot =
                await _context.Slots
                    .FirstOrDefaultAsync(s =>
                        s.SlotId == selectedSlotId);

            if (slot == null)
            {
                TempData["BookingError"] =
                    "Selected parking slot was not found.";

                return RedirectToAction(nameof(SlotSelection));
            }

            if (slot.SlotStatus == "Blocked")
            {
                TempData["BookingError"] =
                    "Selected parking slot is blocked.";

                return RedirectToAction(nameof(SlotSelection));
            }

            if (slot.SlotType != draft.VehicleType)
            {
                TempData["BookingError"] =
                    "Selected slot does not match the vehicle type.";

                return RedirectToAction(nameof(SlotSelection));
            }

            bool conflict =
                await _context.Bookings
                    .AnyAsync(b =>
                        b.SlotId == selectedSlotId &&
                        b.BStatus == "Active" &&
                        draft.StartTime < b.EndTime &&
                        draft.EndTime > b.StartTime);

            if (conflict)
            {
                TempData["BookingError"] =
                    "This slot is already booked for the selected time.";

                return RedirectToAction(nameof(SlotSelection));
            }

            draft.SlotId = slot.SlotId;
            draft.SlotNumber = slot.SlotNum;

            SaveDraft(draft);

            return RedirectToAction(nameof(Contact));
        }

        // =========================================================
        // CONTACT - GET
        // =========================================================
        [HttpGet]
        public IActionResult Contact()
        {
            var draft = GetDraft();

            if (draft == null || draft.SlotId == 0)
            {
                return RedirectToAction(nameof(SlotSelection));
            }

            var model =
                new BookingContactViewModel
                {
                    Email = draft.Email,
                    Mobile = draft.Mobile
                };

            return View(model);
        }

        // =========================================================
        // CONTACT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            BookingContactViewModel model)
        {
            var draft = GetDraft();

            if (draft == null || draft.SlotId == 0)
            {
                return RedirectToAction(nameof(Create));
            }

            model.Email =
                (model.Email ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            model.Mobile =
                (model.Mobile ?? string.Empty)
                .Trim();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            draft.Email = model.Email;
            draft.Mobile = model.Mobile;

            // Already logged in: keep the current account.
            var loggedInUserId = GetLoggedInUserId();

            if (loggedInUserId.HasValue)
            {
                draft.UserId = loggedInUserId.Value;
                SaveDraft(draft);

                return RedirectToAction(nameof(Confirmation));
            }

            // Guest: check email exactly at Contact Details.
            // Registered email -> continue booking.
            // New email -> Sign Up -> return to Contact.
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == model.Email);

            if (existingUser != null)
            {
                draft.UserId = existingUser.UId;
                SaveDraft(draft);

                return RedirectToAction(nameof(Confirmation));
            }

            draft.UserId = null;
            SaveDraft(draft);

            return RedirectToAction(
                "Register",
                "Auth",
                new
                {
                    returnUrl = Url.Action(
                        nameof(Contact),
                        "Bookings")
                });
        }

        // =========================================================
        // CONFIRMATION - GET
        // =========================================================
        [HttpGet]
        public IActionResult Confirmation()
        {
            var draft = GetDraft();

            if (draft == null ||
                draft.SlotId == 0 ||
                !draft.UserId.HasValue)
            {
                return RedirectToAction(nameof(Contact));
            }

            return View(draft);
        }

        // =========================================================
        // CONFIRMATION - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmation(
            string confirmBooking)
        {
            var draft = GetDraft();

            if (draft == null ||
                draft.SlotId == 0 ||
                !draft.UserId.HasValue)
            {
                return RedirectToAction(nameof(Contact));
            }

            return RedirectToAction(nameof(Payment));
        }

        // =========================================================
        // PAYMENT - GET
        // =========================================================
        [HttpGet]
        public IActionResult Payment()
        {
            var draft = GetDraft();

            if (draft == null ||
                draft.SlotId == 0 ||
                !draft.UserId.HasValue)
            {
                return RedirectToAction(nameof(Contact));
            }

            return View(draft);
        }

        // =========================================================
        // PAYMENT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(
            string paymentMode = "upi")
        {
            paymentMode = paymentMode?.ToLowerInvariant() == "card"
                ? "card"
                : "upi";
            int? loggedInUserId =
                GetLoggedInUserId();

            var draft = GetDraft();

            if (draft == null ||
                draft.SlotId == 0 ||
                !draft.UserId.HasValue)
            {
                return RedirectToAction(
                    nameof(Contact));
            }

            // If an account session exists, it must match the
            // booking draft. Guest bookings may continue because
            // Contact Details resolved the registered user ID.
            if (loggedInUserId.HasValue &&
                draft.UserId.Value != loggedInUserId.Value)
            {
                HttpContext.Session.Remove(
                    BookingDraftSessionKey);

                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            await UpdatePastActiveBookingsAsync();

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // RE-CHECK SLOT
                // -------------------------------------------------
                var slot =
                    await _context.Slots
                        .FirstOrDefaultAsync(s =>
                            s.SlotId == draft.SlotId);

                if (slot == null)
                {
                    throw new Exception(
                        "Selected parking slot was not found.");
                }

                if (slot.SlotStatus == "Blocked")
                {
                    throw new Exception(
                        "Selected parking slot is blocked.");
                }

                if (slot.SlotType != draft.VehicleType)
                {
                    throw new Exception(
                        "Selected slot does not match the vehicle type.");
                }

                bool conflict =
                    await _context.Bookings
                        .AnyAsync(b =>
                            b.SlotId == draft.SlotId &&
                            b.BStatus == "Active" &&
                            draft.StartTime < b.EndTime &&
                            draft.EndTime > b.StartTime);

                if (conflict)
                {
                    throw new Exception(
                        "Slot already booked. Please select another slot.");
                }

                // -------------------------------------------------
                // VEHICLE
                // -------------------------------------------------
                var vehicle =
                    await _context.Vehicles
                        .FirstOrDefaultAsync(v =>
                            v.UId == draft.UserId.Value &&
                            v.VNum == draft.VehicleNumber);

                if (vehicle == null)
                {
                    bool vehicleAlreadyExists =
                        await _context.Vehicles
                            .AnyAsync(v =>
                                v.VNum ==
                                draft.VehicleNumber);

                    if (vehicleAlreadyExists)
                    {
                        throw new Exception(
                            "This vehicle number already exists.");
                    }

                    vehicle =
                        new Vehicle
                        {
                            UId =
                                draft.UserId.Value,

                            VNum =
                                draft.VehicleNumber,

                            VType =
                                draft.VehicleType,

                            CreatedAt =
                                DateTime.Now
                        };

                    _context.Vehicles.Add(vehicle);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (vehicle.VType !=
                        draft.VehicleType)
                    {
                        throw new Exception(
                            "Vehicle type does not match the existing vehicle.");
                    }
                }

                // -------------------------------------------------
                // BOOKING
                // -------------------------------------------------
                var booking =
                    new Booking
                    {
                        UId =
                            draft.UserId.Value,

                        VId =
                            vehicle.VId,

                        VType =
                            draft.VehicleType,

                        SlotId =
                            draft.SlotId,

                        StartTime =
                            draft.StartTime,

                        EndTime =
                            draft.EndTime,

                        TotalAmount =
                            draft.TotalAmount,

                        PaymentStatus =
                            "paid",

                        BStatus =
                            "Active",

                        BookingDate =
                            DateTime.Now
                    };

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                // -------------------------------------------------
                // PAYMENT
                // -------------------------------------------------
                var payment =
                    new Payment
                    {
                        BId =
                            booking.BId,

                        PaidAmount =
                            draft.TotalAmount,

                        PaymentType =
                            "advance",

                        Mode =
                            paymentMode == "card"
                                ? "card"
                                : "upi",

                        PayStatus =
                            "paid",

                        PayDate =
                            DateTime.Now
                    };

                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                // -------------------------------------------------
                // RECEIPT
                // -------------------------------------------------
                var receipt =
                    new Receipt
                    {
                        PId =
                            payment.PId,

                        RecNo =
                            "REC"
                            + DateTime.Now
                                .ToString(
                                    "yyyyMMddHHmmssfff")
                            + Random.Shared.Next(
                                100,
                                999),

                        GeneratedAt =
                            DateTime.Now
                    };

                _context.Receipts.Add(receipt);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                HttpContext.Session.SetInt32(
                    BookingSuccessSessionKey,
                    booking.BId);

                return RedirectToAction(
                    nameof(PaymentSuccess));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["PaymentError"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Payment));
            }
        }

        // =========================================================
        // PAYMENT SUCCESS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess()
        {
            int? bookingId =
                HttpContext.Session.GetInt32(
                    BookingSuccessSessionKey);

            if (!bookingId.HasValue)
            {
                return RedirectToAction(
                    nameof(Create));
            }

            var booking =
                await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Slot)
                    .Include(b => b.Payments)
                        .ThenInclude(p => p.Receipts)
                    .FirstOrDefaultAsync(b =>
                        b.BId ==
                        bookingId.Value);

            if (booking == null)
            {
                return NotFound();
            }

            var payment =
                booking.Payments.FirstOrDefault();

            int bookingDisplayId = await _context.Bookings
                .CountAsync(b => b.BId <= booking.BId);

            ViewBag.BookingDisplayId = bookingDisplayId;

            if (payment != null)
            {
                ViewBag.PaymentDisplayId = await _context.Payments
                    .CountAsync(p => p.PId <= payment.PId);

                ViewBag.ReceiptDownloadToken =
                    _receiptAccessService.CreateToken(payment.PId);
            }

            HttpContext.Session.Remove(BookingSuccessSessionKey);

            ViewBag.ReceiptNumber =
                payment?
                    .Receipts
                    .FirstOrDefault()?
                    .RecNo;

            return View(booking);
        }

        // =========================================================
        // MY BOOKINGS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            int? loggedInUserId =
                GetLoggedInUserId();

            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            // Automatically update expired bookings first.
            await UpdatePastActiveBookingsAsync();

            var bookings =
                await _context.Bookings
                    .Include(b => b.Vehicle)
                    .Include(b => b.Slot)
                    .Include(b => b.Payments)
                    .Where(b =>
                        b.UId == loggedInUserId.Value)
                    .OrderByDescending(
                        b => b.BookingDate)
                    .ToListAsync();

            return View(bookings);
        }

        // =========================================================
        // DETAILS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(
            int? id)
        {
            int? loggedInUserId =
                GetLoggedInUserId();

            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            // Update expired Active bookings before
            // showing booking details.
            await UpdatePastActiveBookingsAsync();

            var booking =
                await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Slot)
                    .Include(b => b.Payments)
                        .ThenInclude(p => p.Receipts)
                    .FirstOrDefaultAsync(b =>
                        b.BId == id.Value &&
                        b.UId == loggedInUserId.Value);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.ReceiptNumber =
                booking.Payments
                    .SelectMany(p => p.Receipts)
                    .Select(r => r.RecNo)
                    .FirstOrDefault();

            return View(booking);
        }

        // =========================================================
        // CANCEL BOOKING
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id)
        {
            int? loggedInUserId =
                GetLoggedInUserId();

            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // Update old Active bookings first.
                await UpdatePastActiveBookingsAsync();

                var booking =
                    await _context.Bookings
                        .Include(b => b.Slot)
                        .FirstOrDefaultAsync(b =>
                            b.BId == id &&
                            b.UId == loggedInUserId.Value &&
                            b.BStatus == "Active");

                if (booking == null)
                {
                    TempData["BookingError"] =
                        "Invalid booking or booking is no longer active.";

                    return RedirectToAction(
                        nameof(MyBookings));
                }

                if (DateTime.Now >=
                    booking.StartTime)
                {
                    TempData["BookingError"] =
                        "Booking cannot be cancelled after the start time.";

                    return RedirectToAction(
                        nameof(MyBookings));
                }

                double hoursLeft =
                    (booking.StartTime -
                     DateTime.Now)
                    .TotalHours;

                decimal refundAmount = 0m;

                if (hoursLeft > 24)
                {
                    refundAmount =
                        booking.TotalAmount;
                }
                else if (hoursLeft > 12)
                {
                    refundAmount =
                        booking.TotalAmount *
                        0.5m;
                }

                booking.BStatus =
                    "Cancelled";

                booking.CancelledAt =
                    DateTime.Now;

                booking.CancelReason =
                    "User cancelled";

                booking.RefundAmount =
                    refundAmount;

                if (refundAmount > 0)
                {
                    var existingRefund =
                        await _context.Refunds
                            .FirstOrDefaultAsync(r =>
                                r.BId ==
                                booking.BId);

                    if (existingRefund == null)
                    {
                        var refund =
                            new Refund
                            {
                                BId =
                                    booking.BId,

                                UId =
                                    loggedInUserId.Value,

                                Amount =
                                    refundAmount,

                                RefundStatus =
                                    "pending",

                                CreatedAt =
                                    DateTime.Now
                            };

                        _context.Refunds.Add(
                            refund);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["BookingSuccess"] =
                    refundAmount > 0
                        ? $"Booking cancelled. Refund of ₹{refundAmount:0.00} is pending."
                        : "Booking cancelled. No refund is applicable.";

                return RedirectToAction(
                    nameof(MyBookings));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["BookingError"] =
                    "Cancellation failed. Please try again.";

                return RedirectToAction(
                    nameof(MyBookings));
            }
        }

        // =========================================================
        // SESSION HELPERS
        // =========================================================
        private void SaveDraft(
            BookingDraftViewModel draft)
        {
            string json =
                JsonSerializer.Serialize(draft);

            HttpContext.Session.SetString(
                BookingDraftSessionKey,
                json);
        }

        private BookingDraftViewModel? GetDraft()
        {
            string? json =
                HttpContext.Session.GetString(
                    BookingDraftSessionKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize
                <BookingDraftViewModel>(json);
        }
    }
}