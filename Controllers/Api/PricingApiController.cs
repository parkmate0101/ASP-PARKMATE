using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;
using ParkMate.Models;
using ParkMate.ViewModels.Api;

namespace ParkMate.Controllers.Api
{
    [ApiController]
    [Route("api/pricing")]
    public class PricingApiController : ControllerBase
    {
        private readonly ParkMateDbContext _context;

        public PricingApiController(ParkMateDbContext context)
        {
            _context = context;
        }

        // GET: /api/pricing
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pricing = await _context.Pricings
                .AsNoTracking()
                .OrderBy(p => p.PriceId)
                .Select(p => new
                {
                    priceId = p.PriceId,
                    vehicleType = p.VType,
                    pricePerHour = p.PricePerHour,
                    pricePerDay = p.PricePerDay,
                    updatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, count = pricing.Count, data = pricing });
        }

        // GET: /api/pricing/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pricing = await _context.Pricings
                .AsNoTracking()
                .Where(p => p.PriceId == id)
                .Select(p => new
                {
                    priceId = p.PriceId,
                    vehicleType = p.VType,
                    pricePerHour = p.PricePerHour,
                    pricePerDay = p.PricePerDay,
                    updatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (pricing == null)
                return NotFound(new { success = false, message = "Pricing record not found." });

            return Ok(new { success = true, data = pricing });
        }

        // POST: /api/pricing
        [HttpPost]
        [ApiAdminOnly]
        public async Task<IActionResult> Create(PricingApiRequest request)
        {
            var validation = ValidateRequest(request);
            if (validation != null)
                return validation;

            string type = request.VType.Trim();

            if (await _context.Pricings.AnyAsync(p => p.VType == type))
                return Conflict(new { success = false, message = "Pricing for this vehicle type already exists." });

            var pricing = new Pricing
            {
                VType = type,
                PricePerHour = request.PricePerHour,
                PricePerDay = request.PricePerDay,
                UpdatedAt = DateTime.Now
            };

            _context.Pricings.Add(pricing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = pricing.PriceId }, new
            {
                success = true,
                message = "Pricing created successfully.",
                data = new
                {
                    priceId = pricing.PriceId,
                    vehicleType = pricing.VType,
                    pricePerHour = pricing.PricePerHour,
                    pricePerDay = pricing.PricePerDay,
                    updatedAt = pricing.UpdatedAt
                }
            });
        }

        // PUT: /api/pricing/1
        [HttpPut("{id:int}")]
        [ApiAdminOnly]
        public async Task<IActionResult> Update(int id, PricingApiRequest request)
        {
            var validation = ValidateRequest(request);
            if (validation != null)
                return validation;

            var pricing = await _context.Pricings.FindAsync(id);
            if (pricing == null)
                return NotFound(new { success = false, message = "Pricing record not found." });

            string type = request.VType.Trim();

            bool duplicate = await _context.Pricings.AnyAsync(p =>
                p.PriceId != id && p.VType == type);

            if (duplicate)
                return Conflict(new { success = false, message = "Another pricing record already exists for this vehicle type." });

            pricing.VType = type;
            pricing.PricePerHour = request.PricePerHour;
            pricing.PricePerDay = request.PricePerDay;
            pricing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Pricing updated successfully.",
                data = new
                {
                    priceId = pricing.PriceId,
                    vehicleType = pricing.VType,
                    pricePerHour = pricing.PricePerHour,
                    pricePerDay = pricing.PricePerDay,
                    updatedAt = pricing.UpdatedAt
                }
            });
        }

        // DELETE: /api/pricing/1
        [HttpDelete("{id:int}")]
        [ApiAdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var pricing = await _context.Pricings.FindAsync(id);
            if (pricing == null)
                return NotFound(new { success = false, message = "Pricing record not found." });

            _context.Pricings.Remove(pricing);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Pricing deleted successfully." });
        }

        private static IActionResult? ValidateRequest(PricingApiRequest request)
        {
            if (request.VType != "2_wheeler" && request.VType != "4_wheeler")
                return new BadRequestObjectResult(new { success = false, message = "VType must be 2_wheeler or 4_wheeler." });

            if (request.PricePerHour < 0 || request.PricePerDay < 0)
                return new BadRequestObjectResult(new { success = false, message = "Prices cannot be negative." });

            return null;
        }
    }
}
