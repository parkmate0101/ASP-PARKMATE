using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;

namespace ParkMate.Controllers
{
    [AdminOnly]
    public class SlotsController : Controller
    {
        private readonly ParkMateDbContext _context;

        public SlotsController(ParkMateDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // LIST ALL SLOTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var slots = await _context.Slots
                .OrderBy(s => s.SlotId)
                .ToListAsync();

            return View(slots);
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
        public async Task<IActionResult> Create(Slot slot)
        {
            if (ModelState.IsValid)
            {
                _context.Slots.Add(slot);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(slot);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slot =
                await _context.Slots.FindAsync(id);

            if (slot == null)
            {
                return NotFound();
            }

            return View(slot);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Slot slot)
        {
            if (id != slot.SlotId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(slot);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(slot);
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slot =
                await _context.Slots
                    .FirstOrDefaultAsync(
                        s => s.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            return View(slot);
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
            var slot =
                await _context.Slots.FindAsync(id);

            if (slot != null)
            {
                _context.Slots.Remove(slot);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // TOGGLE SLOT STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id)
        {
            var slot =
                await _context.Slots.FindAsync(id);

            if (slot == null)
            {
                return NotFound();
            }

            if (slot.SlotStatus == "Available")
            {
                slot.SlotStatus = "Blocked";
            }
            else
            {
                slot.SlotStatus = "Available";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}