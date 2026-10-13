using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Filters;

namespace ParkMate.Controllers
{
    [AdminOnly]
    public class UsersController : Controller
    {
        private readonly ParkMateDbContext _context;

        public UsersController(
            ParkMateDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // USER LIST + SEARCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search)
        {
            var users =
                _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Contact.Contains(search));
            }

            ViewData["Search"] =
                search;

            var result =
                await users
                    .OrderBy(u => u.UId)
                    .ToListAsync();

            return View(result);
        }

        // =========================================================
        // USER DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user =
                await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(
                        u => u.UId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}