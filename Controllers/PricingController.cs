using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;

namespace ParkMate.Controllers
{
    [AdminOnly]
    public class PricingController : Controller
    {
        private readonly ParkMateDbContext _context;

        public PricingController(ParkMateDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pricing =
                await _context.Pricings
                    .OrderBy(p => p.PriceId)
                    .ToListAsync();

            return View(pricing);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Pricing pricing)
        {
            bool alreadyExists =
                await _context.Pricings
                    .AnyAsync(p =>
                        p.VType == pricing.VType);

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    "VType",
                    "Pricing for this vehicle type already exists.");
            }

            if (ModelState.IsValid)
            {
                pricing.UpdatedAt =
                    DateTime.Now;

                _context.Pricings.Add(pricing);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(pricing);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pricing =
                await _context.Pricings
                    .FindAsync(id);

            if (pricing == null)
            {
                return NotFound();
            }

            return View(pricing);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Pricing pricing)
        {
            if (id != pricing.PriceId)
            {
                return NotFound();
            }

            bool alreadyExists =
                await _context.Pricings
                    .AnyAsync(p =>
                        p.VType == pricing.VType &&
                        p.PriceId != pricing.PriceId);

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    "VType",
                    "Pricing for this vehicle type already exists.");
            }

            if (ModelState.IsValid)
            {
                pricing.UpdatedAt =
                    DateTime.Now;

                _context.Pricings.Update(pricing);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(pricing);
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pricing =
                await _context.Pricings
                    .FirstOrDefaultAsync(
                        p => p.PriceId == id);

            if (pricing == null)
            {
                return NotFound();
            }

            return View(pricing);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var pricing =
                await _context.Pricings
                    .FindAsync(id);

            if (pricing != null)
            {
                _context.Pricings.Remove(pricing);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}