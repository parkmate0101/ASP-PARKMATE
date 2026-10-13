using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Models;
using System.Diagnostics;

namespace ParkMate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ParkMateDbContext _context;

        public HomeController(ParkMateDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pricing = await _context.Pricings
                .OrderBy(p => p.VType)
                .ToListAsync();

            return View(pricing);
        }

        [HttpGet]
        public IActionResult BookNow()
        {
            // Book Now always starts the booking flow.
            // Registration/login is decided later at Contact Details.
            return RedirectToAction("Create", "Bookings");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}
