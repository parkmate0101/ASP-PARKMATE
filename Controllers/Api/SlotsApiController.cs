using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;
using ParkMate.ViewModels.Api;

namespace ParkMate.Controllers.Api
{
    [ApiController]
    [Route("api/slots")]
    public class SlotsApiController : ControllerBase
    {
        private readonly ParkMateDbContext _context;

        public SlotsApiController(ParkMateDbContext context)
        {
            _context = context;
        }

        // GET: /api/slots
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var slots = await _context.Slots
                .AsNoTracking()
                .OrderBy(s => s.SlotNum)
                .Select(s => new
                {
                    slotId = s.SlotId,
                    slotNumber = s.SlotNum,
                    slotType = s.SlotType,
                    status = s.SlotStatus
                })
                .ToListAsync();

            return Ok(new { success = true, count = slots.Count, data = slots });
        }

        // GET: /api/slots/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var slot = await _context.Slots
                .AsNoTracking()
                .Where(s => s.SlotId == id)
                .Select(s => new
                {
                    slotId = s.SlotId,
                    slotNumber = s.SlotNum,
                    slotType = s.SlotType,
                    status = s.SlotStatus
                })
                .FirstOrDefaultAsync();

            if (slot == null)
                return NotFound(new { success = false, message = "Parking slot not found." });

            return Ok(new { success = true, data = slot });
        }

        // POST: /api/slots
        [HttpPost]
        [ApiAdminOnly]
        public async Task<IActionResult> Create(SlotApiRequest request)
        {
            var validation = ValidateSlotRequest(request);
            if (validation != null)
                return validation;

            string slotNumber = request.SlotNum.Trim().ToUpperInvariant();

            if (await _context.Slots.AnyAsync(s => s.SlotNum == slotNumber))
                return Conflict(new { success = false, message = "A slot with this slot number already exists." });

            var slot = new Slot
            {
                SlotNum = slotNumber,
                SlotType = request.SlotType,
                SlotStatus = request.SlotStatus
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = slot.SlotId }, new
            {
                success = true,
                message = "Parking slot created successfully.",
                data = new
                {
                    slotId = slot.SlotId,
                    slotNumber = slot.SlotNum,
                    slotType = slot.SlotType,
                    status = slot.SlotStatus
                }
            });
        }

        // PUT: /api/slots/1
        [HttpPut("{id:int}")]
        [ApiAdminOnly]
        public async Task<IActionResult> Update(int id, SlotApiRequest request)
        {
            var validation = ValidateSlotRequest(request);
            if (validation != null)
                return validation;

            var slot = await _context.Slots.FindAsync(id);
            if (slot == null)
                return NotFound(new { success = false, message = "Parking slot not found." });

            string slotNumber = request.SlotNum.Trim().ToUpperInvariant();

            bool duplicate = await _context.Slots.AnyAsync(s =>
                s.SlotId != id && s.SlotNum == slotNumber);

            if (duplicate)
                return Conflict(new { success = false, message = "Another slot already uses this slot number." });

            slot.SlotNum = slotNumber;
            slot.SlotType = request.SlotType;
            slot.SlotStatus = request.SlotStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Parking slot updated successfully.",
                data = new
                {
                    slotId = slot.SlotId,
                    slotNumber = slot.SlotNum,
                    slotType = slot.SlotType,
                    status = slot.SlotStatus
                }
            });
        }

        // DELETE: /api/slots/1
        [HttpDelete("{id:int}")]
        [ApiAdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _context.Slots
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null)
                return NotFound(new { success = false, message = "Parking slot not found." });

            if (slot.Bookings.Any())
            {
                return Conflict(new
                {
                    success = false,
                    message = "This slot cannot be deleted because booking records exist for it. Block the slot instead."
                });
            }

            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Parking slot deleted successfully." });
        }

        private static IActionResult? ValidateSlotRequest(SlotApiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SlotNum))
                return new BadRequestObjectResult(new { success = false, message = "Slot number is required." });

            if (request.SlotType != "2_wheeler" && request.SlotType != "4_wheeler")
                return new BadRequestObjectResult(new { success = false, message = "SlotType must be 2_wheeler or 4_wheeler." });

            if (request.SlotStatus != "Available" && request.SlotStatus != "Blocked")
                return new BadRequestObjectResult(new { success = false, message = "SlotStatus must be Available or Blocked." });

            return null;
        }
    }
}
